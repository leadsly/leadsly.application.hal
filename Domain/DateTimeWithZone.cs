using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public struct DateTimeWithZone
    {
        private readonly DateTime dateTimeUnspec;
        private readonly TimeZoneInfo timeZone;

        public DateTimeWithZone(DateTime dateTime, TimeZoneInfo timeZone)
        {
            dateTimeUnspec = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
            this.timeZone = timeZone;
        }

        public DateTime DateTimeUnspecified
        {
            get
            {
                return dateTimeUnspec;
            }
        }

        public TimeZoneInfo TimeZone
        {
            get
            {
                return timeZone;
            }
        }

        public DateTime LocalTime
        {
            get
            {
                return TimeZoneInfo.ConvertTimeFromUtc(DateTimeUnspecified, timeZone);
            }
        }
    }
}
