using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Services.Interfaces;

public interface IStaffManagementService
{
    // Personel performans takibi
    Task<StaffPerformance> GetStaffPerformanceAsync(int stylistId, DateTime startDate, DateTime endDate);
    Task<List<StaffPerformance>> GetAllStaffPerformanceAsync(DateTime startDate, DateTime endDate);
    Task UpdateStaffPerformanceAsync(int stylistId, DateTime startDate, DateTime endDate);
    
    // Personel maaş yönetimi
    Task<StaffSalary> GetStaffSalaryAsync(int stylistId, DateTime payPeriodStart, DateTime payPeriodEnd);
    Task<List<StaffSalary>> GetAllStaffSalariesAsync(DateTime payPeriodStart, DateTime payPeriodEnd);
    Task<StaffSalary> CalculateStaffSalaryAsync(int stylistId, DateTime payPeriodStart, DateTime payPeriodEnd);
    Task<bool> PayStaffSalaryAsync(int salaryId);
    
    // Personel eğitim takibi
    Task<List<StaffTraining>> GetStaffTrainingsAsync(int stylistId);
    Task<StaffTraining> CreateStaffTrainingAsync(StaffTraining training);
    Task<StaffTraining> UpdateStaffTrainingAsync(StaffTraining training);
    Task<bool> DeleteStaffTrainingAsync(int trainingId);
    
    // Personel devam takibi
    Task<StaffAttendance> GetStaffAttendanceAsync(int stylistId, DateTime date);
    Task<List<StaffAttendance>> GetStaffAttendanceRangeAsync(int stylistId, DateTime startDate, DateTime endDate);
    Task<StaffAttendance> CheckInAsync(int stylistId, DateTime checkInTime);
    Task<StaffAttendance> CheckOutAsync(int stylistId, DateTime checkOutTime);
    
    // Personel değerlendirme sistemi
    Task<List<StaffEvaluation>> GetStaffEvaluationsAsync(int stylistId);
    Task<StaffEvaluation> CreateStaffEvaluationAsync(StaffEvaluation evaluation);
    Task<StaffEvaluation> UpdateStaffEvaluationAsync(StaffEvaluation evaluation);
    
    // Personel hedefleri
    Task<List<StaffGoal>> GetStaffGoalsAsync(int stylistId);
    Task<StaffGoal> CreateStaffGoalAsync(StaffGoal goal);
    Task<StaffGoal> UpdateStaffGoalAsync(StaffGoal goal);
    Task<bool> DeleteStaffGoalAsync(int goalId);
    Task UpdateGoalProgressAsync(int goalId);
}
