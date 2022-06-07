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
        DateTimeOffset GetNowLocalized(string zoneId);        
        DateTimeOffset ParseDateTimeOffsetLocalized(string zoneId, string timeOfDay);
        long TimestampNow();
        long TimestampFromDateTimeOffset(DateTimeOffset dateTimeOffset);
    }
}
