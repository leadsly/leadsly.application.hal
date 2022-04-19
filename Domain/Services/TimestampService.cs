﻿using Domain.Services.Interfaces;
using Leadsly.Application.Model.Entities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services
{
    public class TimestampService : ITimestampService
    {
        public TimestampService(ILogger<TimestampService> logger, IMemoryCache memCache)
        {
            _logger = logger;
            _memoryCache = memCache;
        }

        private readonly ILogger<TimestampService> _logger;
        private readonly IMemoryCache _memoryCache;

        public DateTimeOffset GetDateTimeNowWithZone(string zoneId)
        {
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(zoneId);

            DateTime localDateTime = new DateTimeWithZone(DateTime.Now, timeZoneInfo).LocalTime;

            return new DateTimeOffset(localDateTime);
        }
    }
}