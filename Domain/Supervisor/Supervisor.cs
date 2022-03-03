using Domain.Models;
using Domain.Providers;
using Domain.Repositories;
using Leadsly.Application.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace Domain.Supervisor
{
    public partial class Supervisor : ISupervisor
    {
        public Supervisor(
            IWebDriverRepository webDriverRepository, 
            IHalAuthProvider halAuthProvider, 
            IWebDriver driver, 
            IFileManager fileManager, 
            IWebDriverProvider webDriverProvider,
            IMemoryCache memoryCache, 
            ILogger<Supervisor> logger)
        {
            _logger = logger;
            _memoryCache = memoryCache;
            _fileManager = fileManager;
            _webDriverRepository = webDriverRepository;
            _halAuthProvider = halAuthProvider;
            _driver = driver;
            _webDriverProvider = webDriverProvider;
        }

        private readonly IWebDriver _driver;
        private readonly IWebDriverProvider _webDriverProvider;
        private readonly IHalAuthProvider _halAuthProvider;
        private readonly ILogger<Supervisor> _logger;
        private readonly IFileManager _fileManager;
        private readonly IMemoryCache _memoryCache;
        private readonly IWebDriverRepository _webDriverRepository;

    }
}
