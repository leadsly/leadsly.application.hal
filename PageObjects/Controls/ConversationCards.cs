using Domain.POMs.Controls;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PageObjects.Controls
{
    public class ConversationCards : IConversationCards
    {
        public ConversationCards(
            IWebDriverUtilities webDriverUtilities)
        {
            _webDriverUtilities = webDriverUtilities;
        }

        private readonly IWebDriverUtilities _webDriverUtilities;

        private IList<IWebElement> ConversationCardsCloseButtons(IWebDriver webDriver)
        {
            IReadOnlyCollection<IWebElement> conversationCardsCloseButtons = default;
            try
            {
                conversationCardsCloseButtons = webDriver.FindElements(By.CssSelector("button[data-control-name='overlay.close_conversation_window']"));

            }
            catch (Exception ex)
            {

            }

            if (conversationCardsCloseButtons == null || conversationCardsCloseButtons.Count == 0)
            {
                try
                {
                    conversationCardsCloseButtons = webDriver.FindElements(By.XPath("//div[contains(@class, 'msg-convo-wrapper')] //descendant::li-icon[@type='cancel-icon']/ancestor::button"));
                }
                catch (Exception ex)
                {

                }
            }

            if (conversationCardsCloseButtons == null || conversationCardsCloseButtons.Count == 0)
            {
                try
                {
                    conversationCardsCloseButtons = webDriver.FindElements(By.XPath("//div[contains(@class, 'msg-convo-wrapper')] //descendant::li-icon[@type='close']/ancestor::button"));
                }
                catch (Exception ex)
                {

                }
            }

            return conversationCardsCloseButtons.ToList();
        }

        public IList<IWebElement> GetAllConversationCloseButtons(IWebDriver webDriver)
        {
            IList<IWebElement> closeButtons = _webDriverUtilities.WaitUntilNotNull(ConversationCardsCloseButtons, webDriver, 10);
            return closeButtons;
        }

        public IWebElement GetCloseConversationButton(IWebDriver webDriver, IWebElement conversationPopup)
        {
            IWebElement closeConversationButton = _webDriverUtilities.WaitUntilNotNull(CloseConversationButton, conversationPopup, webDriver, 5);
            return CloseConversationButton(conversationPopup);
        }

        private IWebElement CloseConversationButton(IWebElement conversationPopup)
        {
            IWebElement closeButton = default;
            try
            {
                closeButton = conversationPopup.FindElement(By.CssSelector("button[data-control-name='overlay.close_conversation_window']"));
            }
            catch (Exception ex)
            {

            }

            if (closeButton == null)
            {
                try
                {
                    closeButton = conversationPopup.FindElement(By.XPath("//div[contains(@class, 'msg-convo-wrapper')] //descendant::li-icon[@type='cancel-icon']/ancestor::button"));
                }
                catch (Exception ex)
                {

                }
            }

            if (closeButton == null)
            {
                try
                {
                    closeButton = conversationPopup.FindElement(By.XPath("//div[contains(@class, 'msg-convo-wrapper')] //descendant::li-icon[@type='close']/ancestor::button"));
                }
                catch (Exception ex)
                {

                }
            }

            return closeButton;
        }
    }
}
