namespace Kuafor.Web.Services;

public static class TurkishDateHelper
{
    // Türkçe gün isimleri
    private static readonly string[] TurkishDays = 
    {
        "Pazar", "Pazartesi", "Salı", "Çarşamba", 
        "Perşembe", "Cuma", "Cumartesi"
    };
    
    // Türkçe ay isimleri
    private static readonly string[] TurkishMonths = 
    {
        "Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran",
        "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık"
    };
    
    // Türkiye resmi tatil günleri (2024)
    private static readonly DateTime[] TurkishHolidays = 
    {
        new DateTime(2024, 1, 1),   // Yılbaşı
        new DateTime(2024, 4, 23),  // Çocuk Bayramı
        new DateTime(2024, 5, 1),   // İşçi Bayramı
        new DateTime(2024, 5, 19),  // Gençlik Bayramı
        new DateTime(2024, 7, 15),  // Demokrasi Bayramı
        new DateTime(2024, 8, 30),  // Zafer Bayramı
        new DateTime(2024, 10, 29), // Cumhuriyet Bayramı
        new DateTime(2024, 11, 10), // Atatürk'ü Anma
        new DateTime(2024, 12, 31)  // Yılbaşı Arifesi
    };
    
    // Tarihi Türkçe formatında döndürür
    public static string GetTurkishDate(DateTime date)
    {
        var dayName = TurkishDays[(int)date.DayOfWeek];
        var day = date.Day;
        var monthName = TurkishMonths[date.Month - 1];
        var year = date.Year;
        
        return $"{dayName}, {day} {monthName} {year}";
    }
    
    // Günün Türkçe adını döndürür
    public static string GetTurkishDayName(DateTime date)
    {
        return TurkishDays[(int)date.DayOfWeek];
    }
    
    // Ayın Türkçe adını döndürür
    public static string GetTurkishMonthName(DateTime date)
    {
        return TurkishMonths[date.Month - 1];
    }
    
    // Hafta sonu mu kontrol eder
    public static bool IsWeekend(DateTime date)
    {
        return date.DayOfWeek == DayOfWeek.Saturday || 
               date.DayOfWeek == DayOfWeek.Sunday;
    }
    
    // Resmi tatil mi kontrol eder
    public static bool IsHoliday(DateTime date)
    {
        var dateOnly = date.Date;
        return TurkishHolidays.Contains(dateOnly);
    }
    
    // Çalışma günü mü kontrol eder (tatil değil ve hafta sonu değil)
    public static bool IsWorkingDay(DateTime date)
    {
        return !IsWeekend(date) && !IsHoliday(date);
    }
    
    // Kuaför çalışma saatleri içinde mi kontrol eder
    public static bool IsWithinHairSalonHours(DateTime dateTime)
    {
        var hour = dateTime.Hour;
        
        if (IsWeekend(dateTime))
        {
            // Hafta sonu: 10:00 - 20:00
            return hour >= 10 && hour < 20;
        }
        else
        {
            // Hafta içi: 09:00 - 21:00
            return hour >= 9 && hour < 21;
        }
    }
    
    // Tarih aralığındaki çalışma günü sayısını hesaplar
    public static int GetWorkingDaysCount(DateTime startDate, DateTime endDate)
    {
        var count = 0;
        var currentDate = startDate.Date;
        
        while (currentDate <= endDate.Date)
        {
            if (IsWorkingDay(currentDate))
                count++;
            currentDate = currentDate.AddDays(1);
        }
        
        return count;
    }
}
