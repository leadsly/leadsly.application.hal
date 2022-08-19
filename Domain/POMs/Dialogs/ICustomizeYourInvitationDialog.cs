using OpenQA.Selenium;

namespace Domain.POMs.Dialogs
{
    public interface ICustomizeYourInvitationDialog
    {
        public IWebElement Content(IWebDriver webDriver);

        public bool SendConnection(IWebDriver webDriver);

        public void CloseDialog(IWebDriver webDriver);
    }
}
