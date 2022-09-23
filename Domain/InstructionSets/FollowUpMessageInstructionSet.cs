using Domain.Facades.Interfaces;
using Domain.InstructionSets.Interfaces;
using Domain.Interactions;
using Domain.Interactions.FollowUpMessage.CreateNewMessage;
using Domain.Interactions.FollowUpMessage.EnterMessage;
using Domain.Interactions.FollowUpMessage.EnterProspectName;
using Domain.Models.FollowUpMessage;
using Domain.MQ.Messages;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace Domain.InstructionSets
{
    public class FollowUpMessageInstructionSet : IFollowUpMessageInstructionSet
    {
        public FollowUpMessageInstructionSet(
            ILogger<FollowUpMessageInstructionSet> logger,
            IFollowUpMessageInteractionFacade interactionFacade)
        {
            _interactionFacade = interactionFacade;
            _logger = logger;
        }

        private readonly IFollowUpMessageInteractionFacade _interactionFacade;
        private readonly ILogger<FollowUpMessageInstructionSet> _logger;
        private SentFollowUpMessageModel SentFollowUpMessage { get; set; }
        public SentFollowUpMessageModel GetSentFollowUpMessage()
        {
            SentFollowUpMessageModel item = SentFollowUpMessage;
            SentFollowUpMessage = null;
            return item;
        }

        public void SendFollowUpMessage(IWebDriver webDriver, FollowUpMessageBody message)
        {
            if (CreateNewMessageInteraction(webDriver) == false)
            {
                return;
            }

            if (EnterProspectNameInteraction(webDriver, message.ProspectName) == false)
            {
                return;
            }

            if (EnterMessageInteraction(webDriver, message.Content, message.OrderNum) == false)
            {
                return;
            }

            SentFollowUpMessage = _interactionFacade.SentFollowUpMessage;
        }

        private bool CreateNewMessageInteraction(IWebDriver webDriver)
        {
            InteractionBase interaction = new CreateNewMessageInteraction
            {
                WebDriver = webDriver
            };

            return _interactionFacade.HandleCreateNewMessageInteraction(interaction);
        }

        private bool EnterProspectNameInteraction(IWebDriver webDriver, string prospectName)
        {
            InteractionBase interaction = new EnterProspectNameInteraction
            {
                ProspectName = prospectName,
                WebDriver = webDriver
            };

            return _interactionFacade.HandleEnterProspectNameInteraction(interaction);
        }

        private bool EnterMessageInteraction(IWebDriver webDriver, string content, int orderNum)
        {
            InteractionBase interaction = new EnterMessageInteraction
            {
                Content = content,
                WebDriver = webDriver,
                OrderNum = orderNum
            };

            return _interactionFacade.HandleEnterMessageInteraction(interaction);
        }
    }
}
