﻿using Domain.Models;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services
{
    public class WebDriverService : IWebDriverService
    {
        public WebDriverService(ILogger<WebDriverService> logger, IWebDriver driver, IDefaultTabWebDriver defaultTabWebDriver)
        {
            _logger = logger;
            _defaultTabWebDriver = defaultTabWebDriver;
            _driver = driver;
        }

        private readonly ILogger<WebDriverService> _logger;
        private readonly IWebDriver _driver;
        private readonly IDefaultTabWebDriver _defaultTabWebDriver;

        public HalOperationResult<T> CloseTab<T>(string windowHandleId) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();
            string windowHandleToClose = string.Empty;
            try
            {
                windowHandleToClose = _driver.WindowHandles.FirstOrDefault(wH => wH == windowHandleId);
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
                    if (_driver.CurrentWindowHandle != windowHandleToClose)
                    {
                        _logger.LogInformation("Web driver is not on the window that was requested to be closed. Switching to that window");
                        // switch to the window handle to close
                        _driver.SwitchTo().Window(windowHandleToClose);
                    }
                    _logger.LogInformation("Closing web driver window requested to be closed");
                    _driver.Close();
                    result.Value.WindowTabClosed = true;

                    // ensure default tab window is still available
                    string defaultTabWindow = _driver.WindowHandles.FirstOrDefault(wH => wH == _defaultTabWebDriver.DefaultTabWindowHandleId);
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
                        _driver.SwitchTo().Window(_defaultTabWebDriver.DefaultTabWindowHandleId);
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

        public IWebDriverInformation Create(ChromeOptions options, long implicitDefaultTimeout)
        {
            WebDriverInformation result = new()
            {
                Succeeded = false
            };

            IWebDriver driver = null;
            try
            {
                // [OMikolajczyk_3-1-2022] Keeping as proof of concept for how to request new web drivers.
                // string chromeProfileName = $"Chrome_Profile_{Guid.NewGuid()}";
                // copy template profile into default chrome directory and re-name it                
                // IOperationResult copyResult = _fileManager.CloneDefaultChromeProfile(chromeProfileName, webDriverOptions);
                driver = new ChromeDriver(options);
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(implicitDefaultTimeout);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create new web driver.");
                result.Succeeded = false;
                result.Failures.Add(new()
                {
                    Code = Codes.WEBDRIVER_ERROR,
                    Detail = ex.Message,
                    Reason = "Failed to create web driver instance"
                });
                return result;
            }


            result.WebDriverId = Guid.NewGuid().ToString();
            result.Succeeded = true;
            return result;
        }

        public HalOperationResult<T> SwitchTo<T>(string requestedWindowHandle, out string currentWindowHandle) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            if (requestedWindowHandle == null)
            {
                _driver.SwitchTo().NewWindow(WindowType.Tab);
            }
            else
            {
                // check if requested window handle exists
                string wH = _driver.WindowHandles.FirstOrDefault(wH => wH == requestedWindowHandle);
                if (wH == null)
                {
                    _logger.LogError("Web driver does not have any windows open that match the requested window");
                    result.Failures.Add(new()
                    {
                        Code = Codes.WEBDRIVER_WINDOW_LOCATION_ERROR,
                        Reason = "Failed to find the requested window handle",
                        Detail = $"Web driver could not find any tabs that match {requestedWindowHandle}"
                    });
                    currentWindowHandle = string.Empty;
                    return result;
                }
                // only switch tabs if current window handle id does not equal requested handle id
                if (_driver.CurrentWindowHandle != wH)
                {
                    _driver.SwitchTo().Window(wH);
                }
            }
            currentWindowHandle = _driver.CurrentWindowHandle;
            result.Succeeded = true;
            return result;
        }
    }
}
