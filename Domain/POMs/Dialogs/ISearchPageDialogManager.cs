using OpenQA.Selenium;

namespace Domain.POMs.Dialogs
{
    public interface ISearchPageDialogManager
    {
        bool IsConnectWithProspectModalOpen(IWebDriver webDriver);

        SendInviteModalType DetermineSendInviteModalType(IWebDriver webDriver);

        SendInviteModalType CheckModalType(IWebDriver webDriver);
    }
}
