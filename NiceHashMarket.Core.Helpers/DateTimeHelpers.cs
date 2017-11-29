using System;

namespace NiceHashMarket.Core.Helpers
{
    public static class DateTimeHelpers
    {
        public static DateTime ToDateTime(this long unixTime, DateTimeKind timeKind)
        {
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, timeKind);
            dateTime = dateTime.AddSeconds(unixTime).ToLocalTime();

            return dateTime;
        }
    }
}
