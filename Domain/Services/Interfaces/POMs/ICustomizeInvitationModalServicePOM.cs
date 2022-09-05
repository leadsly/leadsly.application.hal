using OpenQA.Selenium;

namespace Domain.Services.Interfaces.POMs
{
    public interface ICustomizeInvitationModalServicePOM
    {
        bool HandleInteraction(IWebDriver webDriver);

        void CloseDialog(IWebDriver webDriver);
    }
}
