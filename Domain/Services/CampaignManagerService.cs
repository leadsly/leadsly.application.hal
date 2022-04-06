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
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
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
        /// the other phases can be triggered for a given campaign.
        /// </summary>
        private readonly ConcurrentDictionary<string, RabbitMQMessageProperties> MessageDetails_Queue = new();
        
        /// <summary>
        /// MessageDetails_ConstantPhases describes phases that can be run at any time. That includes MonitorForNewAcceptedConnections phase,
        /// ScanProspectsForReplies phase and FollowUpMessageBrowser phase. The first two phases are executed at the beginning of the day and run until
        /// end of day. FollowUpMessage phase is executed async and gets triggered when a new connection is received.
        /// </summary>
        private readonly ConcurrentDictionary<string, RabbitMQMessageProperties> MessageDetails_ConstantPhases = new();
        private readonly ConcurrentDictionary<string, RabbitMQMessageProperties> MessageDetails_SendConnectionsRequests = new();
        private readonly ConcurrentDictionary<string, RabbitMQMessageProperties> MessageDetails_FollowUpMessagePhases = new();
        private ConcurrentQueue<Action> ExecutePhasesQueue = new();

        private ConcurrentQueue<Func<Task>> ExecuteSynchronousPhasesQueue = new();


        ~CampaignManagerService()
        {

        }

        private void NextPhase()
        {
            if (ExecutePhasesQueue.Count > 0)
            {
                ExecutePhasesQueue.TryDequeue(out Action nextPhase);
                nextPhase();
            }
            else
            {
                _logger.LogInformation("There are no more queued jobs to run");
            }
        }

        private async Task NextSynchronousPhase()
        {
            if (ExecuteSynchronousPhasesQueue.Count > 0)
            {
                ExecuteSynchronousPhasesQueue.TryDequeue(out Func<Task> nextPhase);
                await nextPhase();
            }
            else
            {
                _logger.LogInformation("There are no more queued jobs to run that must execute synchronously. This only applies to FollowUpMessagesPhase and SendConnectionRequestsPhase");
            }
        }

        #region NetworkingConnections

        public async Task OnNetworkingConnectionsEventReceived(object sender, BasicDeliverEventArgs eventArgs)
        {
            IModel channel = ((AsyncEventingBasicConsumer)sender).Model;

            var headers = eventArgs.BasicProperties.Headers;
            headers.TryGetValue(RabbitMQConstants.NetworkingConnections.NetworkingType, out object networkTypeObj);

            byte[] networkTypeArr = networkTypeObj as byte[];

            string networkType = Encoding.UTF8.GetString(networkTypeArr);
            if (networkType == null)
            {
                _logger.LogError("Failed to determine networking connection type. It should be a header either for SendConnectionRequests or for ProspectList");
                throw new ArgumentException("A header value must be provided!");
            }

            if((networkType as string) == RabbitMQConstants.NetworkingConnections.ProspectList)
            {
                await StartProspectList(channel, eventArgs);
            }
            else if((networkType as string) == RabbitMQConstants.NetworkingConnections.SendConnectionRequests)
            {
                await StartSendingConnectionRequests(channel, eventArgs);
            }
            else
            {
                string networkTypeStr = networkType as string;
                _logger.LogError("Invalid network type specified {networkTypeStr}", networkTypeStr);
            }                       
        }


        #endregion

        #region FollowUpMessages

        public void OnFollowUpMessageEventReceived(object sender, BasicDeliverEventArgs eventArgs)
        {
            IModel channel = ((EventingBasicConsumer)sender).Model;
            string messageId = eventArgs.BasicProperties.MessageId;

            // this can be started asap and doesn't rely on any other event            
            // execute send follow up message logic
            if (MessageDetails_FollowUpMessagePhases.Keys.Any())
            {
                // queue up this phase for later
                ExecutePhasesQueue.Enqueue(() => QueueFollowUpMessages(messageId, channel, eventArgs));                
            }
            else
            {
                QueueFollowUpMessages(messageId, channel, eventArgs);
            }
        }

        private void QueueFollowUpMessages(string messageId, IModel channel, BasicDeliverEventArgs eventArgs)
        {
            MessageDetails_FollowUpMessagePhases.TryAdd(messageId, new()
            {
                BasicDeliveryEventArgs = eventArgs,
                Channel = channel
            });

            BackgroundJob.Enqueue<ICampaignManagerService>((x) => x.StartFollowUpMessages(messageId));
        }

        public void StartFollowUpMessages(string messageId)
        {
            MessageDetails_FollowUpMessagePhases.TryGetValue(messageId, out RabbitMQMessageProperties props);
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
                    MessageDetails_FollowUpMessagePhases.TryRemove(messageId, out _);
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
            if (MessageDetails_Queue.Keys.Any())
            {
                // queue up this phase for later
                ExecutePhasesQueue.Enqueue(() => QueueConnectionWithdraw(messageId, channel, eventArgs));
            }
            else
            {
                QueueConnectionWithdraw(messageId, channel, eventArgs);
            }
        }

        private void QueueConnectionWithdraw(string messageId, IModel channel, BasicDeliverEventArgs eventArgs)
        {
            MessageDetails_Queue.TryAdd(messageId, new()
            {
                BasicDeliveryEventArgs = eventArgs,
                Channel = channel
            });

            BackgroundJob.Enqueue<ICampaignManagerService>((x) => x.StartConnectionWithdraw(messageId));
        }

        public void StartConnectionWithdraw(string messageId)
        {
            MessageDetails_Queue.TryGetValue(messageId, out RabbitMQMessageProperties props);
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
                    MessageDetails_Queue.TryRemove(messageId, out _);
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
            //if (MessageDetails_ConstantPhases.Keys.Any())
            //{
            //    // queue up this phase for later
            //    ExecutePhasesQueue.Enqueue(() => QueueMonitorForNewAcceptedConnections(messageId, channel, eventArgs));
            //}
            //else
            //{
            //    QueueMonitorForNewAcceptedConnections(messageId, channel, eventArgs);
            //}
            QueueMonitorForNewAcceptedConnections(messageId, channel, eventArgs);
        }

        private void QueueMonitorForNewAcceptedConnections(string messageId, IModel channel, BasicDeliverEventArgs eventArgs)
        {
            MessageDetails_ConstantPhases.TryAdd(messageId, new()
            {
                BasicDeliveryEventArgs = eventArgs,
                Channel = channel
            });

            BackgroundJob.Enqueue<ICampaignManagerService>((x) => x.StartMonitorForNewConnections(messageId));
        }

        public async Task StartMonitorForNewConnections(string messageId)
        {
            MessageDetails_ConstantPhases.TryGetValue(messageId, out RabbitMQMessageProperties props);
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
                    MessageDetails_ConstantPhases.TryRemove(messageId, out _);
                    NextPhase();
                    ackOperation();
                }
            }
        }

        #endregion

        #region ProspectList

        //public void OnProspectListEventReceived(object sender, BasicDeliverEventArgs eventArgs)
        //{
        //    IModel channel = ((EventingBasicConsumer)sender).Model;
        //    string messageId = eventArgs.BasicProperties.MessageId;
        //    if (MessageDetails_Queue.Keys.Any())
        //    {
        //        // queue up this phase for later
        //        ExecuteSynchronousPhasesQueue.Enqueue(() => QueueProspectList(messageId, channel, eventArgs));
        //    }
        //    else
        //    {
        //        QueueProspectList(messageId, channel, eventArgs);
        //    }
        //}

        //private Task QueueProspectList(string messageId, IModel channel, BasicDeliverEventArgs eventArgs)
        //{
        //    MessageDetails_Queue.TryAdd(messageId, new()
        //    {
        //        BasicDeliveryEventArgs = eventArgs,
        //        Channel = channel
        //    });

        //    BackgroundJob.Enqueue<ICampaignManagerService>((x) => x.StartProspectList(messageId));

        //    return Task.CompletedTask;
        //}

        private async Task StartProspectList(IModel channel, BasicDeliverEventArgs eventArgs)
        {
            //MessageDetails_Queue.TryGetValue(messageId, out RabbitMQMessageProperties props);
            //BasicDeliverEventArgs eventArgs = props.BasicDeliveryEventArgs;
            //IModel channel = props.Channel;

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
                    HalOperationResult<IOperationResponse> operationResult = await campaignPhaseFacade.ExecutePhaseAsync<IOperationResponse>(prospectListBody);

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
                    //MessageDetails_Queue.TryRemove(messageId, out _);
                    //await NextSynchronousPhase();
                    ackOperation();
                }
            }
        }

        #endregion

        #region SendConnectionRequests

        private async Task StartSendingConnectionRequests(IModel channel, BasicDeliverEventArgs eventArgs)
        {

            using (var scope = _serviceProvider.CreateScope())
            {
                //try and deserialize the response
                ICampaignPhaseSerializer serializer = scope.ServiceProvider.GetRequiredService<ICampaignPhaseSerializer>();
                byte[] body = eventArgs.Body.ToArray();
                string message = Encoding.UTF8.GetString(body);
                SendConnectionsBody sendConnectionsBody = serializer.DeserializeSendConnectionRequestsBody(message);

                Action ackOperation = default;
                try
                {
                    ICampaignPhaseFacade campaignPhaseFacade = scope.ServiceProvider.GetRequiredService<ICampaignPhaseFacade>();
                    HalOperationResult<IOperationResponse> operationResult = await campaignPhaseFacade.ExecutePhaseAsync<IOperationResponse>(sendConnectionsBody);

                    if (operationResult.Succeeded == true)
                    {
                        _logger.LogInformation("SendConnectionRequests executed successfully. Acknowledging message");
                        ackOperation = () => channel.BasicAck(eventArgs.DeliveryTag, false);
                    }
                    else
                    {
                        _logger.LogWarning("SendConnectionRequests phase did not successfully execute. Negatively acknowledging the message and re-queuing it");
                        ackOperation = () => channel.BasicNack(eventArgs.DeliveryTag, false, true);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception occured while executing send connection requests. Negatively acknowledging the message and re-queuing it");
                    ackOperation = () => channel.BasicNack(eventArgs.DeliveryTag, false, true);
                }
                finally
                {
                    //MessageDetails_SendConnectionsRequests.TryRemove(messageId, out _);
                    //MessageDetails_Queue.TryRemove(messageId, out _);
                    ackOperation();
                    //await NextSynchronousPhase();
                }
            }
        }

        //public void OnSendConnectionRequestsEventReceived(object sender, BasicDeliverEventArgs eventArgs)
        //{
        //    IModel channel = ((EventingBasicConsumer)sender).Model;
        //    string messageId = eventArgs.BasicProperties.MessageId;

        //    ScheduleSendConnectionsPhaseRequests(messageId, channel, eventArgs);
        //}

        //private void ScheduleSendConnectionsPhaseRequests(string messageId, IModel channel, BasicDeliverEventArgs eventArgs)
        //{
        //    using (var scope = _serviceProvider.CreateScope())
        //    {
        //        //try and deserialize the response
        //        ICampaignPhaseSerializer serializer = scope.ServiceProvider.GetRequiredService<ICampaignPhaseSerializer>();
        //        byte[] body = eventArgs.Body.ToArray();
        //        string message = Encoding.UTF8.GetString(body);
        //        SendConnectionsBody sendConnectionsBody = serializer.DeserializeSendConnectionRequestsBody(message);

        //        DateTime dateTimeToStart = DateTime.Parse(sendConnectionsBody.SendConnectionsStage.StartTime);
        //        DateTime now = DateTime.Parse("7:30 PM"); // DateTime.Now;
        //        // if right now 11:56PM is before the scheduled time, schedule the job, else, enqueue it right away
        //        if (now.TimeOfDay < dateTimeToStart.TimeOfDay)
        //        {
        //            MessageDetails_SendConnectionsRequests.TryAdd(messageId, new()
        //            {
        //                BasicDeliveryEventArgs = eventArgs,
        //                Channel = channel,
        //                SendConnectionsBody = sendConnectionsBody,
        //            });

        //            BackgroundJob.Schedule<ICampaignManagerService>((x) => x.StartSendConnectionRequests(messageId), dateTimeToStart.TimeOfDay);
        //        }
        //        else
        //        {
        //            MessageDetails_SendConnectionsRequests.TryAdd(messageId, new()
        //            {
        //                BasicDeliveryEventArgs = eventArgs,
        //                Channel = channel,
        //                SendConnectionsBody = sendConnectionsBody
        //            });

        //            BackgroundJob.Enqueue<ICampaignManagerService>((x) => x.StartSendConnectionRequests(messageId));
        //        }

        //    }
        //}

        //public async Task StartSendConnectionRequests(string messageId)
        //{
        //    MessageDetails_SendConnectionsRequests.TryGetValue(messageId, out RabbitMQMessageProperties options);
        //    //run this AFTER prospect list
        //    //This has to assume that ProspectList phase is either running or already ran
        //    if (MessageDetails_Queue.Keys.Any())
        //    {
        //        // queue up this phase for later
        //        ExecuteSynchronousPhasesQueue.Enqueue(() => StartSendingConnectionRequests(messageId, options.Channel, options.BasicDeliveryEventArgs, options.SendConnectionsBody));
        //    }
        //    else
        //    {
        //        MessageDetails_Queue.TryAdd(messageId, new()
        //        {
        //            Channel = options.Channel,
        //            BasicDeliveryEventArgs = options.BasicDeliveryEventArgs
        //        });

        //        await StartSendingConnectionRequests(messageId, options.Channel, options.BasicDeliveryEventArgs, options.SendConnectionsBody);
        //    }
        //}

        //private async Task SendConnectionRequests(string messageId, IModel channel, BasicDeliverEventArgs eventArgs, SendConnectionsBody messageBody)
        //{
        //    await StartSendingConnectionRequests(messageId, channel, eventArgs, messageBody);
        //}

        //private async Task StartSendingConnectionRequests(string messageId, IModel channel, BasicDeliverEventArgs eventArgs, SendConnectionsBody sendConnections)
        //{           

        //    using (var scope = _serviceProvider.CreateScope())
        //    {
        //        Action ackOperation = default;
        //        try
        //        {
        //            ICampaignPhaseFacade campaignPhaseFacade = scope.ServiceProvider.GetRequiredService<ICampaignPhaseFacade>();
        //            HalOperationResult<IOperationResponse> operationResult = await campaignPhaseFacade.ExecutePhaseAsync<IOperationResponse>(sendConnections);

        //            if (operationResult.Succeeded == true)
        //            {
        //                _logger.LogInformation("SendConnectionRequests executed successfully. Acknowledging message");
        //                ackOperation = () => channel.BasicAck(eventArgs.DeliveryTag, false);
        //            }
        //            else
        //            {
        //                _logger.LogWarning("SendConnectionRequests phase did not successfully execute. Negatively acknowledging the message and re-queuing it");
        //                ackOperation = () => channel.BasicNack(eventArgs.DeliveryTag, false, true);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError(ex, "Exception occured while executing send connection requests. Negatively acknowledging the message and re-queuing it");
        //            ackOperation = () => channel.BasicNack(eventArgs.DeliveryTag, false, true);
        //        }
        //        finally
        //        {
        //            MessageDetails_SendConnectionsRequests.TryRemove(messageId, out _);
        //            MessageDetails_Queue.TryRemove(messageId, out _);
        //            ackOperation();
        //            await NextSynchronousPhase();                    
        //        }
        //    }
        //}

        #endregion

        #region Rescrape

        public void OnRescrapeSearchurlsEventReceived(object sender, BasicDeliverEventArgs eventArgs)
        {
            //IModel channel = ((EventingBasicConsumer)sender).Model;
            //string messageId = eventArgs.BasicProperties.MessageId;
            //// run this rarely, but this would be prospect list under the hood
            //if (MessageDetails_Queue.Keys.Any())
            //{
            //    // queue up this phase for later
            //    ExecutePhasesQueue.Enqueue(() => QueueProspectList(messageId, channel, eventArgs));
            //}
            //else
            //{
            //    QueueProspectList(messageId, channel, eventArgs);
            //}
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
            //if (MessageDetails_ConstantPhases.Keys.Any())
            //{
            //    // queue up this phase for later
            //    ExecutePhasesQueue.Enqueue(() => QueueScanProspectsForReplies(messageId, channel, eventArgs));
            //}
            //else
            //{
            //    QueueScanProspectsForReplies(messageId, channel, eventArgs);
            //}
            QueueScanProspectsForReplies(messageId, channel, eventArgs);
        }

        private void QueueScanProspectsForReplies(string messageId, IModel channel, BasicDeliverEventArgs eventArgs)
        {
            MessageDetails_ConstantPhases.TryAdd(messageId, new()
            {
                BasicDeliveryEventArgs = eventArgs,
                Channel = channel
            });

            BackgroundJob.Enqueue<ICampaignManagerService>((x) => x.StartScanningProspectsForReplies(messageId));
        }

        public void StartScanningProspectsForReplies(string messageId)
        {
            MessageDetails_ConstantPhases.TryGetValue(messageId, out RabbitMQMessageProperties props);
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
                    MessageDetails_ConstantPhases.TryRemove(messageId, out _);
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
