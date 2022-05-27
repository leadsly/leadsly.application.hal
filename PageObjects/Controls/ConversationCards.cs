using Domain.POMs.Controls;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageObjects.Controls
{
    public class ConversationCards : IConversationCards
    {
        private IReadOnlyCollection<IWebElement> ViewableConversationCards(IWebDriver webDriver)
        {
            IReadOnlyCollection<IWebElement> conversationCards = default;
            try
            {
                conversationCards = webDriver.FindElements(By.CssSelector("div[role='dialog']"));
            }
            catch (Exception ex)
            {

            }
            return conversationCards;
        }

        private IReadOnlyCollection<IWebElement> ConversationCardsCloseButtons(IWebDriver webDriver)
        {
            IReadOnlyCollection<IWebElement> conversationCardsCloseButtons = default;
            try
            {
                conversationCardsCloseButtons = webDriver.FindElements(By.CssSelector("button[data-control-name='overlay.close_conversation_window']"));
            }
            catch (Exception ex)
            {

            }
            return conversationCardsCloseButtons;
        }

        public IReadOnlyCollection<IWebElement> GetConversationCards(IWebDriver webDriver)
        {
            return ViewableConversationCards(webDriver);
        }

        public IReadOnlyCollection<IWebElement> GetAllConversationCloseButtons(IWebDriver webDriver)
        {
            return ConversationCardsCloseButtons(webDriver);
        }
    }
}
