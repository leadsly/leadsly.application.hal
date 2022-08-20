using OpenQA.Selenium;

namespace Domain.POMs.Dialogs
{
    public interface ISearchPageDialogManager
    {
        bool HandleConnectWithProspectModal(IWebDriver webDriver);

        void TryCloseModal(IWebDriver webDriver);
    }
}
