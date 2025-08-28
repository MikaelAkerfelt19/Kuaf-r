using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Entities;
using Kuafor.Web.Models.Appointments;
using Kuafor.Web.Models.Validation;

namespace Kuafor.Web.Services;

public class ValidationService : IValidationService
{
    public bool IsValidAppointmentTime(DateTime startTime, DateTime endTime, int stylistId)
    {
        var now = DateTime.UtcNow;
        
        if (startTime <= now)
            return false;
        
        if (endTime <= startTime)
            return false;
        
        var maxFutureDate = now.AddYears(1);
        if (startTime > maxFutureDate)
            return false;
        
        var appointmentDuration = endTime - startTime;
        var minDuration = TimeSpan.FromMinutes(1);
        var maxDuration = TimeSpan.FromHours(8);
        
        if (appointmentDuration < minDuration || appointmentDuration > maxDuration)
            return false;
        
        return true;
    }
    
    public bool IsValidCoupon(string couponCode, decimal basketTotal)
    {
        if (string.IsNullOrWhiteSpace(couponCode))
            return false;
        
        if (basketTotal <= 0)
            return false;
        
        if (couponCode.Length < 3)
            return false;
        
        if (!couponCode.All(c => char.IsLetterOrDigit(c)))
            return false;
        
        return true;
    }
    
    public bool IsWithinWorkingHours(DateTime dateTime, int branchId)
    {
        if (dateTime.DayOfWeek == DayOfWeek.Sunday)
            return false;
        
        var hour = dateTime.Hour;
        if (hour < 9 || hour >= 21)
            return false;
        
        if (hour == 13)
            return false;
        
        return true;
    }
    
    public bool IsValidPrice(decimal price, int serviceId)
    {
        if (price <= 0)
            return false;
        
        var maxPrice = 10000m;
        if (price > maxPrice)
            return false;
        
        var roundedPrice = Math.Round(price, 2);
        if (price != roundedPrice)
            return false;
        
        return true;
    }

    public Task<ValidationResult> ValidateAppointmentAsync(CreateAppointmentViewModel model)
    {
        var result = new ValidationResult();
        
        // 1. Temel validation
        if (!IsValidAppointmentTime(model.StartAt, model.StartAt.AddMinutes(model.DurationMin), model.StylistId))
        {
            result.AddError("Randevu zamanı geçersiz");
        }
        
        // 2. Minimum süre kontrolü
        if (model.DurationMin < 15)
        {
            result.AddError("Minimum randevu süresi 15 dakikadır");
        }
        
        return Task.FromResult(result);
    }
}
