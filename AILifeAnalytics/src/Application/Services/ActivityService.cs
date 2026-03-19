using AILifeAnalytics.Application.DTOs;
using AILifeAnalytics.Domain.Entities;
using AILifeAnalytics.Domain.Interfaces;

namespace AILifeAnalytics.Application.Services;

public class ActivityService
{
    private readonly IActivityRepository _activityRepo;
    private readonly IMetricsService _metricsService;

    public ActivityService(IActivityRepository activityRepo, IMetricsService metricsService)
    {
        _activityRepo = activityRepo;
        _metricsService = metricsService;
    }

    public async Task<DashboardResponse> GetDashboardAsync()
    {
        var all = (await _activityRepo.GetAllAsync())
            .OrderByDescending(a => a.Date)
            .ToList();

        var metrics = await _metricsService.CalculateAsync(all);
        var recent = all.Take(30).ToList();

        var productivityChart = recent
            .OrderBy(a => a.Date)
            .Select(a => new ChartDataPoint
            {
                Label = a.Date.ToString("MM/dd"),
                Value = _metricsService.CalculateProductivityScore(a)
            });

        var moodChart = recent
            .OrderBy(a => a.Date)
            .Select(a => new ChartDataPoint
            {
                Label = a.Date.ToString("MM/dd"),
                Value = a.Mood * 10
            });

        var sleepChart = recent
            .OrderBy(a => a.Date)
            .Select(a => new ChartDataPoint
            {
                Label = a.Date.ToString("MM/dd"),
                Value = a.SleepHours
            });

        var avgSleep = all.Any() ? all.Average(a => a.SleepHours) : 0;
        var avgWork = all.Any() ? all.Average(a => a.WorkHours) : 0;
        var avgLeisure = Math.Max(0, 24 - avgSleep - avgWork);

        return new DashboardResponse
        {
            Metrics = new MetricsResponse
            {
                ProductivityScore = metrics.ProductivityScore,
                EnergyLevel = metrics.EnergyLevel,
                BurnoutRisk = metrics.BurnoutRisk,
                LifeBalanceIndex = metrics.LifeBalanceIndex,
                ConsistencyStreak = metrics.ConsistencyStreak,
                BurnoutStatus = metrics.BurnoutStatus
            },
            RecentActivities = recent.Take(7).Select(MapToResponse),
            ProductivityChart = productivityChart,
            MoodChart = moodChart,
            SleepChart = sleepChart,
            TimeDistribution = new TimeDistribution
            {
                AvgSleepHours = Math.Round(avgSleep, 1),
                AvgWorkHours = Math.Round(avgWork, 1),
                AvgLeisureHours = Math.Round(avgLeisure, 1)
            }
        };
    }

    public async Task<ActivityResponse> CreateAsync(CreateActivityRequest request)
    {
        // Check for duplicate date
        var existing = await _activityRepo.GetByDateAsync(request.Date.Date);
        if (existing != null)
            throw new InvalidOperationException($"Entry for {request.Date:yyyy-MM-dd} already exists.");

        var activity = new Activity
        {
            Date = request.Date.Date,
            SleepHours = request.SleepHours,
            WorkHours = request.WorkHours,
            FocusLevel = request.FocusLevel,
            Mood = request.Mood,
            Notes = request.Notes
        };

        var created = await _activityRepo.CreateAsync(activity);
        return MapToResponse(created);
    }

    public async Task<ActivityResponse> UpdateAsync(Guid id, UpdateActivityRequest request)
    {
        var existing = await _activityRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Activity {id} not found.");

        existing.Date = request.Date.Date;
        existing.SleepHours = request.SleepHours;
        existing.WorkHours = request.WorkHours;
        existing.FocusLevel = request.FocusLevel;
        existing.Mood = request.Mood;
        existing.Notes = request.Notes;
        existing.UpdatedAt = DateTime.UtcNow;

        var updated = await _activityRepo.UpdateAsync(existing);
        return MapToResponse(updated);
    }

    public async Task<bool> DeleteAsync(Guid id) => await _activityRepo.DeleteAsync(id);

    public async Task<IEnumerable<ActivityResponse>> GetAllAsync()
    {
        var all = await _activityRepo.GetAllAsync();
        return all.OrderByDescending(a => a.Date).Select(MapToResponse);
    }

    private ActivityResponse MapToResponse(Activity a) => new()
    {
        Id = a.Id,
        Date = a.Date,
        SleepHours = a.SleepHours,
        WorkHours = a.WorkHours,
        FocusLevel = a.FocusLevel,
        Mood = a.Mood,
        Notes = a.Notes,
        ProductivityScore = _metricsService.CalculateProductivityScore(a),
        EnergyLevel = _metricsService.CalculateEnergyLevel(a)
    };
}
