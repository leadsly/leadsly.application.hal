using Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;

namespace Domain.Services
{
    public class TimestampService : ITimestampService
    {
        public TimestampService(ILogger<TimestampService> logger)
        {
            _logger = logger;
        }

        private readonly ILogger<TimestampService> _logger;

        public DateTime GetNowLocalized(string zoneId)
        {
            _logger.LogInformation("Executing GetDateTimeNowWithZone for zone {zoneId}", zoneId);
            DateTime nowLocal = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, zoneId);
            _logger.LogInformation($"Now in local zone {zoneId} is {nowLocal.Date}");
            return nowLocal;
        }

        public DateTime GetDateTimeWithZone(string zoneId, long timestamp)
        {
            _logger.LogInformation("Executing GetDateTimeWithZone for zone {zoneId} and timestamp {timestamp}", zoneId, timestamp);
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(zoneId);
            DateTimeOffset timestampOffSet = DateTimeOffset.FromUnixTimeSeconds(timestamp);
            DateTime localDateTime = new DateTimeWithZone(timestampOffSet.DateTime, timeZoneInfo).LocalTime;
            _logger.LogInformation($"Local date time in zone {zoneId} is {localDateTime.Date}");

            return localDateTime;
        }

        public long TimestampNow()
        {
            _logger.LogInformation("Executing timestamp now");
            return DateTimeOffset.Now.ToUnixTimeSeconds();
        }

        public long TimestampFromDateTimeOffset(DateTimeOffset dateTimeOffset)
        {
            _logger.LogInformation("Executing timestamp from datetimeoffset");
            return dateTimeOffset.ToUnixTimeSeconds();
        }
    }
}
