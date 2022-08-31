using OpenQA.Selenium;

namespace Domain.Services.Interfaces.POMs
{
    public interface IHowDoYouKnowModalService
    {
        public bool HandleInteraction(IWebDriver webDriver);

        public void CloseDialog(IWebDriver webDriver);
    }
}
