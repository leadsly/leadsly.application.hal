﻿using Domain.POMs.Controls;
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

            if (conversationCardsCloseButtons == null || conversationCardsCloseButtons.Count == 0)
            {
                try
                {
                    conversationCardsCloseButtons = webDriver.FindElements(By.XPath("//div[contains(@class, 'msg-convo-wrapper')] //descendant:: li-icon[@type='cancel-icon']/ancestor::button"));
                }
                catch (Exception ex)
                {

                }
            }

            return conversationCardsCloseButtons;
        }

        public IReadOnlyCollection<IWebElement> GetAllConversationCloseButtons(IWebDriver webDriver)
        {
            return ConversationCardsCloseButtons(webDriver);
        }

        public IWebElement GetCloseConversationButton(IWebElement conversationPopup)
        {
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
                    closeButton = conversationPopup.FindElement(By.XPath("//div[contains(@class, 'msg-convo-wrapper')] //descendant:: li-icon[@type='cancel-icon']/ancestor::button"));
                }
                catch (Exception ex)
                {

                }
            }

            return closeButton;
        }
    }
}
