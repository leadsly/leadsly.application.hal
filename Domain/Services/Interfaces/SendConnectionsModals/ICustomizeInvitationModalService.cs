using OpenQA.Selenium;

namespace Domain.Services.Interfaces.SendConnectionsModals
{
    public interface ICustomizeInvitationModalService
    {
        bool HandleInteraction(IWebDriver webDriver);

        void CloseDialog(IWebDriver webDriver);
    }
}
