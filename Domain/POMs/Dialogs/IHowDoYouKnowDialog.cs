using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.POMs.Dialogs
{
    public interface IHowDoYouKnowDialog
    {
        public IList<IWebElement> SelectionChoices(IWebDriver webDriver);

        public bool SelectChoice(HowDoYouKnowChoice choice, IList<IWebElement> choices);

        public bool VerifySelection(HowDoYouKnowChoice choice, IList<IWebElement> choices);
        public bool SendConnection(IWebDriver webDriver);
        public void CloseDialog(IWebDriver webDriver);
    }
}
