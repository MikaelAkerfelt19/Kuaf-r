using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Services.Interfaces;

public interface IValidationService
{
    bool IsValidAppointmentTime(DateTime startTime, DateTime endTime, int stylistId);
    bool IsValidCoupon(string couponCode, decimal basketTotal);
    bool IsWithinWorkingHours(DateTime dateTime, int branchId);
    bool IsValidPrice(decimal price, int serviceId);
}
