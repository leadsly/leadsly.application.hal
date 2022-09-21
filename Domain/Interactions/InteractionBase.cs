using OpenQA.Selenium;

namespace Domain.Interactions
{
    public class InteractionBase : IInteraction
    {
        public IWebDriver WebDriver { get; set; }
    }
}
