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
        //DateTime GetNowLocalized(string zoneId);
        DateTimeOffset GetNowLocalized(string zoneId);

        DateTimeOffset GetDateTimeOffsetLocal(string zoneId, long timestamp);

        long TimestampNow();

        long TimestampFromDateTimeOffset(DateTimeOffset dateTimeOffset);
    }
}
