using Domain.Executors.AllInOneVirtualAssistant.Events;
using Domain.InstructionSets.Interfaces;
using Domain.Models.FollowUpMessage;
using Domain.MQ.Messages;
using Domain.Orchestrators.Interfaces;
using Domain.Providers.Interfaces;
using Leadsly.Application.Model;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;

namespace Domain.Orchestrators
{
    public class FollowUpMessagePhaseOrchestrator : PhaseOrchestratorBase, IFollowUpMessagePhaseOrchestrator
    {
        public FollowUpMessagePhaseOrchestrator(
            ILogger<FollowUpMessagePhaseOrchestrator> logger,
            IFollowUpMessageInstructionSet instructionSet,
            IWebDriverProvider webDriverProvider)
            : base(logger)
        {
            _instructionSet = instructionSet;
            _logger = logger;
            _webDriverProvider = webDriverProvider;
        }

        private readonly IFollowUpMessageInstructionSet _instructionSet;
        private readonly ILogger<FollowUpMessagePhaseOrchestrator> _logger;
        private readonly IWebDriverProvider _webDriverProvider;
        private IList<SentFollowUpMessageModel> SentFollowUpMessages { get; set; } = new List<SentFollowUpMessageModel>();
        public event FollowUpMessagesSentEventHandler FollowUpMessagesSent;
        public event ProspectsThatRepliedEventHandler ProspectsThatRepliedDetected
        {
            add => _instructionSet.ProspectsThatRepliedDetected += value;
            remove => _instructionSet.ProspectsThatRepliedDetected -= value;
        }

        public SentFollowUpMessageModel GetSentFollowUpMessage() => _instructionSet.GetSentFollowUpMessage();

        public IList<SentFollowUpMessageModel> GetSentFollowUpMessages()
        {
            IList<SentFollowUpMessageModel> messages = SentFollowUpMessages;
            SentFollowUpMessages = new List<SentFollowUpMessageModel>();
            return messages;
        }

        public void Execute(FollowUpMessageBody message)
        {
            string halId = message.HalId;
            string messageTypeName = nameof(FollowUpMessageBody);
            _logger.LogInformation("Executing {0} on HalId {1}", messageTypeName, halId);

            IWebDriver webDriver = _webDriverProvider.GetOrCreateWebDriver(BrowserPurpose.FollowUpMessages, message);
            if (webDriver == null)
            {
                _logger.LogError("Execution of {0} failed. WebDriver could not be found or created. Cannot proceed. HalId: {1}", messageTypeName, message.HalId);
                return;
            }

            if (GoToPage(webDriver, message.PageUrl) == false)
            {
                _logger.LogError("Execution of {0} failed. WebDriver could not navigate to the given PageUrl {1}. HalId {2}", messageTypeName, message.PageUrl, message.HalId);
                return;
            }

            ExecuteInternal(webDriver, message);
        }

        private void ExecuteInternal(IWebDriver webDriver, FollowUpMessageBody message)
        {
            try
            {
                _instructionSet.SendFollowUpMessage(webDriver, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected exception occured while executing {0} phase", nameof(FollowUpMessageBody));
            }
        }

        public void Execute(IWebDriver webDriver, AllInOneVirtualAssistantMessageBody message)
        {
            if (message.FollowUpMessages != null)
            {
                Queue<FollowUpMessageBody> followUpMessages = message.FollowUpMessages;
                int length = followUpMessages.Count;
                try
                {
                    for (int i = 0; i < length; i++)
                    {
                        FollowUpMessageBody followUpMessage = followUpMessages.Dequeue();

                        SendFollowUpMessage(webDriver, followUpMessage);

                        OutputFollowUpMessagesSent(followUpMessage);
                    }
                }
                finally
                {
                    SwitchBackToMainTab(webDriver);
                }
            }
        }

        private void SendFollowUpMessage(IWebDriver webDriver, FollowUpMessageBody message)
        {
            try
            {
                _instructionSet.SendFollowUpMessage_AllInOne(webDriver, message);

                SentFollowUpMessageModel sentFollowUpMessage = GetSentFollowUpMessage();
                if (sentFollowUpMessage != null)
                {
                    SentFollowUpMessages.Add(sentFollowUpMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected exception occured while executing {0} phase", nameof(FollowUpMessageBody));
            }
        }

        private void OutputFollowUpMessagesSent(FollowUpMessageBody message)
        {
            if (SentFollowUpMessages != null && SentFollowUpMessages.Count > 0)
            {
                this.FollowUpMessagesSent.Invoke(this, new FollowUpMessagesSentEventArgs(message, SentFollowUpMessages));
            }
        }
    }
}
