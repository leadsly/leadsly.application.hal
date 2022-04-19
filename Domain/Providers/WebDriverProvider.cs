using Domain.Models;
using Domain.Providers.Interfaces;
using Domain.Repositories;
using Domain.Services;
using Domain.Services.Interfaces;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using Leadsly.Application.Model.WebDriver;
using Leadsly.Application.Model.WebDriver.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Providers
{
    public class WebDriverProvider : IWebDriverProvider
    {
        public WebDriverProvider(IWebDriverRepository webDriverRepository, IWebDriverService webDriverService, IMemoryCache memoryCache, ILogger<WebDriverProvider> logger)
        {
            _logger = logger;
            _webDriverRepository = webDriverRepository;
            _webDriverService = webDriverService;
            _memoryCache = memoryCache;
        }

        private readonly ILogger<WebDriverProvider> _logger;
        private readonly IWebDriverRepository _webDriverRepository;
        private readonly IMemoryCache _memoryCache;
        private readonly IWebDriverService _webDriverService;
        private readonly object _getWebDriverLock = new object();
        private static Dictionary<BrowserPurpose, IWebDriver> Drivers { get; set; } = new Dictionary<BrowserPurpose, IWebDriver>();
        public HalOperationResult<T> CloseTab<T>(BrowserPurpose browserPurpose, string windowHandleId) where T : IOperationResponse
        {
            string browser = Enum.GetName(browserPurpose);
            _logger.LogInformation("Closing WebDriver tab for browser purpose: {browser}", browser);

            HalOperationResult<T> result = new();
            HalOperationResult<IGetOrCreateWebDriverOperation> getWebDriverOperationResult = new();
            lock (_getWebDriverLock)
            {
                getWebDriverOperationResult = GetWebDriver<IGetOrCreateWebDriverOperation>(browserPurpose);
            }

            if(getWebDriverOperationResult.Succeeded == false)
            {
                result.Failures = getWebDriverOperationResult.Failures;
                return result;
            }

            return _webDriverService.CloseTab<T>(getWebDriverOperationResult.Value.WebDriver, windowHandleId);
        }

        private HalOperationResult<T> CreateWebDriver<T>(BrowserPurpose browserPurpose, string chromeProfileName)
            where T : IOperationResponse
        {
            WebDriverOptions webDriverOptions = GetWebDriverOptions();         
            return _webDriverService.Create<T>(browserPurpose, webDriverOptions, chromeProfileName);
        }
        
        private HalOperationResult<T> WebDriverDoesNotExist<T>(WebDriverOperationData operationData) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();
            // re-create the webdriver
            HalOperationResult<IGetOrCreateWebDriverOperation> createWebDriverResult = CreateWebDriver<IGetOrCreateWebDriverOperation>(operationData.BrowserPurpose, operationData.ChromeProfileName);
            if (createWebDriverResult.Succeeded == false)
            {
                result.Failures = createWebDriverResult.Failures;
                return result;
            }

            try
            {
                // add web driver to the list
                Drivers.Add(operationData.BrowserPurpose, createWebDriverResult.Value.WebDriver);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to add new webdriver to the list");
                result.Failures.Add(new()
                {
                    Code = Codes.WEBDRIVER_ERROR,
                    Reason = "Failed to add webdriver to the list",
                    Detail = ex.Message
                });
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        public HalOperationResult<T> CloseBrowser<T>(BrowserPurpose browserPurpose)
            where T : IOperationResponse
        {
            // always return succeeded here
            HalOperationResult<T> result = new()
            {
                Succeeded = true
            };

            HalOperationResult<IGetOrCreateWebDriverOperation> getWebDriverResult = GetWebDriver<IGetOrCreateWebDriverOperation>(browserPurpose);
            if(getWebDriverResult.Succeeded == false)
            {
                string purpose = Enum.GetName(browserPurpose);
                _logger.LogWarning("Failed to get web driver by purpose {purpose}", purpose);
                return result;
            }

            _webDriverService.Close<T>(getWebDriverResult.Value.WebDriver);
            RemoveWebDriver<T>(browserPurpose);

            result.Succeeded = true;
            return result;
        }

        private HalOperationResult<T> RemoveWebDriver<T>(BrowserPurpose browserPurpose) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();
            try
            {
                Drivers.Remove(browserPurpose);
            }
            catch(Exception ex)
            {
                result.Failures.Add(new()
                {
                    Reason = "Failed to remove web driver from the dictionary",
                    Detail = ex.Message
                });
            }
            finally
            {
                // always set succeeded to try
                result.Succeeded = true;
            }
            return result;
        }
        private HalOperationResult<T> AddWebDriver<T>(BrowserPurpose browserPurpose, IWebDriver webDriver) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();
            string browser = Enum.GetName(browserPurpose);
            try
            {
                _logger.LogTrace("Adding new WebDriver instance to Drivers list. Browser purpose is: {browser}", browser);
                Drivers.Add(browserPurpose, webDriver);
                _logger.LogTrace("Successfully added new WebDriver instance to Drivers list. Browser purpose is: {browser}", browser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add WebDriver instance to Drivers list. Browser purpose is: {browser}", browser);
                result.Failures.Add(new()
                {
                    Code = Codes.WEBDRIVER_MANAGEMENT_ERROR,
                    Reason = "Failed to Add web driver to the dictionary",
                    Detail = ex.Message
                });
                return result;
            }
            result.Succeeded = true;
            return result;
        }

        private HalOperationResult<T> GetWebDriver<T>(BrowserPurpose browserPurpose) where T : IOperationResponse
        {
            string browser = Enum.GetName(browserPurpose);
            _logger.LogInformation("Attempting to retrieve WebDriver from existing list by browser purpose: {browser}", browser);
            HalOperationResult<T> result = new();
            IWebDriver driver = default;
            try
            {
                // try and get the value         
                Drivers.TryGetValue(browserPurpose, out driver);
                if(driver == null)
                {
                    _logger.LogDebug("WebDriver does not exist in the list. New WebDriver instance will need to be created for browser purpose: {browser}", browser);
                    return result;
                }                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured getting WebDriver from the list by browser purpose: {browser}", browser);
                result.Failures.Add(new()
                {
                    Code = Codes.WEBDRIVER_ERROR,
                    Reason = "Failed to get WebDriver from the list",
                    Detail = ex.Message
                });
                return result;
            }

            _logger.LogDebug("WebDriver instance exist for browser purpose: {browser}", browser);
            IGetOrCreateWebDriverOperation operation = new GetOrCreateWebDriverOperation
            {
                WebDriver = driver
            };
            result.Value = (T)operation;
            result.Succeeded = true;
            return result;
        }

        private HalOperationResult<T> CannotEstablishConnectionToWebDriver<T>(WebDriverOperationData operationData) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();
            // re-create the webdriver
            HalOperationResult<IGetOrCreateWebDriverOperation> createWebDriverResult = CreateWebDriver<IGetOrCreateWebDriverOperation>(operationData.BrowserPurpose, operationData.ChromeProfileName);
            if (createWebDriverResult.Succeeded == false)
            {
                result.Failures = createWebDriverResult.Failures;
                return result;
            }

            try
            {
                // remove webdriver from the dictionary
                Drivers.Remove(operationData.BrowserPurpose);
                Drivers.Add(operationData.BrowserPurpose, createWebDriverResult.Value.WebDriver);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to remove then add webdriver to the list");
                result.Failures.Add(new()
                {
                    Code = Codes.WEBDRIVER_ERROR,
                    Reason = "Failed to remove then add webdriver to the list",
                    Detail = ex.Message
                });
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        private HalOperationResult<T> EnsureWebDriverIsAvailable<T>(WebDriverOperationData operationData) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();
            result = WebDriverExists<T>(operationData.BrowserPurpose);
            if (result.Succeeded == false)
            {
                result = WebDriverDoesNotExist<T>(operationData);
                if(result.Succeeded == false)
                {
                    return result;
                }
            }

            result = CheckWebDriverConnection<T>(operationData.BrowserPurpose);
            if(result.Succeeded == false)
            {
                result = CannotEstablishConnectionToWebDriver<T>(operationData);
                if(result.Succeeded == false)
                {
                    return result;
                }
            }

            result.Succeeded = true;
            return result;
        }        

        private WebDriverOptions GetWebDriverOptions()
        {
            _logger.LogInformation("Retrieving WebDriver options");

            WebDriverOptions webDriverOptions = default;
            if (_memoryCache.TryGetValue(CacheKeys.WebDriverOptions, out webDriverOptions) == false)
            {
                _logger.LogDebug("WebDriver options has not been yet loaded. Retrieving configuration options and saving them in memory.");
                webDriverOptions = _webDriverRepository.GetWebDriverOptions();
                _memoryCache.Set(CacheKeys.WebDriverOptions, webDriverOptions, TimeSpan.FromHours(16));
            }
            return webDriverOptions;
        }

        private HalOperationResult<T> WebDriverExists<T>(BrowserPurpose browserPurpose) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();
            string driverPurpose = Enum.GetName(browserPurpose);
            _logger.LogInformation("Checking if {driverPurpose} has been initialized already", driverPurpose);
            HalOperationResult<IGetOrCreateWebDriverOperation> getWebDriverResult = GetWebDriver<IGetOrCreateWebDriverOperation>(browserPurpose);
            if(getWebDriverResult.Succeeded == false)
            {
                result.Failures = getWebDriverResult.Failures;
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        private HalOperationResult<T> CheckWebDriverConnection<T>(BrowserPurpose browserPurpose) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();
            string driverPurpose = Enum.GetName(browserPurpose);            
            // we need to verify that we can successfully send commands to the web driver before we proceed, if we can't we need to re-create the web driver
            HalOperationResult<IGetOrCreateWebDriverOperation> getWebDriverResult = GetWebDriver<IGetOrCreateWebDriverOperation>(browserPurpose);
            if (getWebDriverResult.Succeeded == false)
            {
                result.Failures = getWebDriverResult.Failures;
                return result;
            }
            _logger.LogInformation("Checking if web driver is healthy and can accept commands");
            IWebDriver driver = getWebDriverResult.Value.WebDriver;
            _ = driver.Url;
            result.Succeeded = true;
            return result;
        }

        public HalOperationResult<T> GetOrCreateWebDriver<T>(WebDriverOperationData operationData) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            result = GetWebDriver<T>(operationData.BrowserPurpose);
            if(result.Succeeded == true)
            {
                return result;
            }

            result = CreateWebDriver<T>(operationData);
            if(result.Succeeded == true)
            {
                return result;
            }

            _logger.LogError("Failed to get or create web driver instance.");            
            return result;
        }

        public HalOperationResult<T> CreateWebDriver<T>(WebDriverOperationData operationData) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            HalOperationResult<IGetOrCreateWebDriverOperation> createWebDriverResult = CreateWebDriver<IGetOrCreateWebDriverOperation>(operationData.BrowserPurpose, operationData.ChromeProfileName);
            if (createWebDriverResult.Succeeded == false)
            {
                result.Failures = createWebDriverResult.Failures;
                return result;
            }
            IWebDriver webDriver = createWebDriverResult.Value.WebDriver;

            result = AddWebDriver<T>(operationData.BrowserPurpose, webDriver);
            if(result.Succeeded == false)
            {
                return result;
            }

            result.Succeeded = true;
            result.Value = (T)createWebDriverResult.Value;
            return result;
        }
        
        public HalOperationResult<T> GetWebDriver<T>(WebDriverOperationData operationData) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            result = WebDriverExists<T>(operationData.BrowserPurpose);
            if(result.Succeeded == false)
            {
                string browserPurpose = Enum.GetName(operationData.BrowserPurpose);
                result.Failures.Add(new()
                {
                    Code = Codes.WEBDRIVER_MANAGEMENT_ERROR,
                    Reason = "The requested web driver does not exist in the list",
                    Detail = $"Web driver for {browserPurpose} does not exist in the list"
                });
                return result;
            }

            result = CheckWebDriverConnection<T>(operationData.BrowserPurpose);
            if(result.Succeeded == false)
            {
                string browserPurpose = Enum.GetName(operationData.BrowserPurpose);
                result.Failures.Add(new()
                {
                    Code = Codes.WEBDRIVER_ERROR,
                    Reason = "The requested web driver is not responding to commands",
                    Detail = $"Web driver for {browserPurpose} is not responding to commands"
                });
                return result;
            }
            HalOperationResult<IGetOrCreateWebDriverOperation> getWebDriverResult = GetWebDriver<IGetOrCreateWebDriverOperation>(operationData.BrowserPurpose);            
            result.Value = (T)getWebDriverResult.Value;
            result.Succeeded = true;
            return result;
        }

        public HalOperationResult<T> SwitchTo<T>(IWebDriver webDriver, string windowHandleId) where T : IOperationResponse
        {
            _logger.LogInformation("Switching to window handle id {windowHandleId}", windowHandleId);
            HalOperationResult<T> result = new();
            try
            {
                string windownHandleToSwitchTo = webDriver.WindowHandles.Where(wH => wH == windowHandleId).FirstOrDefault();
                if(windownHandleToSwitchTo != null)
                {
                    _logger.LogInformation("Requested window handle was found! Switching to it.");
                    webDriver.SwitchTo().Window(windownHandleToSwitchTo);
                }
                else
                {
                    _logger.LogInformation("Requested window was not found. Keeping focus on the current tab.");
                    return result;
                }
                
            }
            catch(Exception ex)
            {
                result.Failures.Add(new()
                {
                    Reason = "Failed to switch tabs for the given web driver",
                    Detail = ex.Message
                });
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        public HalOperationResult<T> NewTab<T>(IWebDriver webDriver) where T : IOperationResponse
        {
            _logger.LogInformation("Attempting to create a new tab.");
            HalOperationResult<T> result = new();

            INewTabOperation operation = new NewTabOperation();
            try
            {
                _logger.LogTrace("Opening new tab");
                webDriver.SwitchTo().NewWindow(WindowType.Tab);
                _logger.LogTrace("New tab has been successfully created");

                operation.WindowHandleId = webDriver.CurrentWindowHandle;
                string windowHandleId = operation.WindowHandleId;
                _logger.LogTrace("New tab's window handle id is: {windowHandleId}", windowHandleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create a new tab for the given webdriver");
                return result;
            }

            result.Value = (T)operation;
            result.Succeeded = true;
            return result;
        }
    }
}
