using Domain.Facades.Interfaces;
using Domain.Providers;
using Domain.Providers.Campaigns.Interfaces;
using Domain.Services.Interfaces;
using Domain.Supervisor;
using Hangfire;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns;
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
        private readonly ConcurrentDictionary<string, RabbitMQMessageProperties> MessageProperties = new();
        private ConcurrentQueue<Action> ExecutePhasesChain = new();

        private static CurrentJob CurrentJob { get; set; }
        private readonly object _jobLock = new object();
        private readonly object _readJobLock_FollowUpMessages = new object();
        private readonly object _updateJobLock_FollowUpMessages = new object();
        private static string ConnectionWithDrawRecurringJob = "connection-widthdraw";
        

        ~CampaignManagerService()
        {

        }

        public void OnConnectionWithdrawEventReceived(object channel, BasicDeliverEventArgs eventArgs)
        {            
            RecurringJob.AddOrUpdate(ConnectionWithDrawRecurringJob, () => Console.WriteLine("testestest"), "*9***");
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
                lock (_updateJobLock_FollowUpMessages)
                {
                    ClearCurrentJob();
                }
                _logger.LogInformation("There are no more queued jobs to run");
            }
        }

        private void ClearCurrentJob()
        {
            CurrentJob = default;
        }

        #region OnEventReceived Handlers

        public void OnFollowUpMessageEventReceived(object sender, BasicDeliverEventArgs eventArgs)
        {
            IModel channel = ((EventingBasicConsumer)sender).Model;
            string messageId = eventArgs.BasicProperties.MessageId;

            // this can be started asap and doesn't rely on any other event            
            // execute send follow up message logic
            if (MessageProperties.Keys.Any())
            {
                // queue up this phase for later
                ExecutePhasesChain.Enqueue(() => QueueFollowUpMessages(messageId, channel, eventArgs));                
            }
            else
            {
                QueueFollowUpMessages(messageId, channel, eventArgs);
            }
        }        

        public void OnMonitorForNewAcceptedConnectionsEventReceived(object sender, BasicDeliverEventArgs eventArgs)
        {
            // this can be started asap and doesn't rely on any other event
            // execute monitor for new accepted connections logic
            IModel channel = ((EventingBasicConsumer)sender).Model;
        }

        public void OnProspectListEventReceived(object sender, BasicDeliverEventArgs eventArgs)
        {
            IModel channel = ((EventingBasicConsumer)sender).Model;
            if (CurrentJob.Executing)
            {
                // queue up this phase for later
                ExecutePhasesChain.Enqueue(() => QueueProspectList(channel, eventArgs));
            }
            else
            {
                QueueProspectList(channel, eventArgs);
            }
        }

        public void OnSendConnectionRequestsEventReceived(object sender, BasicDeliverEventArgs eventArgs)
        {
            IModel channel = ((EventingBasicConsumer)sender).Model;
            // run this AFTER prospect list
            // This has to assume that ProspectList phase is either running or already ran
            if (CurrentJob.Executing)
            {
                // queue up this phase for later
                ExecutePhasesChain.Enqueue(() => QueueSendConnectionRequests(channel, eventArgs));
            }
            else
            {
                QueueSendConnectionRequests(channel, eventArgs);
            }
        }        

        public void OnRescrapeSearchurlsEventReceived(object sender, BasicDeliverEventArgs eventArgs)
        {
            IModel channel = ((EventingBasicConsumer)sender).Model;
            // run this rarely, but this would be prospect list under the hood
            if (CurrentJob.Executing)
            {
                // queue up this phase for later
                ExecutePhasesChain.Enqueue(() => QueueProspectList(channel, eventArgs));
            }
            else
            {
                QueueProspectList(channel, eventArgs);
            }
        }

        public void OnScanProspectsForRepliesEventReceived(object sender, BasicDeliverEventArgs eventArgs)
        {
            IModel channel = ((EventingBasicConsumer)sender).Model;
            // this can be started asap and doesn't rely on any other event
            // execute scan prospects for replies logic
            // run this rarely, but this would be prospect list under the hood
            if (CurrentJob.Executing)
            {
                // queue up this phase for later
                ExecutePhasesChain.Enqueue(() => QueueScanProspectsForReplies(channel, eventArgs));
            }
            else
            {
                QueueScanProspectsForReplies(channel, eventArgs);
            }
        }        

        public void OnSendEmailInvitesEventReceived(object sender, BasicDeliverEventArgs eventArgs)
        {
            IModel channel = ((EventingBasicConsumer)sender).Model;
            throw new NotImplementedException();
        }

        #endregion

        private void QueueScanProspectsForReplies(IModel channel, BasicDeliverEventArgs eventArgs)
        {
            SetCurrentJob(PhasesType.ProspectList, channel, eventArgs);

            BackgroundJob.Enqueue(() => StartScanProspectsForReplies());
        }

        private void StartScanProspectsForReplies()
        {

        }

        private void QueueFollowUpMessages(string messageId, IModel channel, BasicDeliverEventArgs eventArgs)
        {
            MessageProperties.TryAdd(messageId, new()
            {
                BasicDeliveryEventArgs = eventArgs,
                Channel = channel
            });

            BackgroundJob.Enqueue<ICampaignManagerService>((x) => x.StartFollowUpMessages(messageId)); 
        }

        public void StartFollowUpMessages(string messageId)
        {
            MessageProperties.TryGetValue(messageId, out RabbitMQMessageProperties props);
            BasicDeliverEventArgs eventArgs = props.BasicDeliveryEventArgs;
            IModel channel = props.Channel;          

            using (var scope = _serviceProvider.CreateScope())
            {
                //try and deserialize the response
                IDeserializerFacade deserializerFacade = scope.ServiceProvider.GetRequiredService<IDeserializerFacade>();
                byte[] body = eventArgs.Body.ToArray();
                string message = Encoding.UTF8.GetString(body);
                FollowUpMessagesBody followUpMessages = deserializerFacade.DeserializeFollowUpMessagesBody(message);

                try
                {
                    ICampaignPhaseFacade campaignPhaseFacade = scope.ServiceProvider.GetRequiredService<ICampaignPhaseFacade>();
                    HalOperationResult<IOperationResponse> operationResult = campaignPhaseFacade.ExecuteFollowUpMessagesPhase<IOperationResponse>(followUpMessages);

                    if (operationResult.Succeeded == true)
                    {
                        _logger.LogInformation("ExecuteFollowUpMessagesPhase executed successfully. Acknowledging message");
                        MessageProperties.TryRemove(messageId, out _);
                        channel.BasicAck(eventArgs.DeliveryTag, false);
                    }
                    else
                    {
                        _logger.LogWarning("Executing Follow Up Messages Phase did not successfully succeeded. Negatively acknowledging the message and re-queuing it");
                        MessageProperties.TryRemove(messageId, out _);
                        channel.BasicNack(eventArgs.DeliveryTag, false, true);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception occured while executing Follow Up Messages Phase. Negatively acknowledging the message and re-queuing it");
                    MessageProperties.TryRemove(messageId, out _);
                    channel.BasicNack(eventArgs.DeliveryTag, false, true);
                }
                finally
                {
                    NextPhase();
                }
            }
                
        }

        private void QueueSendConnectionRequests(IModel channel, BasicDeliverEventArgs eventArgs)
        {
            SetCurrentJob(PhasesType.ProspectList, channel, eventArgs);

            BackgroundJob.Enqueue(() => StartSendConnectionRequests());
        }
        private void StartSendConnectionRequests()
        {
        }

        private void QueueProspectList(IModel channel, BasicDeliverEventArgs eventArgs)
        {
            SetCurrentJob(PhasesType.ProspectList, channel, eventArgs);

            BackgroundJob.Enqueue(() => StartProspectList());
        }

        private void StartProspectList()
        {

        }

        private void SetCurrentJob(PhasesType phasesType, IModel channel, BasicDeliverEventArgs eventArgs)
        {
            CurrentJob job = new CurrentJob
            {
                Executing = true,
                PhaseType = phasesType,
                Channel = channel,
                DeliveryEventArgs = eventArgs
            };
            lock (_jobLock)
            {
                CurrentJob = job;
            }
        }
               
    }
}
