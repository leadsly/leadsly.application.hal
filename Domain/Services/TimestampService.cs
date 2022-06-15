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

            DateTime nowLocalTime = TimeZoneInfo.ConvertTime(DateTime.Now, tzInfo);
            DateTimeOffset targetDateTimeOffset =
                new DateTimeOffset
                (
                    nowLocalTime,
                    tzInfo.GetUtcOffset
                    (
                        DateTime.SpecifyKind(nowLocalTime, DateTimeKind.Local)
                    )
                );

            return targetDateTimeOffset;
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
            TimeSpan ts = DateTime.Parse(timeOfDay).TimeOfDay;
            DateTime nowLocalTime = TimeZoneInfo.ConvertTime(DateTime.Now, tzInfo);
            DateTime targetDateTime = nowLocalTime.Date.AddTicks(ts.Ticks);

            DateTimeOffset targetDateTimeOffset =
                new DateTimeOffset
                (
                    targetDateTime,
                    tzInfo.GetUtcOffset
                    (
                        DateTime.SpecifyKind(targetDateTime, DateTimeKind.Local)
                    )   
                );

            return targetDateTimeOffset;
        }
    }
}
