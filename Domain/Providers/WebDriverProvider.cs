﻿using Domain.Models;
using Domain.Repositories;
using Domain.Services;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using Leadsly.Application.Model.WebDriver;
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
        private readonly object _webDriverOperationLock = new object();
        private readonly object _getWebDriverOperation = new object();
        private static Dictionary<BrowserPurpose, IWebDriver> Drivers { get; set; } = new Dictionary<BrowserPurpose, IWebDriver>();
        public HalOperationResult<T> CloseTab<T>(BrowserPurpose browserPurpose, string windowHandleId) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();
            HalOperationResult<IGetWebDriverOperation> getWebDriverOperationResult = new();
            lock (_getWebDriverOperation)
            {
                getWebDriverOperationResult = GetWebDriver<IGetWebDriverOperation>(browserPurpose);
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
            WebDriverOptions webDriverOptions = GetWebDriverOptions(chromeProfileName);         
            return _webDriverService.Create<T>(browserPurpose, webDriverOptions);
        }
        
        private HalOperationResult<T> WebDriverDoesNotExist<T>(WebDriverOperationData operationData) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();
            // re-create the webdriver
            HalOperationResult<IGetWebDriverOperation> createWebDriverResult = CreateWebDriver<IGetWebDriverOperation>(operationData.BrowserPurpose, operationData.ChromeProfileName);
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

            HalOperationResult<IGetWebDriverOperation> getWebDriverResult = GetWebDriver<IGetWebDriverOperation>(browserPurpose);
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
            try
            {
                Drivers.Add(browserPurpose, webDriver);
            }
            catch (Exception ex)
            {
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
            HalOperationResult<T> result = new();
            IWebDriver driver = default;
            try
            {
                // try and get the value         
                Drivers.TryGetValue(browserPurpose, out driver);
                if(driver == null)
                {
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get new webdriver to the list");
                result.Failures.Add(new()
                {
                    Code = Codes.WEBDRIVER_ERROR,
                    Reason = "Failed to get webdriver to the list",
                    Detail = ex.Message
                });
                return result;
            }

            IGetWebDriverOperation operation = new GetWebDriverOperation
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
            HalOperationResult<IGetWebDriverOperation> createWebDriverResult = CreateWebDriver<IGetWebDriverOperation>(operationData.BrowserPurpose, operationData.ChromeProfileName);
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

        private WebDriverOptions GetWebDriverOptions(string chromeProfileName)
        {
            WebDriverOptions webDriverOptions = default;
            if (_memoryCache.TryGetValue(CacheKeys.WebDriverOptions, out webDriverOptions) == false)
            {
                webDriverOptions = _webDriverRepository.GetWebDriverOptions();
                webDriverOptions.ChromeProfileConfigOptions.ChromeProfileName = chromeProfileName ?? string.Empty;
                _memoryCache.Set(CacheKeys.WebDriverOptions, webDriverOptions, TimeSpan.FromHours(16));
            }
            return webDriverOptions;
        }

        private HalOperationResult<T> WebDriverExists<T>(BrowserPurpose browserPurpose) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();
            string driverPurpose = Enum.GetName(browserPurpose);
            _logger.LogInformation("Checking if {driverPurpose} has been initialized already", driverPurpose);
            HalOperationResult<IGetWebDriverOperation> getWebDriverResult = GetWebDriver<IGetWebDriverOperation>(browserPurpose);
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
            HalOperationResult<IGetWebDriverOperation> getWebDriverResult = GetWebDriver<IGetWebDriverOperation>(browserPurpose);
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

        public HalOperationResult<T> CreateWebDriver<T>(WebDriverOperationData operationData) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            HalOperationResult<ICreateWebDriverOperation> createWebDriverResult = CreateWebDriver<ICreateWebDriverOperation>(operationData.BrowserPurpose, operationData.ChromeProfileName);
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

            ICreateWebDriverOperation operation = new CreateWebDriverOperation
            {
                WebDriver = webDriver
            };

            result.Succeeded = true;
            result.Value = (T)operation;
            return result;
        }
    }
}
