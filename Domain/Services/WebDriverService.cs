using Domain.Models;
using Domain.Services.Interfaces;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using Leadsly.Application.Model.WebDriver;
using Leadsly.Application.Model.WebDriver.Interfaces;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services
{
    public class WebDriverService : IWebDriverService
    {
        public WebDriverService(ILogger<WebDriverService> logger, IFileManager fileManager)
        {
            _logger = logger;
            _fileManager = fileManager;
        }

        private readonly ILogger<WebDriverService> _logger;
        private readonly IDefaultTabWebDriver _defaultTabWebDriver;
        private readonly IFileManager _fileManager;        
        private const long DefaultImplicitWait = 10;

        public HalOperationResult<T> CloseTab<T>(IWebDriver driver, string windowHandleId) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();
            string windowHandleToClose = string.Empty;
            try
            {
                windowHandleToClose = driver.WindowHandles.FirstOrDefault(wH => wH == windowHandleId);
                if (windowHandleToClose == null)
                {
                    _logger.LogWarning("Attempted to close window with handle id {windowHandleId}, current instance of the web driver does not have this window handle", windowHandleId);
                    result.Failures.Add(new()
                    {
                        Reason = "Couldn't locate the specified window",
                        Detail = $"Failed to find web driver window with the id {windowHandleId}"
                    });
                    return result;
                }
                else
                {
                    // ensure we are on the window we are trying to close
                    if (driver.CurrentWindowHandle != windowHandleToClose)
                    {
                        _logger.LogInformation("Web driver is not on the window that was requested to be closed. Switching to that window");
                        // switch to the window handle to close
                        driver.SwitchTo().Window(windowHandleToClose);
                    }
                    _logger.LogInformation("Closing web driver window requested to be closed");
                    driver.Close();

                    // ensure default tab window is still available
                    string defaultTabWindow = driver.WindowHandles.FirstOrDefault(wH => wH == _defaultTabWebDriver.DefaultTabWindowHandleId);
                    if (defaultTabWindow == null)
                    {
                        _logger.LogWarning("Web driver's default blank tab is not found, it might have been closed on accident");
                        result.Failures.Add(new()
                        {
                            Reason = "Couldn't locate the default blank tab",
                            Detail = "Failed to locate web driver's default blank tab"
                        });
                        return result;
                    }
                    else
                    {
                        driver.SwitchTo().Window(_defaultTabWebDriver.DefaultTabWindowHandleId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Closing web driver's tab failed");
                result.Succeeded = false;
                result.Failures.Add(new()
                {
                    Code = Codes.WEBDRIVER_ERROR,
                    Reason = $"Failed to close web driver's tab with the id {windowHandleToClose}",
                    Detail = ex.Message
                });
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        public HalOperationResult<T> Create<T>(BrowserPurpose browserPurpose, WebDriverOptions webDriverOptions) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();
            ChromeOptions options = null;
            string newChromeProfileDir = string.Empty;
            if (browserPurpose == BrowserPurpose.Auth)
            {
                // use default user-data-dir profile to authenticate, this will be the profile used later on to copy
                options = SetChromeOptions(webDriverOptions, webDriverOptions.ChromeProfileConfigOptions.DefaultChromeProfileName, webDriverOptions.ChromeProfileConfigOptions.DefaultChromeUserProfilesDir);
            }
            else
            {
                string newChromeProfileName = Guid.NewGuid().ToString();

                result = _fileManager.CloneDefaultChromeProfile<T>(newChromeProfileName, webDriverOptions);                

                if(result.Succeeded == false)
                {
                    return result;
                }
                newChromeProfileDir = webDriverOptions.ChromeProfileConfigOptions.DefaultChromeUserProfilesDir + newChromeProfileName;
                options = SetChromeOptions(webDriverOptions, newChromeProfileName, webDriverOptions.ChromeProfileConfigOptions.DefaultChromeUserProfilesDir);                
            }

            return Create<T>(options, webDriverOptions.DefaultImplicitWait, newChromeProfileDir);            
        }

        private HalOperationResult<T> Create<T>(ChromeOptions options, long implicitWait = DefaultImplicitWait, string newChromeProfileDir = null) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();
            IWebDriver driver = null;
            try
            {
                IEnumerable<int> pidsBefore = Process.GetProcessesByName("chrome").Select(p => p.Id);

                driver = new ChromeDriver(options);

                IEnumerable<int> pidsAfter = Process.GetProcessesByName("chrome").Select(p => p.Id);
                IEnumerable<int> newChromePids = pidsAfter.Except(pidsBefore);

                driver.Manage().Window.Maximize();
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(implicitWait);                
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to create new web driver.");

                // remove created profile
                if(newChromeProfileDir != null)
                    _fileManager.RemoveDirectory<T>(newChromeProfileDir);

                result.Failures.Add(new()
                {
                    Code = Codes.WEBDRIVER_ERROR,
                    Detail = ex.Message,
                    Reason = "Failed to create web driver instance"
                });
                return result;
            }

            ICreateWebDriverOperation operation = new CreateWebDriverOperation
            {
                WebDriver = driver,
            };

            result.Value = (T)operation;
            result.Succeeded = true;
            return result;
        }

        private ChromeOptions SetChromeOptions(WebDriverOptions webDriverOptions, string profileName, string userDataDir)
        {
            ChromeOptions options = new();
            //options.AddArgument("--disable-blink-features=AutomationControlled");
            //options.AddArgument("window-size=1280,800");
            //options.AddArgument("user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36");
            foreach (string addArgument in webDriverOptions.ChromeProfileConfigOptions.AddArguments)
            {
                options.AddArgument(addArgument);
            }
            options.AddArgument(@$"user-data-dir={userDataDir}\{profileName}");            

            return options;
        }

        public HalOperationResult<T> SwitchTo<T>(IWebDriver webDriver, WebDriverOperationData operationData, out string currentWindowHandleId) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();
            currentWindowHandleId = string.Empty;            
            try
            {
                if (operationData.RequestedWindowHandleId == null)
                {
                    webDriver.SwitchTo().NewWindow(WindowType.Tab);
                }
                else
                {
                    // check if requested window handle exists
                    string wH = webDriver.WindowHandles.FirstOrDefault(wH => wH == operationData.RequestedWindowHandleId);
                    if (wH == null)
                    {
                        _logger.LogError("Web driver does not have any windows open that match the requested window");
                        result.Failures.Add(new()
                        {
                            Code = Codes.WEBDRIVER_WINDOW_LOCATION_ERROR,
                            Reason = "Failed to find the requested window handle",
                            Detail = $"Web driver could not find any tabs that match {operationData.RequestedWindowHandleId}"
                        });                        
                        return result;
                    }
                    // only switch tabs if current window handle id does not equal requested handle id
                    if (webDriver.CurrentWindowHandle != wH)
                    {
                        webDriver.SwitchTo().Window(wH);
                    }
                }
            }
            catch(Exception ex)
            {
                result.Failures.Add(new()
                {
                    Reason = "Web driver failed to switch tabs",
                    Detail = ex.Message
                });                
                return result;
            }

            currentWindowHandleId = webDriver.CurrentWindowHandle;
            result.Succeeded = true;
            return result;
        }

        public HalOperationResult<T> Close<T>(IWebDriver driver) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();
            try
            {
                // close all sessions of this webdriver
                driver.Dispose();              
            }
            catch(Exception ex)
            {
                result.Failures.Add(new()
                {
                    Reason = "Failed to close web driver",
                    Detail = ex.Message
                });
                return result;
            }
            result.Succeeded = true;
            return result;
        }
    }
}
