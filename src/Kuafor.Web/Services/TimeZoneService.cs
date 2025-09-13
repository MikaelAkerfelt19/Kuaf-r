using System;

namespace Kuafor.Web.Services
{
    public interface ITimeZoneService
    {
        DateTime ConvertToLocalTime(DateTime utcDateTime);
        DateTime ConvertToUtc(DateTime localDateTime);
        string GetLocalTimeString(DateTime utcDateTime, string format = "HH:mm");
    }

    public class TimeZoneService : ITimeZoneService
    {
        private readonly TimeZoneInfo _turkeyTimeZone;

        public TimeZoneService()
        {
            _turkeyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
        }

        public DateTime ConvertToLocalTime(DateTime utcDateTime)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, _turkeyTimeZone);
        }

        public DateTime ConvertToUtc(DateTime localDateTime)
        {
            // DateTime'ın Kind property'sini kontrol et
            if (localDateTime.Kind == DateTimeKind.Unspecified)
            {
                // Unspecified ise Local olarak işaretle
                localDateTime = DateTime.SpecifyKind(localDateTime, DateTimeKind.Local);
            }
            
            // Eğer zaten UTC ise, direkt döndür
            if (localDateTime.Kind == DateTimeKind.Utc)
            {
                return localDateTime;
            }
            
            return TimeZoneInfo.ConvertTimeToUtc(localDateTime, _turkeyTimeZone);
        }

        public string GetLocalTimeString(DateTime utcDateTime, string format = "HH:mm")
        {
            return ConvertToLocalTime(utcDateTime).ToString(format);
        }
    }
}