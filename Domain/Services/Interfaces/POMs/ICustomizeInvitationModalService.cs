namespace Domain.Services.Interfaces.POMs
{
    public interface ICustomizeInvitationModalService
    {
        bool HandleInteraction(IWebDriver webDriver);

        void CloseDialog(IWebDriver webDriver);
    }
}
