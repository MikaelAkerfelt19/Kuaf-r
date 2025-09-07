using Microsoft.EntityFrameworkCore;
using Kuafor.Web.Data;
using Kuafor.Web.Models.Entities;
using Kuafor.Web.Models.Enums;
using Kuafor.Web.Services.Interfaces;

namespace Kuafor.Web.Services;

public class StaffManagementService : IStaffManagementService
{
    private readonly ApplicationDbContext _context;

    public StaffManagementService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StaffPerformance> GetStaffPerformanceAsync(int stylistId, DateTime startDate, DateTime endDate)
    {
        var performance = await _context.StaffPerformances
            .FirstOrDefaultAsync(sp => sp.StylistId == stylistId && 
                                      sp.PeriodStart >= startDate && 
                                      sp.PeriodEnd <= endDate);

        if (performance == null)
        {
            // Performans verisi yoksa hesapla
            performance = await CalculateStaffPerformanceAsync(stylistId, startDate, endDate);
        }

        return performance;
    }

    public async Task<List<StaffPerformance>> GetAllStaffPerformanceAsync(DateTime startDate, DateTime endDate)
    {
        var performances = await _context.StaffPerformances
            .Include(sp => sp.Stylist)
            .Where(sp => sp.PeriodStart >= startDate && sp.PeriodEnd <= endDate)
            .ToListAsync();

        return performances;
    }

    public async Task UpdateStaffPerformanceAsync(int stylistId, DateTime startDate, DateTime endDate)
    {
        var performance = await CalculateStaffPerformanceAsync(stylistId, startDate, endDate);
        
        var existingPerformance = await _context.StaffPerformances
            .FirstOrDefaultAsync(sp => sp.StylistId == stylistId && 
                                      sp.PeriodStart >= startDate && 
                                      sp.PeriodEnd <= endDate);

        if (existingPerformance != null)
        {
            existingPerformance.TotalAppointments = performance.TotalAppointments;
            existingPerformance.CompletedAppointments = performance.CompletedAppointments;
            existingPerformance.CancelledAppointments = performance.CancelledAppointments;
            existingPerformance.NoShowAppointments = performance.NoShowAppointments;
            existingPerformance.TotalRevenue = performance.TotalRevenue;
            existingPerformance.AverageTicketValue = performance.AverageTicketValue;
            existingPerformance.CommissionEarned = performance.CommissionEarned;
            existingPerformance.AverageRating = performance.AverageRating;
            existingPerformance.TotalRatings = performance.TotalRatings;
            existingPerformance.Complaints = performance.Complaints;
            existingPerformance.Compliments = performance.Compliments;
            existingPerformance.AverageServiceTime = performance.AverageServiceTime;
            existingPerformance.UtilizationRate = performance.UtilizationRate;
            existingPerformance.OvertimeHours = performance.OvertimeHours;
            existingPerformance.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            _context.StaffPerformances.Add(performance);
        }

        await _context.SaveChangesAsync();
    }

