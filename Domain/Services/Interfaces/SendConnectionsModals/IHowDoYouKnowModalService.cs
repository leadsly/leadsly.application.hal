using OpenQA.Selenium;

namespace Domain.Services.Interfaces.SendConnectionsModals
{
    public interface IHowDoYouKnowModalService
    {
        public bool HandleInteraction(IWebDriver webDriver);

        public void CloseDialog(IWebDriver webDriver);
    }
}
