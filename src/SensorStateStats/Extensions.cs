using System;

namespace SensorStateStats
{
    internal static class Extensions
    {
        public static long MsTillTheEndOfHour(this DateTime self)
        {
            var endOfHour = new DateTime(self.Year, self.Month, self.Day, self.Hour, 0, 0, DateTimeKind.Utc).AddHours(1);
            return (long)(endOfHour - self).TotalMilliseconds;
        }
    }
}
