using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface ITimestampService
    {
        DateTimeOffset GetDateTimeNowWithZone(string zoneId);

        DateTimeOffset GetDateTimeWithZone(string zoneId, long timestamp);

        long TimestampNowWithZone(string zoneId);

        long TimestampFromDateTimeOffset(DateTimeOffset dateTimeOffset);
    }
}
