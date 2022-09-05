using Domain.POMs.Controls;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;

namespace PageObjects.Controls
{
    public class ConversationCards : IConversationCards
    {
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

        public IReadOnlyCollection<IWebElement> GetAllConversationCloseButtons(IWebDriver webDriver)
        {
            return ConversationCardsCloseButtons(webDriver);
        }
    }
}
