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

        public DateTimeOffset GetNowLocalized(string zoneId)
        {
            TimeZoneInfo tzInfo = TimeZoneInfo.FindSystemTimeZoneById(zoneId);
            _logger.LogInformation("Executing GetDateTimeNowWithZone for zone {zoneId}", zoneId);

            DateTime nowLocalTime = TimeZoneInfo.ConvertTime(DateTime.Now, tzInfo);
            DateTimeOffset targetDateTimeOffset =
                new DateTimeOffset
                (
                    DateTime.SpecifyKind(nowLocalTime, DateTimeKind.Unspecified
                ),
                tzInfo.GetUtcOffset
                (
                    DateTime.SpecifyKind(nowLocalTime, DateTimeKind.Local)
                ));

            
            _logger.LogInformation($"Now in local zone {zoneId} is {targetDateTimeOffset}");
            return targetDateTimeOffset;
        }

        public DateTimeOffset GetDateTimeOffsetLocal(string zoneId, long timestamp)
        {
            _logger.LogInformation("Executing GetDateTimeWithZone for zone {zoneId} and timestamp {timestamp}", zoneId, timestamp);
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(zoneId);
            DateTimeOffset timestampOffSet = DateTimeOffset.FromUnixTimeSeconds(timestamp);
            DateTimeOffset targetTime = TimeZoneInfo.ConvertTime(timestampOffSet, timeZoneInfo);
            _logger.LogInformation($"Local date time in zone {zoneId} is {targetTime.Date}");

            return targetTime;
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

        public DateTimeOffset ParseDateTimeOffsetLocalized(string zoneId, string timeOfDay)
        {
            TimeZoneInfo tzInfo = TimeZoneInfo.FindSystemTimeZoneById(zoneId);
            if (DateTime.TryParse(timeOfDay, out DateTime dateTime) == false)
            {
                string startTime = timeOfDay;
                _logger.LogError("Failed to parse Networking start time. Tried to parse {startTime}", startTime);
            }

            DateTimeOffset targetDateTimeOffset =
                new DateTimeOffset
                (
                    DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified
                ),
                tzInfo.GetUtcOffset
                (
                    DateTime.SpecifyKind(dateTime, DateTimeKind.Local)
                ));

            return targetDateTimeOffset;
        }
    }
}
