using Domain.Facades.Interfaces;
using Domain.Providers;
using Domain.Providers.Campaigns.Interfaces;
using Domain.Serializers.Interfaces;
using Domain.Services.Interfaces;
using Domain.Supervisor;
using Hangfire;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns.Phases;
using Leadsly.Application.Model.RabbitMQ;
using Leadsly.Application.Model.Responses;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services
{
    public class CampaignManagerService : ICampaignManagerService
    {
        public CampaignManagerService(ILogger<CampaignManagerService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        private readonly ILogger<CampaignManagerService> _logger;
        private readonly IServiceProvider _serviceProvider;
        /// <summary>
        /// Describes the phases that depend on each other. This means that only one phase at a time can be executed.
        /// This applies to ProspectList phase and SendConnectionsList phase. ProspectList must complete first before any of
        /// the other phases can be triggered.
        /// </summary>
        private readonly ConcurrentDictionary<string, RabbitMQMessageProperties> MessageProperties_Sync = new();
        
        /// <summary>
        /// MessageProperties_ConstantPhases describes phases that can be run at any time. That includes MonitorForNewAcceptedConnections phase,
        /// ScanProspectsForReplies phase and FollowUpMessageBrowser phase. The first two phases are executed at the beginning of the day and run until
        /// end of day. FollowUpMessage phase is executed async and gets triggered when a new connection is received.
        /// </summary>
        private readonly ConcurrentDictionary<string, RabbitMQMessageProperties> MessageProperties_ConstantPhases = new();
        private readonly ConcurrentDictionary<string, RabbitMQMessageProperties> MessageProperties_FollowUpMessagePhases = new();
        private ConcurrentQueue<Action> ExecutePhasesChain = new();
               

        ~CampaignManagerService()
        {

        }

        private void NextPhase()
        {
            if (ExecutePhasesChain.Count > 0)
            {
                ExecutePhasesChain.TryDequeue(out Action nextPhase);
                nextPhase();
            }
            else
            {
                _logger.LogInformation("There are no more queued jobs to run");
            }
        }

        #region FollowUpMessages

        public void OnFollowUpMessageEventReceived(object sender, BasicDeliverEventArgs eventArgs)
        {
            IModel channel = ((EventingBasicConsumer)sender).Model;
            string messageId = eventArgs.BasicProperties.MessageId;

            // this can be started asap and doesn't rely on any other event            
            // execute send follow up message logic
            if (MessageProperties_FollowUpMessagePhases.Keys.Any())
            {
                // queue up this phase for later
                ExecutePhasesChain.Enqueue(() => QueueFollowUpMessages(messageId, channel, eventArgs));                
            }
            else
            {
                QueueFollowUpMessages(messageId, channel, eventArgs);
            }
        }

        private void QueueFollowUpMessages(string messageId, IModel channel, BasicDeliverEventArgs eventArgs)
        {
            MessageProperties_FollowUpMessagePhases.TryAdd(messageId, new()
            {
                BasicDeliveryEventArgs = eventArgs,
                Channel = channel
            });

            BackgroundJob.Enqueue<ICampaignManagerService>((x) => x.StartFollowUpMessages(messageId));
        }

        public void StartFollowUpMessages(string messageId)
        {
            MessageProperties_FollowUpMessagePhases.TryGetValue(messageId, out RabbitMQMessageProperties props);
            BasicDeliverEventArgs eventArgs = props.BasicDeliveryEventArgs;
            IModel channel = props.Channel;

            using (var scope = _serviceProvider.CreateScope())
            {
                //try and deserialize the response
                ICampaignPhaseSerializer serializer = scope.ServiceProvider.GetRequiredService<ICampaignPhaseSerializer>();
                byte[] body = eventArgs.Body.ToArray();
                string message = Encoding.UTF8.GetString(body);
                FollowUpMessagesBody followUpMessages = serializer.DeserializeFollowUpMessagesBody(message);
                Action ackOperation = default;
                try
                {
                    ICampaignPhaseFacade campaignPhaseFacade = scope.ServiceProvider.GetRequiredService<ICampaignPhaseFacade>();
                    HalOperationResult<IOperationResponse> operationResult = campaignPhaseFacade.ExecutePhase<IOperationResponse>(followUpMessages);

                    if (operationResult.Succeeded == true)
                    {
                        _logger.LogInformation("ExecuteFollowUpMessagesPhase executed successfully. Acknowledging message");
                        ackOperation = () => channel.BasicAck(eventArgs.DeliveryTag, false);
                    }
                    else
                    {
                        _logger.LogWarning("Executing Follow Up Messages Phase did not successfully succeeded. Negatively acknowledging the message and re-queuing it");
                        ackOperation = () => channel.BasicNack(eventArgs.DeliveryTag, false, true);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception occured while executing Follow Up Messages Phase. Negatively acknowledging the message and re-queuing it");
                    ackOperation = () => channel.BasicNack(eventArgs.DeliveryTag, false, true);
                }
                finally
                {
                    MessageProperties_FollowUpMessagePhases.TryRemove(messageId, out _);
                    NextPhase();
                    ackOperation();
                }
            }
        }

        #endregion

        #region ConnectionWithdraw

        public void OnConnectionWithdrawEventReceived(object sender, BasicDeliverEventArgs eventArgs)
        {
            IModel channel = ((EventingBasicConsumer)sender).Model;
            string messageId = eventArgs.BasicProperties.MessageId;

            // this can be started asap and doesn't rely on any other event            
            // execute send follow up message logic
            if (MessageProperties_Sync.Keys.Any())
            {
                // queue up this phase for later
                ExecutePhasesChain.Enqueue(() => QueueConnectionWithdraw(messageId, channel, eventArgs));
            }
            else
            {
                QueueConnectionWithdraw(messageId, channel, eventArgs);
            }
        }

        private void QueueConnectionWithdraw(string messageId, IModel channel, BasicDeliverEventArgs eventArgs)
        {
            MessageProperties_Sync.TryAdd(messageId, new()
            {
                BasicDeliveryEventArgs = eventArgs,
                Channel = channel
            });

            BackgroundJob.Enqueue<ICampaignManagerService>((x) => x.StartConnectionWithdraw(messageId));
        }

        public void StartConnectionWithdraw(string messageId)
        {
            MessageProperties_Sync.TryGetValue(messageId, out RabbitMQMessageProperties props);
            BasicDeliverEventArgs eventArgs = props.BasicDeliveryEventArgs;
            IModel channel = props.Channel;

            using (var scope = _serviceProvider.CreateScope())
            {
                //try and deserialize the response
                ICampaignPhaseSerializer serializer = scope.ServiceProvider.GetRequiredService<ICampaignPhaseSerializer>();
                byte[] body = eventArgs.Body.ToArray();
                string message = Encoding.UTF8.GetString(body);
                ConnectionWithdrawBody connectionWithdraw = serializer.DeserializeConnectionWithdrawBody(message);
                Action ackOperation = default;
                try
                {
                    ICampaignPhaseFacade campaignPhaseFacade = scope.ServiceProvider.GetRequiredService<ICampaignPhaseFacade>();
                    HalOperationResult<IOperationResponse> operationResult = campaignPhaseFacade.ExecutePhase<IOperationResponse>(connectionWithdraw);

                    if (operationResult.Succeeded == true)
                    {
                        _logger.LogInformation("ExecuteFollowUpMessagesPhase executed successfully. Acknowledging message");
                        ackOperation = () => channel.BasicAck(eventArgs.DeliveryTag, false);
                    }
                    else
                    {
                        _logger.LogWarning("Executing Follow Up Messages Phase did not successfully succeeded. Negatively acknowledging the message and re-queuing it");
                        ackOperation = () => channel.BasicNack(eventArgs.DeliveryTag, false, true);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception occured while executing Follow Up Messages Phase. Negatively acknowledging the message and re-queuing it");
                    ackOperation = () => channel.BasicNack(eventArgs.DeliveryTag, false, true);
                }
                finally
                {
                    MessageProperties_Sync.TryRemove(messageId, out _);
                    NextPhase();
                    ackOperation();
                }
            }
        }

        #endregion

        #region MonitorForNewAcceptedConnections

        public void OnMonitorForNewAcceptedConnectionsEventReceived(object sender, BasicDeliverEventArgs eventArgs)
        {
            // this can be started asap and doesn't rely on any other event
            // execute monitor for new accepted connections logic
            IModel channel = ((EventingBasicConsumer)sender).Model;
            string messageId = eventArgs.BasicProperties.MessageId;
            //if (MessageProperties_ConstantPhases.Keys.Any())
            //{
            //    // queue up this phase for later
            //    ExecutePhasesChain.Enqueue(() => QueueMonitorForNewAcceptedConnections(messageId, channel, eventArgs));
            //}
            //else
            //{
            //    QueueMonitorForNewAcceptedConnections(messageId, channel, eventArgs);
            //}
            QueueMonitorForNewAcceptedConnections(messageId, channel, eventArgs);
        }

        private void QueueMonitorForNewAcceptedConnections(string messageId, IModel channel, BasicDeliverEventArgs eventArgs)
        {
            MessageProperties_ConstantPhases.TryAdd(messageId, new()
            {
                BasicDeliveryEventArgs = eventArgs,
                Channel = channel
            });

            BackgroundJob.Enqueue<ICampaignManagerService>((x) => x.StartMonitorForNewConnections(messageId));
        }

        public async Task StartMonitorForNewConnections(string messageId)
        {
            MessageProperties_ConstantPhases.TryGetValue(messageId, out RabbitMQMessageProperties props);
            BasicDeliverEventArgs eventArgs = props.BasicDeliveryEventArgs;
            IModel channel = props.Channel;

            using (var scope = _serviceProvider.CreateScope())
            {
                //try and deserialize the response
                ICampaignPhaseSerializer serializer = scope.ServiceProvider.GetRequiredService<ICampaignPhaseSerializer>();
                byte[] body = eventArgs.Body.ToArray();
                string message = Encoding.UTF8.GetString(body);
                MonitorForNewAcceptedConnectionsBody monitorForNewAcceptedConnections = serializer.DeserializeMonitorForNewAcceptedConnectionsBody(message);
                Action ackOperation = default;
                try
                {
                    ICampaignPhaseFacade campaignPhaseFacade = scope.ServiceProvider.GetRequiredService<ICampaignPhaseFacade>();
                    HalOperationResult<IOperationResponse> operationResult = await campaignPhaseFacade.ExecutePhase<IOperationResponse>(monitorForNewAcceptedConnections);

                    if (operationResult.Succeeded == true)
                    {
                        _logger.LogInformation("ExecuteFollowUpMessagesPhase executed successfully. Acknowledging message");
                        ackOperation = () => channel.BasicAck(eventArgs.DeliveryTag, false);
                    }
                    else
                    {
                        _logger.LogWarning("Executing Follow Up Messages Phase did not successfully succeeded. Negatively acknowledging the message and re-queuing it");
                        ackOperation = () => channel.BasicNack(eventArgs.DeliveryTag, false, true);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception occured while executing Follow Up Messages Phase. Negatively acknowledging the message and re-queuing it");
                    ackOperation = () => channel.BasicNack(eventArgs.DeliveryTag, false, true);
                }
                finally
                {
                    MessageProperties_ConstantPhases.TryRemove(messageId, out _);
                    NextPhase();
                    ackOperation();
                }
            }
        }

        #endregion

        #region ProspectList

        public void OnProspectListEventReceived(object sender, BasicDeliverEventArgs eventArgs)
        {
            IModel channel = ((EventingBasicConsumer)sender).Model;
            string messageId = eventArgs.BasicProperties.MessageId;
            if (MessageProperties_Sync.Keys.Any())
            {
                // queue up this phase for later
                ExecutePhasesChain.Enqueue(() => QueueProspectList(messageId, channel, eventArgs));
            }
            else
            {
                QueueProspectList(messageId, channel, eventArgs);
            }
        }

        private void QueueProspectList(string messageId, IModel channel, BasicDeliverEventArgs eventArgs)
        {
            MessageProperties_Sync.TryAdd(messageId, new()
            {
                BasicDeliveryEventArgs = eventArgs,
                Channel = channel
            });

            BackgroundJob.Enqueue<ICampaignManagerService>((x) => x.StartProspectList(messageId));
        }

        public async Task StartProspectList(string messageId)
        {
            MessageProperties_Sync.TryGetValue(messageId, out RabbitMQMessageProperties props);
            BasicDeliverEventArgs eventArgs = props.BasicDeliveryEventArgs;
            IModel channel = props.Channel;

            using (var scope = _serviceProvider.CreateScope())
            {
                //try and deserialize the response
                ICampaignPhaseSerializer serializer = scope.ServiceProvider.GetRequiredService<ICampaignPhaseSerializer>();
                byte[] body = eventArgs.Body.ToArray();
                string message = Encoding.UTF8.GetString(body);
                ProspectListBody prospectListBody = serializer.DeserializeProspectListBody(message);
                Action ackOperation = default;
                try
                {
                    ICampaignPhaseFacade campaignPhaseFacade = scope.ServiceProvider.GetRequiredService<ICampaignPhaseFacade>();
                    HalOperationResult<IOperationResponse> operationResult = await campaignPhaseFacade.ExecutePhase<IOperationResponse>(prospectListBody);

                    if (operationResult.Succeeded == true)
                    {
                        _logger.LogInformation("ExecuteFollowUpMessagesPhase executed successfully. Acknowledging message");
                        ackOperation = () => channel.BasicAck(eventArgs.DeliveryTag, false);
                    }
                    else
                    {
                        _logger.LogWarning("Executing Follow Up Messages Phase did not successfully succeeded. Negatively acknowledging the message and re-queuing it");
                        ackOperation = () => channel.BasicNack(eventArgs.DeliveryTag, false, true);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception occured while executing Follow Up Messages Phase. Negatively acknowledging the message and re-queuing it");
                    ackOperation = () => channel.BasicNack(eventArgs.DeliveryTag, false, true);
                }
                finally
                {
                    MessageProperties_Sync.TryRemove(messageId, out _);
                    NextPhase();
                    ackOperation();
                }
            }
        }

        #endregion

        #region SendConnectionRequests

        public void OnSendConnectionRequestsEventReceived(object sender, BasicDeliverEventArgs eventArgs)
        {
            IModel channel = ((EventingBasicConsumer)sender).Model;
            string messageId = eventArgs.BasicProperties.MessageId;
            // run this AFTER prospect list
            // This has to assume that ProspectList phase is either running or already ran
            if (MessageProperties_Sync.Keys.Any())
            {
                // queue up this phase for later
                ExecutePhasesChain.Enqueue(() => QueueSendConnectionRequests(messageId, channel, eventArgs));
            }
            else
            {
                QueueSendConnectionRequests(messageId, channel, eventArgs);
            }
        }

        private void QueueSendConnectionRequests(string messageId, IModel channel, BasicDeliverEventArgs eventArgs)
        {
            MessageProperties_Sync.TryAdd(messageId, new()
            {
                BasicDeliveryEventArgs = eventArgs,
                Channel = channel
            });

            BackgroundJob.Enqueue<ICampaignManagerService>((x) => x.StartSendConnectionRequests(messageId));
        }

        public void StartSendConnectionRequests(string messageId)
        {
            MessageProperties_Sync.TryGetValue(messageId, out RabbitMQMessageProperties props);
            BasicDeliverEventArgs eventArgs = props.BasicDeliveryEventArgs;
            IModel channel = props.Channel;

            using (var scope = _serviceProvider.CreateScope())
            {
                //try and deserialize the response
                ICampaignPhaseSerializer serializer = scope.ServiceProvider.GetRequiredService<ICampaignPhaseSerializer>();
                byte[] body = eventArgs.Body.ToArray();
                string message = Encoding.UTF8.GetString(body);
                SendConnectionsBody sendConnections = serializer.DeserializeSendConnectionRequestsBody(message);
                Action ackOperation = default;
                try
                {
                    ICampaignPhaseFacade campaignPhaseFacade = scope.ServiceProvider.GetRequiredService<ICampaignPhaseFacade>();
                    HalOperationResult<IOperationResponse> operationResult = campaignPhaseFacade.ExecutePhase<IOperationResponse>(sendConnections);

                    if (operationResult.Succeeded == true)
                    {
                        _logger.LogInformation("ExecuteFollowUpMessagesPhase executed successfully. Acknowledging message");
                        ackOperation = () => channel.BasicAck(eventArgs.DeliveryTag, false);
                    }
                    else
                    {
                        _logger.LogWarning("Executing Follow Up Messages Phase did not successfully succeeded. Negatively acknowledging the message and re-queuing it");
                        ackOperation = () => channel.BasicNack(eventArgs.DeliveryTag, false, true);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception occured while executing Follow Up Messages Phase. Negatively acknowledging the message and re-queuing it");
                    ackOperation = () => channel.BasicNack(eventArgs.DeliveryTag, false, true);
                }
                finally
                {
                    MessageProperties_Sync.TryRemove(messageId, out _);
                    NextPhase();
                    ackOperation();
                }
            }
        }

        #endregion

        #region Rescrape

        public void OnRescrapeSearchurlsEventReceived(object sender, BasicDeliverEventArgs eventArgs)
        {
            IModel channel = ((EventingBasicConsumer)sender).Model;
            string messageId = eventArgs.BasicProperties.MessageId;
            // run this rarely, but this would be prospect list under the hood
            if (MessageProperties_Sync.Keys.Any())
            {
                // queue up this phase for later
                ExecutePhasesChain.Enqueue(() => QueueProspectList(messageId, channel, eventArgs));
            }
            else
            {
                QueueProspectList(messageId, channel, eventArgs);
            }
        }

        #endregion

        #region ScanProspectsForReplies

        public void OnScanProspectsForRepliesEventReceived(object sender, BasicDeliverEventArgs eventArgs)
        {
            IModel channel = ((EventingBasicConsumer)sender).Model;
            string messageId = eventArgs.BasicProperties.MessageId;
            // this can be started asap and doesn't rely on any other event
            // execute scan prospects for replies logic
            // run this rarely, but this would be prospect list under the hood
            //if (MessageProperties_ConstantPhases.Keys.Any())
            //{
            //    // queue up this phase for later
            //    ExecutePhasesChain.Enqueue(() => QueueScanProspectsForReplies(messageId, channel, eventArgs));
            //}
            //else
            //{
            //    QueueScanProspectsForReplies(messageId, channel, eventArgs);
            //}
            QueueScanProspectsForReplies(messageId, channel, eventArgs);
        }

        private void QueueScanProspectsForReplies(string messageId, IModel channel, BasicDeliverEventArgs eventArgs)
        {
            MessageProperties_ConstantPhases.TryAdd(messageId, new()
            {
                BasicDeliveryEventArgs = eventArgs,
                Channel = channel
            });

            BackgroundJob.Enqueue<ICampaignManagerService>((x) => x.StartScanningProspectsForReplies(messageId));
        }

        public void StartScanningProspectsForReplies(string messageId)
        {
            MessageProperties_ConstantPhases.TryGetValue(messageId, out RabbitMQMessageProperties props);
            BasicDeliverEventArgs eventArgs = props.BasicDeliveryEventArgs;
            IModel channel = props.Channel;

            using (var scope = _serviceProvider.CreateScope())
            {
                //try and deserialize the response
                ICampaignPhaseSerializer serializer = scope.ServiceProvider.GetRequiredService<ICampaignPhaseSerializer>();
                byte[] body = eventArgs.Body.ToArray();
                string message = Encoding.UTF8.GetString(body);
                ScanProspectsForRepliesBody scanProspectsForRepliesBody = serializer.DeserializeScanProspectsForRepliesBody(message);
                Action ackOperation = default;
                try
                {
                    ICampaignPhaseFacade campaignPhaseFacade = scope.ServiceProvider.GetRequiredService<ICampaignPhaseFacade>();
                    HalOperationResult<IOperationResponse> operationResult = campaignPhaseFacade.ExecutePhase<IOperationResponse>(scanProspectsForRepliesBody);

                    if (operationResult.Succeeded == true)
                    {
                        _logger.LogInformation("ExecuteScanProspectsForRepliesPhase executed successfully. Acknowledging message");
                        ackOperation = () => channel.BasicAck(eventArgs.DeliveryTag, false);
                    }
                    else
                    {
                        _logger.LogWarning("Executing Scan Prospects For Replies phase did not successfully succeeded. Negatively acknowledging the message and re-queuing it");
                        ackOperation = () => channel.BasicNack(eventArgs.DeliveryTag, false, true);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception occured while executing Follow Up Messages Phase. Negatively acknowledging the message and re-queuing it");
                    ackOperation = () => channel.BasicNack(eventArgs.DeliveryTag, false, true);
                }
                finally
                {
                    MessageProperties_ConstantPhases.TryRemove(messageId, out _);
                    NextPhase();
                    ackOperation();
                }
            }
        }

        #endregion

        #region SendEmailInvites
        public void OnSendEmailInvitesEventReceived(object sender, BasicDeliverEventArgs eventArgs)
        {
            IModel channel = ((EventingBasicConsumer)sender).Model;
            throw new NotImplementedException();
        }

        #endregion
    }
}
