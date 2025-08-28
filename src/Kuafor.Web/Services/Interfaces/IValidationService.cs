using Kuafor.Web.Models.Entities;
using Kuafor.Web.Models.Appointments;
using Kuafor.Web.Models.Validation;

namespace Kuafor.Web.Services.Interfaces;

public interface IValidationService
{
    bool IsValidAppointmentTime(DateTime startTime, DateTime endTime, int stylistId);
    bool IsValidCoupon(string couponCode, decimal basketTotal);
    bool IsWithinWorkingHours(DateTime dateTime, int branchId);
    bool IsValidPrice(decimal price, int serviceId);
    Task<ValidationResult> ValidateAppointmentAsync(CreateAppointmentViewModel model);
}
