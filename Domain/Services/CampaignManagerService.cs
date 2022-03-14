using Domain.Providers;
using Domain.Providers.Campaigns.Interfaces;
using Hangfire;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Entities.Campaigns;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services
{
    public class CampaignManagerService : ICampaignManagerService
    {
        public CampaignManagerService(ILogger<CampaignManagerService> logger, IFollowUpMessagesProvider followUpMessagesProvider, IDeserializerProvider deserializerProvider)
        {
            _logger = logger;
            _followUpMessagesProvider = followUpMessagesProvider;
            _deserializerProvider = deserializerProvider;
        }

        private readonly IFollowUpMessagesProvider _followUpMessagesProvider;
        private readonly ILogger<CampaignManagerService> _logger;
        private readonly IDeserializerProvider _deserializerProvider;
        private static CurrentJob CurrentJob { get; set; }
        private object _jobLock = new object();
        private static string ConnectionWithDrawRecurringJob = "connection-widthdraw";
        private static Queue<Action> ExecutePhasesChain = new();
        

        public void OnConnectionWithdrawEventReceived(object channel, BasicDeliverEventArgs eventArgs)
        {            
            RecurringJob.AddOrUpdate(ConnectionWithDrawRecurringJob, () => Console.WriteLine("testestest"), "*9***");
        }

        public void NextPhase()
        {
            if (ExecutePhasesChain.Count > 0)
            {
                Action nextPhase = ExecutePhasesChain.Dequeue();
                nextPhase();
            }
        }

        public void PositiveAcknowledge(IModel channel, ulong deliveryTag)
        {
            channel.BasicAck(deliveryTag, false);
        }

        public void NegativeAcknowledge(IModel channel, ulong deliveryTag, bool retry)
        {
            channel.BasicNack(deliveryTag, false, retry);
        }

        public void ClearCurrentJob()
        {
            CurrentJob = default;
        }

        #region OnEventReceived Handlers

        public void OnFollowUpMessageEventReceived(object sender, BasicDeliverEventArgs eventArgs)
        {
            //IModel channel = ((EventingBasicConsumer)sender).Model;

            //channel.BasicAck(eventArgs.DeliveryTag, false);

            var body = eventArgs.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);


            // this can be started asap and doesn't rely on any other event            
            // execute send follow up message logic
            if (CurrentJob.Executing)
            {
                // queue up this phase for later
                ExecutePhasesChain.Enqueue(() => QueueFollowUpMessages());
            }
            else
            {
                QueueFollowUpMessages();
            }
        }        

        public void OnMonitorForNewAcceptedConnectionsEventReceived(object channel, BasicDeliverEventArgs eventArgs)
        {
            // this can be started asap and doesn't rely on any other event
            // execute monitor for new accepted connections logic
        }

        public void OnProspectListEventReceived(object channel, BasicDeliverEventArgs eventArgs)
        {
            if (CurrentJob.Executing)
            {
                // queue up this phase for later
                ExecutePhasesChain.Enqueue(() => QueueProspectList());
            }
            else
            {
                QueueProspectList();
            }
        }

        public void OnSendConnectionRequestsEventReceived(object channel, BasicDeliverEventArgs eventArgs)
        {
            // run this AFTER prospect list
            // This has to assume that ProspectList phase is either running or already ran
            if (CurrentJob.Executing)
            {
                // queue up this phase for later
                ExecutePhasesChain.Enqueue(() => QueueSendConnectionRequests());
            }
            else
            {
                QueueSendConnectionRequests();
            }
        }        

        public void OnRescrapeSearchurlsEventReceived(object channel, BasicDeliverEventArgs eventArgs)
        {
            // run this rarely, but this would be prospect list under the hood
            if (CurrentJob.Executing)
            {
                // queue up this phase for later
                ExecutePhasesChain.Enqueue(() => QueueProspectList());
            }
            else
            {
                QueueProspectList();
            }
        }

        public void OnScanProspectsForRepliesEventReceived(object channel, BasicDeliverEventArgs eventArgs)
        {
            // this can be started asap and doesn't rely on any other event
            // execute scan prospects for replies logic
            // run this rarely, but this would be prospect list under the hood
            if (CurrentJob.Executing)
            {
                // queue up this phase for later
                ExecutePhasesChain.Enqueue(() => QueueScanProspectsForReplies());
            }
            else
            {
                QueueScanProspectsForReplies();
            }
        }        

        public void OnSendEmailInvitesEventReceived(object channel, BasicDeliverEventArgs eventArgs)
        {
            throw new NotImplementedException();
        }

        #endregion

        private void QueueScanProspectsForReplies()
        {
            SetCurrentJob(PhasesType.ProspectList);

            BackgroundJob.Enqueue(() => StartScanProspectsForReplies());
        }

        private void StartScanProspectsForReplies()
        {
        }

        private void QueueFollowUpMessages()
        {
            SetCurrentJob(PhasesType.ProspectList);

            BackgroundJob.Enqueue(() => StartFollowUpMessages());
        }

        private void StartFollowUpMessages()
        {
            
        }

        private void QueueSendConnectionRequests()
        {
            SetCurrentJob(PhasesType.ProspectList);

            BackgroundJob.Enqueue(() => StartSendConnectionRequests());
        }
        private void StartSendConnectionRequests()
        {
        }

        private void QueueProspectList()
        {
            SetCurrentJob(PhasesType.ProspectList);

            BackgroundJob.Enqueue(() => StartProspectList());
        }

        private void StartProspectList()
        {

        }

        private void SetCurrentJob(PhasesType phasesType, IModel channel)
        {
            CurrentJob job = new CurrentJob
            {
                Executing = true,
                PhaseType = phasesType,
                Channel = channel
            };
            lock (_jobLock)
            {
                CurrentJob = job;
            }
        }
               
    }
}
