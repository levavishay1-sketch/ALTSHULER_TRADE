using System;

namespace Alt.Framework.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime ConvertIsraelTimeToUTC(this DateTime israelDateTime)
        {
            if (israelDateTime.Kind != DateTimeKind.Utc)
            {
                israelDateTime = TimeZoneInfo.ConvertTime(israelDateTime, TimeZoneInfo.FindSystemTimeZoneById("Israel Standard Time"), TimeZoneInfo.Utc);
            }
            return israelDateTime;
        }

        public static DateTime ConvertUtcToIsraelTime(this DateTime utcTime)
        {
            if (utcTime.Kind != DateTimeKind.Utc)
            {
                utcTime = utcTime.ToUniversalTime();
            }
            utcTime = TimeZoneInfo.ConvertTime((DateTime)utcTime, TimeZoneInfo.Utc, TimeZoneInfo.FindSystemTimeZoneById("Israel Standard Time"));
            utcTime = DateTime.SpecifyKind((DateTime)utcTime, DateTimeKind.Local);

            return utcTime;
        }

    }
}
