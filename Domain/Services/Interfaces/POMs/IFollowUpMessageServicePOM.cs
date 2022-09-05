using OpenQA.Selenium;

namespace Domain.Services.Interfaces.POMs
{
    public interface IFollowUpMessageServicePOM
    {
        bool ClickCreateNewMessage(IWebDriver webDriver);
        bool EnterProspectName(IWebDriver webDriver, string prospectName);

        bool EnterMessage(IWebDriver webDriver, string content);
    }
}
