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
        public CampaignManagerService(ILogger<CampaignManagerService> logger)
        {
            _logger = logger;
        }

        private readonly ILogger<CampaignManagerService> _logger;
        private static CurrentJob CurrentJob { get; set; }
        private object _jobLock = new object();
        private static string ConnectionWithDrawRecurringJob = "connection-widthdraw";
        private static Queue<Action> ExecutePhases = new();

        public void OnConnectionWithdrawEventReceived(object channel, BasicDeliverEventArgs eventArgs)
        {            
            RecurringJob.AddOrUpdate(ConnectionWithDrawRecurringJob, () => Console.WriteLine("testestest"), "*9***");
        }

        public void NextPhase()
        {
            if (ExecutePhases.Count > 0)
            {
                Action nextPhase = ExecutePhases.Dequeue();
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

        public void OnFollowUpMessageEventReceived(object channel, BasicDeliverEventArgs eventArgs)
        {
            var body = eventArgs.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            // this can be started asap and doesn't rely on any other event            
            // execute send follow up message logic
            if (CurrentJob.Executing)
            {
                // queue up this phase for later
                ExecutePhases.Enqueue(() => QueueFollowUpMessages(channel));
            }
            else
            {
                QueueFollowUpMessages(channel);
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
                ExecutePhases.Enqueue(() => QueueProspectList(channel));
            }
            else
            {
                QueueProspectList(channel);
            }
        }

        public void OnSendConnectionRequestsEventReceived(object channel, BasicDeliverEventArgs eventArgs)
        {
            // run this AFTER prospect list
            // This has to assume that ProspectList phase is either running or already ran
            if (CurrentJob.Executing)
            {
                // queue up this phase for later
                ExecutePhases.Enqueue(() => QueueSendConnectionRequests(channel));
            }
            else
            {
                QueueSendConnectionRequests(channel);
            }
        }        

        public void OnRescrapeSearchurlsEventReceived(object channel, BasicDeliverEventArgs eventArgs)
        {
            // run this rarely, but this would be prospect list under the hood
            if (CurrentJob.Executing)
            {
                // queue up this phase for later
                ExecutePhases.Enqueue(() => QueueProspectList(channel));
            }
            else
            {
                QueueProspectList(channel);
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
                ExecutePhases.Enqueue(() => QueueScanProspectsForReplies(channel));
            }
            else
            {
                QueueScanProspectsForReplies(channel);
            }
        }        

        public void OnSendEmailInvitesEventReceived(object channel, BasicDeliverEventArgs eventArgs)
        {
            throw new NotImplementedException();
        }

        #endregion

        private void QueueScanProspectsForReplies(object channel)
        {
            SetCurrentJob(PhasesType.ProspectList, channel);

            BackgroundJob.Enqueue(() => StartScanProspectsForReplies());
        }

        private void StartScanProspectsForReplies()
        {
            IModel channel = CurrentJob.Channel;
        }

        private void QueueFollowUpMessages(object channel)
        {
            SetCurrentJob(PhasesType.ProspectList, channel);

            BackgroundJob.Enqueue(() => StartFollowUpMessages());
        }

        private void StartFollowUpMessages()
        {
            IModel channel = CurrentJob.Channel;
        }

        private void QueueSendConnectionRequests(object channel)
        {
            SetCurrentJob(PhasesType.ProspectList, channel);

            BackgroundJob.Enqueue(() => StartSendConnectionRequests());
        }
        private void StartSendConnectionRequests()
        {
            IModel channel = CurrentJob.Channel;
        }

        private void QueueProspectList(object channel)
        {
            SetCurrentJob(PhasesType.ProspectList, channel);

            BackgroundJob.Enqueue(() => StartProspectList());
        }

        private void StartProspectList()
        {
            IModel channel = CurrentJob.Channel;

        }

        private void SetCurrentJob(PhasesType phasesType, object channel)
        {
            CurrentJob job = new CurrentJob
            {
                Executing = true,
                PhaseType = phasesType,
                Channel = channel as IModel
            };
            lock (_jobLock)
            {
                CurrentJob = job;
            }
        }
               
    }
}
