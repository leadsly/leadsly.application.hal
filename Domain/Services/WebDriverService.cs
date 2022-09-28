using Domain.Models;
using Domain.Models.Requests;
using Domain.Services.Interfaces;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using Leadsly.Application.Model.WebDriver;
using Leadsly.Application.Model.WebDriver.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Domain.Services
{
    public class WebDriverService : IWebDriverService
    {
        public WebDriverService(ILeadslyGridSidecartService leadslyGridSidecartService, ILogger<WebDriverService> logger, IFileManager fileManager, IWebHostEnvironment env)
        {
            _logger = logger;
            _fileManager = fileManager;
            _env = env;
            _leadslyGridSidecartService = leadslyGridSidecartService;
        }

        private readonly IWebHostEnvironment _env;
        private readonly ILogger<WebDriverService> _logger;
        private readonly ILeadslyGridSidecartService _leadslyGridSidecartService;
        private readonly IFileManager _fileManager;

        public HalOperationResult<T> CloseTab<T>(IWebDriver driver, string windowHandleId) where T : IOperationResponse
        {
            _logger.LogInformation("Closing WebDriver tab by windowHandleId: {windowHandleId}", windowHandleId);
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

        public IWebDriver Create(ChromeOptions options, WebDriverOptions webDriverOptions, string gridNamespaceName, string gridCloudMapServiceName)
        {
            IWebDriver driver = null;
            try
            {
                _logger.LogTrace("Creating new WebDriver instance");
                if (webDriverOptions != null && webDriverOptions.UseGrid == false)
                {
                    driver = new ChromeDriver(options);
                }
                else
                {
                    string seleniumGridUrl = string.Empty;
                    if (_env.IsDevelopment() || _env.IsStaging())
                    {
                        seleniumGridUrl = $"{webDriverOptions.SeleniumGrid.Url}:{webDriverOptions.SeleniumGrid.Port}";
                    }
                    else
                    {
                        seleniumGridUrl = $"http://{gridCloudMapServiceName}.{gridNamespaceName}:{webDriverOptions.SeleniumGrid.Port}";

                    }
                    _logger.LogDebug("Remote Grid url is: {seleniumGridUrl}", seleniumGridUrl);
                    driver = new RemoteWebDriver(new Uri(seleniumGridUrl), options);
                }

                _logger.LogTrace("New WebDriver instance successfully created");

                _logger.LogTrace("Maximizing WebDriver's main window.");
                driver.Manage().Window.Maximize();
                _logger.LogTrace("WebDriver main window was successfully maximized.");
                long implicitWait = webDriverOptions.DefaultImplicitWait;
                _logger.LogTrace("Setting WebDriver ImplicitWait to {implicitWait}", implicitWait);
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(implicitWait);
                long pageLoadTimeout = webDriverOptions.PagLoadTimeout;
                _logger.LogTrace("Setting webdriver's page load timeout to {pageLoadTimeout}", pageLoadTimeout);
                driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(pageLoadTimeout);
                _logger.LogTrace("Successfully set WebDriver's ImplicitWait");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create new WebDriver instance");
                throw ex;
            }

            return driver;
        }

        public ChromeOptions SetChromeOptions(WebDriverOptions webDriverOptions, string proxyNamespaceName, string proxyServiceDiscoveryName, string chromeProfile, string defaultChromeProfileDir)
        {
            _logger.LogInformation("Setting chrome options. Chrome profile name: {chromeProfile}. UserDataDir is: {defaultChromeProfileDir}", chromeProfile, defaultChromeProfileDir);

            ChromeOptions options = new();
            foreach (string addArgument in webDriverOptions.ChromeProfileConfigOptions.AddArguments)
            {
                _logger.LogDebug("New WebDriver argument: {addArgument}", addArgument);
                options.AddArgument(addArgument);
            }

            _logger.LogDebug($"Setting --user-data-dir to {defaultChromeProfileDir}/{chromeProfile}");
            options.AddArgument(@$"--user-data-dir={defaultChromeProfileDir}/{chromeProfile}");

            string httpProxyUrl = _env.IsProduction() ? $"http://{proxyServiceDiscoveryName}.{proxyNamespaceName}" : webDriverOptions.ChromeProfileConfigOptions.Proxy.HttpProxy;

            // options.AddArguments($"--proxy-server={httpProxyUrl}");

            //options.Proxy = new()
            //{
            //    Kind = ProxyKind.Manual,
            //    HttpProxy = _env.IsProduction() ? $"http://{proxyServiceDiscoveryName}.{proxyNamespaceName}" : webDriverOptions.ChromeProfileConfigOptions.Proxy.HttpProxy,
            //    SslProxy = "http://localhost:5078"
            //};

            return options;
        }

        public HalOperationResult<T> Create<T>(BrowserPurpose browserPurpose, WebDriverOptions webDriverOptions, string chromeProfileName, string gridNamespaceName, string gridServiceDiscoveryName) where T : IOperationResponse
        {
            string browser = Enum.GetName(browserPurpose);
            _logger.LogInformation("Creating a new WebDriver instance for browser purpose {browser}", browser);
            HalOperationResult<T> result = new();
            ChromeOptions options = null;
            string newChromeProfileDir = string.Empty;
            if (browserPurpose == BrowserPurpose.Auth)
            {
                options = SetChromeOptions(webDriverOptions, webDriverOptions.ChromeProfileConfigOptions.DefaultChromeProfileName, webDriverOptions.ChromeProfileConfigOptions.DefaultChromeUserProfilesDir);
            }
            else
            {
                string newChromeProfileName = chromeProfileName + "_" + browser;

                // result = _fileManager.CloneDefaultChromeProfile<T>(newChromeProfileName, webDriverOptions);
                Task<HttpResponseMessage> task = Task.Run(() =>
                {
                    CloneChromeProfileRequest req = new()
                    {
                        GridNamespaceName = gridNamespaceName,
                        GridServiceDiscoveryName = gridServiceDiscoveryName,
                        RequestUrl = "FileManager/clone-profile",
                        DefaultChromeProfileName = webDriverOptions.ChromeProfileConfigOptions.DefaultChromeProfileName,
                        DefaultChromeUserProfilesDir = webDriverOptions.ChromeProfileConfigOptions.DefaultChromeUserProfilesDir,
                        NewChromeProfile = newChromeProfileName,
                        ProfilesVolume = webDriverOptions.ProfilesVolume,
                        UseGrid = webDriverOptions.UseGrid
                    };

                    return _leadslyGridSidecartService.CloneChromeProfileAsync(req);
                });
                task.Wait();
                HttpResponseMessage resp = task.Result;

                if (resp.IsSuccessStatusCode == false)
                {
                    return result;
                }
                newChromeProfileDir = webDriverOptions.ChromeProfileConfigOptions.DefaultChromeUserProfilesDir + "/" + newChromeProfileName;

                _logger.LogDebug("New chrome profile directory that will be used is: {newChromeProfileDir}", newChromeProfileDir);

                options = SetChromeOptions(webDriverOptions, newChromeProfileName, webDriverOptions.ChromeProfileConfigOptions.DefaultChromeUserProfilesDir);
            }

            return Create<T>(options, webDriverOptions, gridNamespaceName, gridServiceDiscoveryName);
        }

        private HalOperationResult<T> Create<T>(ChromeOptions options, WebDriverOptions webDriverOptions, string gridNamespaceName, string gridCloudMapServiceName) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();
            IWebDriver driver = null;
            try
            {
                _logger.LogTrace("Creating new WebDriver instance");
                if (webDriverOptions != null && webDriverOptions.UseGrid == false)
                {
                    driver = new ChromeDriver(options);
                }
                else
                {
                    string seleniumGridUrl = string.Empty;
                    if (_env.IsDevelopment() || _env.IsStaging())
                    {
                        seleniumGridUrl = $"{webDriverOptions.SeleniumGrid.Url}:{webDriverOptions.SeleniumGrid.Port}";
                    }
                    else
                    {
                        seleniumGridUrl = $"http://{gridCloudMapServiceName}.{gridNamespaceName}:{webDriverOptions.SeleniumGrid.Port}";

                    }
                    _logger.LogDebug("Remote Grid url is: {seleniumGridUrl}", seleniumGridUrl);
                    driver = new RemoteWebDriver(new Uri(seleniumGridUrl), options);
                }

                _logger.LogTrace("New WebDriver instance successfully created");

                _logger.LogTrace("Maximizing WebDriver's main window.");
                driver.Manage().Window.Maximize();
                _logger.LogTrace("WebDriver main window was successfully maximized.");
                long implicitWait = webDriverOptions.DefaultImplicitWait;
                _logger.LogTrace("Setting WebDriver ImplicitWait to {implicitWait}", implicitWait);
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(implicitWait);
                long pageLoadTimeout = webDriverOptions.PagLoadTimeout;
                _logger.LogTrace("Setting webdriver's page load timeout to {pageLoadTimeout}", pageLoadTimeout);
                driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(pageLoadTimeout);
                _logger.LogTrace("Successfully set WebDriver's ImplicitWait");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create new WebDriver instance");

                // remove created profile
                //if(newChromeProfileDir != null)
                //    _fileManager.RemoveDirectory<T>(newChromeProfileDir);

                result.Failures.Add(new()
                {
                    Code = Codes.WEBDRIVER_ERROR,
                    Detail = ex.Message,
                    Reason = "Failed to create web driver instance"
                });
                return result;
            }

            IGetOrCreateWebDriverOperation operation = new GetOrCreateWebDriverOperation
            {
                WebDriver = driver,
            };

            result.Value = (T)operation;
            result.Succeeded = true;
            return result;
        }

        private ChromeOptions SetChromeOptions(WebDriverOptions webDriverOptions, string profileName, string userDataDir)
        {
            _logger.LogInformation("Setting chrome options. Chrome profile name: {profileName}. UserDataDir is: {userDataDir}", profileName, userDataDir);

            ChromeOptions options = new();
            foreach (string addArgument in webDriverOptions.ChromeProfileConfigOptions.AddArguments)
            {
                _logger.LogDebug("New WebDriver argument: {addArgument}", addArgument);
                options.AddArgument(addArgument);
            }

            _logger.LogDebug($"Setting --user-data-dir to {userDataDir}/{profileName}");
            options.AddArgument(@$"--user-data-dir={userDataDir}/{profileName}");

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
            catch (Exception ex)
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
            catch (Exception ex)
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
