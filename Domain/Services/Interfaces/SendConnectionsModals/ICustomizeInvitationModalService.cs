using OpenQA.Selenium;

namespace Domain.Services.Interfaces.SendConnections
{
    public interface ICustomizeInvitationModalService
    {
        bool HandleInteraction(IWebDriver webDriver);

        void CloseDialog(IWebDriver webDriver);
    }
}
