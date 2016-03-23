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

        public static DateTime ToRoundHourBottom(this DateTime source)
        {
            return new DateTime(source.Year, source.Month, source.Day, source.Hour, 0, 0, DateTimeKind.Utc);//.AddHours(1);
        }
        public static DateTime ToRoundHourUpper(this DateTime source)
        {
            return new DateTime(source.Year, source.Month, source.Day, source.Hour, 0, 0, DateTimeKind.Utc).AddHours(1);
        }
    }
}