    private async Task<StaffPerformance> CalculateStaffPerformanceAsync(int stylistId, DateTime startDate, DateTime endDate)
    {
        var appointments = await _context.Appointments
            .Where(a => a.StylistId == stylistId && 
                       a.StartAt >= startDate && 
                       a.StartAt <= endDate)
            .ToListAsync();

        var totalAppointments = appointments.Count;
        var completedAppointments = appointments.Count(a => a.Status == AppointmentStatus.Completed);
        var cancelledAppointments = appointments.Count(a => a.Status == AppointmentStatus.Cancelled);
        var noShowAppointments = appointments.Count(a => a.Status == AppointmentStatus.NoShow);

        var totalRevenue = appointments.Where(a => a.Status == AppointmentStatus.Completed).Sum(a => a.FinalPrice);
        var averageTicketValue = completedAppointments > 0 ? totalRevenue / completedAppointments : 0;

        // Ortalama hizmet süresi hesaplama
        var averageServiceTime = appointments.Where(a => a.Status == AppointmentStatus.Completed)
            .Average(a => (a.EndAt - a.StartAt).TotalMinutes);

        // Kullanım oranı hesaplama (çalışma saatlerine göre)
        var workingHours = await _context.StylistWorkingHours
            .Where(swh => swh.StylistId == stylistId)
            .ToListAsync();

        var totalWorkingMinutes = workingHours.Sum(wh => (wh.CloseTime - wh.OpenTime).TotalMinutes);
        var utilizedMinutes = appointments.Where(a => a.Status == AppointmentStatus.Completed)
            .Sum(a => (a.EndAt - a.StartAt).TotalMinutes);
        
        var utilizationRate = totalWorkingMinutes > 0 ? (utilizedMinutes / totalWorkingMinutes) * 100 : 0;

        return new StaffPerformance
        {
            StylistId = stylistId,
            TotalAppointments = totalAppointments,
            CompletedAppointments = completedAppointments,
            CancelledAppointments = cancelledAppointments,
            NoShowAppointments = noShowAppointments,
            TotalRevenue = totalRevenue,
            AverageTicketValue = averageTicketValue,
            CommissionEarned = 0, // Bu hesaplama ayrı yapılabilir
            AverageRating = 0, // Bu hesaplama ayrı yapılabilir
            TotalRatings = 0,
            Complaints = 0,
            Compliments = 0,
            AverageServiceTime = averageServiceTime,
            UtilizationRate = utilizationRate,
            OvertimeHours = 0,
            PeriodStart = startDate,
            PeriodEnd = endDate,
            PeriodType = "Custom",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public async Task<StaffSalary?> GetStaffSalaryAsync(int stylistId, DateTime payPeriodStart, DateTime payPeriodEnd)
    {
        return await _context.StaffSalaries
            .FirstOrDefaultAsync(ss => ss.StylistId == stylistId && 
                                      ss.PayPeriodStart >= payPeriodStart && 
                                      ss.PayPeriodEnd <= payPeriodEnd);
    }

    public async Task<List<StaffSalary>> GetAllStaffSalariesAsync(DateTime payPeriodStart, DateTime payPeriodEnd)
    {
        return await _context.StaffSalaries
            .Include(ss => ss.Stylist)
            .Where(ss => ss.PayPeriodStart >= payPeriodStart && ss.PayPeriodEnd <= payPeriodEnd)
            .ToListAsync();
    }

    public async Task<StaffSalary?> CalculateStaffSalaryAsync(int stylistId, DateTime payPeriodStart, DateTime payPeriodEnd)
    {
        var stylist = await _context.Stylists.FirstOrDefaultAsync(s => s.Id == stylistId);
        if (stylist == null) return null;

        var performance = await GetStaffPerformanceAsync(stylistId, payPeriodStart, payPeriodEnd);
        
        // Temel maaş hesaplama (örnek)
        var baseSalary = 5000m; // Bu değer veritabanından alınabilir
        var hourlyRate = 50m;
        var commissionRate = 5m; // %5

        // Komisyon hesaplama
        var commissionEarned = (performance.TotalRevenue * commissionRate) / 100;

        // Prim hesaplama
        var performanceBonus = performance.CompletedAppointments * 10m; // Randevu başına 10 TL
        var salesBonus = performance.TotalRevenue > 10000 ? 500m : 0; // 10.000 TL üzeri satışta 500 TL bonus
        var qualityBonus = performance.AverageRating >= 4.5 ? 300m : 0; // 4.5+ puan için 300 TL bonus

        // Kesintiler
        var taxDeduction = (baseSalary + commissionEarned + performanceBonus + salesBonus + qualityBonus) * 0.15m; // %15 vergi
        var insuranceDeduction = (baseSalary + commissionEarned + performanceBonus + salesBonus + qualityBonus) * 0.14m; // %14 sigorta

        var grossSalary = baseSalary + commissionEarned + performanceBonus + salesBonus + qualityBonus;
        var totalDeductions = taxDeduction + insuranceDeduction;
        var netSalary = grossSalary - totalDeductions;

        return new StaffSalary
        {
            StylistId = stylistId,
            BaseSalary = baseSalary,
            HourlyRate = hourlyRate,
            CommissionRate = commissionRate,
            PerformanceBonus = performanceBonus,
            SalesBonus = salesBonus,
            QualityBonus = qualityBonus,
            AttendanceBonus = 0,
            TaxDeduction = taxDeduction,
            InsuranceDeduction = insuranceDeduction,
            OtherDeductions = 0,
            NetSalary = netSalary,
            PaymentMethod = "Bank Transfer",
            PayPeriodStart = payPeriodStart,
            PayPeriodEnd = payPeriodEnd,
            CreatedAt = DateTime.UtcNow
        };
    }

    public async Task<bool> PayStaffSalaryAsync(int salaryId)
    {
        var salary = await _context.StaffSalaries
            .FirstOrDefaultAsync(ss => ss.Id == salaryId);

        if (salary == null) return false;

        salary.PaidAt = DateTime.UtcNow;
        salary.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<StaffTraining>> GetStaffTrainingsAsync(int stylistId)
    {
        return await _context.StaffTrainings
            .Where(st => st.StylistId == stylistId)
            .OrderByDescending(st => st.TrainingDate)
            .ToListAsync();
    }

    public async Task<StaffTraining> CreateStaffTrainingAsync(StaffTraining training)
    {
        training.CreatedAt = DateTime.UtcNow;
        _context.StaffTrainings.Add(training);
        await _context.SaveChangesAsync();
        return training;
    }

    public async Task<StaffTraining> UpdateStaffTrainingAsync(StaffTraining training)
    {
        training.UpdatedAt = DateTime.UtcNow;
        _context.StaffTrainings.Update(training);
        await _context.SaveChangesAsync();
        return training;
    }

    public async Task<bool> DeleteStaffTrainingAsync(int trainingId)
    {
        var training = await _context.StaffTrainings
            .FirstOrDefaultAsync(st => st.Id == trainingId);

        if (training == null) return false;

        _context.StaffTrainings.Remove(training);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<StaffAttendance?> GetStaffAttendanceAsync(int stylistId, DateTime date)
    {
        return await _context.StaffAttendances
            .FirstOrDefaultAsync(sa => sa.StylistId == stylistId && sa.Date.Date == date.Date);
    }

    public async Task<List<StaffAttendance>> GetStaffAttendanceRangeAsync(int stylistId, DateTime startDate, DateTime endDate)
    {
        return await _context.StaffAttendances
            .Where(sa => sa.StylistId == stylistId && 
                        sa.Date >= startDate && 
                        sa.Date <= endDate)
            .OrderBy(sa => sa.Date)
            .ToListAsync();
    }

    public async Task<StaffAttendance> CheckInAsync(int stylistId, DateTime checkInTime)
    {
        var attendance = await GetStaffAttendanceAsync(stylistId, checkInTime.Date);
        
        if (attendance == null)
        {
            attendance = new StaffAttendance
            {
                StylistId = stylistId,
                Date = checkInTime.Date,
                CheckInTime = checkInTime,
                Status = "Present",
                CreatedAt = DateTime.UtcNow
            };
            _context.StaffAttendances.Add(attendance);
        }
        else
        {
            attendance.CheckInTime = checkInTime;
            attendance.Status = "Present";
            attendance.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return attendance;
    }

    public async Task<StaffAttendance> CheckOutAsync(int stylistId, DateTime checkOutTime)
    {
        var attendance = await GetStaffAttendanceAsync(stylistId, checkOutTime.Date);
        
        if (attendance != null)
        {
            attendance.CheckOutTime = checkOutTime;
            attendance.UpdatedAt = DateTime.UtcNow;
            
            if (attendance.CheckInTime.HasValue)
            {
                var totalHours = (checkOutTime - attendance.CheckInTime.Value).TotalHours;
                attendance.TotalHours = (int)totalHours;
            }
            
            await _context.SaveChangesAsync();
        }

        return attendance!;
    }

    public async Task<List<StaffEvaluation>> GetStaffEvaluationsAsync(int stylistId)
    {
        return await _context.StaffEvaluations
            .Include(se => se.Evaluator)
            .Where(se => se.StylistId == stylistId)
            .OrderByDescending(se => se.EvaluationDate)
            .ToListAsync();
    }

    public async Task<StaffEvaluation> CreateStaffEvaluationAsync(StaffEvaluation evaluation)
    {
        // Genel puan hesaplama
        evaluation.OverallScore = (evaluation.TechnicalSkills + evaluation.CustomerService + 
                                 evaluation.Teamwork + evaluation.Punctuality + 
                                 evaluation.Professionalism + evaluation.Communication + 
                                 evaluation.ProblemSolving + evaluation.Adaptability) / 8;

        evaluation.CreatedAt = DateTime.UtcNow;
        _context.StaffEvaluations.Add(evaluation);
        await _context.SaveChangesAsync();
        return evaluation;
    }

    public async Task<StaffEvaluation> UpdateStaffEvaluationAsync(StaffEvaluation evaluation)
    {
        // Genel puan hesaplama
        evaluation.OverallScore = (evaluation.TechnicalSkills + evaluation.CustomerService + 
                                 evaluation.Teamwork + evaluation.Punctuality + 
                                 evaluation.Professionalism + evaluation.Communication + 
                                 evaluation.ProblemSolving + evaluation.Adaptability) / 8;

        evaluation.UpdatedAt = DateTime.UtcNow;
        _context.StaffEvaluations.Update(evaluation);
        await _context.SaveChangesAsync();
        return evaluation;
    }

    public async Task<List<StaffGoal>> GetStaffGoalsAsync(int stylistId)
    {
        return await _context.StaffGoals
            .Where(sg => sg.StylistId == stylistId)
            .OrderByDescending(sg => sg.CreatedAt)
            .ToListAsync();
    }

    public async Task<StaffGoal> CreateStaffGoalAsync(StaffGoal goal)
    {
        goal.CreatedAt = DateTime.UtcNow;
        _context.StaffGoals.Add(goal);
        await _context.SaveChangesAsync();
        return goal;
    }

    public async Task<StaffGoal> UpdateStaffGoalAsync(StaffGoal goal)
    {
        goal.UpdatedAt = DateTime.UtcNow;
        _context.StaffGoals.Update(goal);
        await _context.SaveChangesAsync();
        return goal;
    }

    public async Task<bool> DeleteStaffGoalAsync(int goalId)
    {
        var goal = await _context.StaffGoals
            .FirstOrDefaultAsync(sg => sg.Id == goalId);

        if (goal == null) return false;

        _context.StaffGoals.Remove(goal);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task UpdateGoalProgressAsync(int goalId)
    {
        var goal = await _context.StaffGoals
            .FirstOrDefaultAsync(sg => sg.Id == goalId);

        if (goal == null) return;

        // Hedef tipine göre mevcut değeri güncelle
        switch (goal.GoalType)
        {
            case "Revenue":
                var performance = await GetStaffPerformanceAsync(goal.StylistId, goal.StartDate, goal.EndDate);
                goal.CurrentValue = performance.TotalRevenue;
                break;
            case "Appointments":
                var appointments = await _context.Appointments
                    .CountAsync(a => a.StylistId == goal.StylistId && 
                                   a.StartAt >= goal.StartDate && 
                                   a.StartAt <= goal.EndDate);
                goal.CurrentValue = appointments;
                break;
            // Diğer hedef tipleri için benzer hesaplamalar yapılabilir
        }

        // İlerleme yüzdesi hesaplama
        if (goal.TargetValue > 0)
        {
            goal.ProgressPercentage = (double)(goal.CurrentValue / goal.TargetValue) * 100;
        }

        // Hedef durumu güncelleme
        if (goal.ProgressPercentage >= 100)
        {
            goal.Status = "Completed";
        }
        else if (DateTime.UtcNow > goal.EndDate && goal.ProgressPercentage < 100)
        {
            goal.Status = "Failed";
        }

        goal.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}
