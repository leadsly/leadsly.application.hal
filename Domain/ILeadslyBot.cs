using OpenQA.Selenium;
using PageObjects.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public interface ILeadslyBot
    {
        LinkedInLoginPage Authenticate(IWebDriver driver, string email, string password);

        LinkedInPage GoToLinkedIn(IWebDriver driver);
    }
}
