using AILifeAnalytics.Domain.Entities;
using AILifeAnalytics.Domain.Enums;
using AILifeAnalytics.Domain.Interfaces;

namespace AILifeAnalytics.Application.Services;

/// <summary>
/// Core business logic for calculating behavioral and performance metrics.
/// All formulas are documented for transparency and auditability.
/// </summary>
public class MetricsService : IMetricsService
{
    private readonly IActivityRepository _activityRepository;

    public MetricsService(IActivityRepository activityRepository)
    {
        _activityRepository = activityRepository;
    }

    /// <summary>
    /// Productivity Score (0-100):
    /// Weighted combination of focus, mood, and sleep quality.
    /// Sleep quality penalizes both under-sleep (<7h) and over-work (>10h).
    /// </summary>
    public double CalculateProductivityScore(Activity activity)
    {
        double sleepQuality = Math.Min(activity.SleepHours / 8.0, 1.0) * 100;
        double focusScore = activity.FocusLevel * 10.0;
        double moodScore = activity.Mood * 10.0;

        // Overwork penalty: reduce score if work > 10h
        double overworkPenalty = activity.WorkHours > 10 ? (activity.WorkHours - 10) * 5 : 0;

        double raw = (sleepQuality * 0.3) + (focusScore * 0.45) + (moodScore * 0.25) - overworkPenalty;
        return Math.Clamp(Math.Round(raw, 1), 0, 100);
    }

    /// <summary>
    /// Energy Level (0-100):
    /// Based on sleep and mood. Insufficient sleep drastically reduces energy.
    /// </summary>
    public double CalculateEnergyLevel(Activity activity)
    {
        double sleepFactor = activity.SleepHours < 6
            ? activity.SleepHours / 6.0 * 0.5  // severe penalty for under 6h
            : Math.Min(activity.SleepHours / 8.0, 1.0);

        double energy = (sleepFactor * 60) + (activity.Mood / 10.0 * 40);
        return Math.Clamp(Math.Round(energy, 1), 0, 100);
    }

    /// <summary>
    /// Burnout Risk (0-100):
    /// Analyzes last 7 days. High risk when: consistent overwork, low mood, poor sleep.
    /// Based on research by Maslach & Leiter on burnout indicators.
    /// </summary>
    public double CalculateBurnoutRisk(IEnumerable<Activity> recentActivities)
    {
        var activities = recentActivities.TakeLast(7).ToList();
        if (!activities.Any()) return 0;

        double avgWorkHours = activities.Average(a => a.WorkHours);
        double avgSleep = activities.Average(a => a.SleepHours);
        double avgMood = activities.Average(a => a.Mood);
        double avgFocus = activities.Average(a => a.FocusLevel);

        double workStress = Math.Clamp((avgWorkHours - 7) / 5.0 * 40, 0, 40);  // 0-40 pts
        double sleepDebt = Math.Clamp((8 - avgSleep) / 4.0 * 30, 0, 30);       // 0-30 pts
        double moodDrain = Math.Clamp((5 - avgMood) / 4.0 * 20, 0, 20);         // 0-20 pts
        double focusDrain = Math.Clamp((5 - avgFocus) / 4.0 * 10, 0, 10);       // 0-10 pts

        return Math.Clamp(Math.Round(workStress + sleepDebt + moodDrain + focusDrain, 1), 0, 100);
    }

    /// <summary>
    /// Life Balance Index (0-100):
    /// Measures balance between work, sleep, and leisure time.
    /// Ideal: ~8h sleep, 6-8h work, 8h personal time.
    /// </summary>
    public double CalculateLifeBalance(Activity activity)
    {
        double totalAccountedHours = activity.SleepHours + activity.WorkHours;
        double leisureHours = Math.Max(0, 24 - totalAccountedHours);

        double sleepBalance = 1 - Math.Abs(activity.SleepHours - 8) / 8.0;
        double workBalance = 1 - Math.Abs(activity.WorkHours - 7) / 9.0;
        double leisureBalance = Math.Min(leisureHours / 6.0, 1.0);

        double balance = (sleepBalance * 35 + workBalance * 35 + leisureBalance * 30);
        return Math.Clamp(Math.Round(balance, 1), 0, 100);
    }

    /// <summary>
    /// Consistency Streak: number of consecutive days with an entry.
    /// </summary>
    public async Task<int> GetConsistencyStreakAsync()
    {
        var all = (await _activityRepository.GetAllAsync())
            .OrderByDescending(a => a.Date)
            .ToList();

        if (!all.Any()) return 0;

        int streak = 0;
        var today = DateTime.Today;
        var checkDate = all.First().Date.Date;

        // Allow up to 1 day gap (today may not be logged yet)
        if ((today - checkDate).Days > 1) return 0;

        foreach (var activity in all)
        {
            if ((checkDate - activity.Date.Date).Days <= 1)
            {
                streak++;
                checkDate = activity.Date.Date.AddDays(-1);
            }
            else break;
        }

        return streak;
    }

    public async Task<Metrics> CalculateAsync(IEnumerable<Activity> activities)
    {
        var activityList = activities.ToList();
        if (!activityList.Any())
            return new Metrics();

        var latest = activityList.OrderByDescending(a => a.Date).First();
        var recent = activityList.OrderByDescending(a => a.Date).Take(7).ToList();

        var burnoutRisk = CalculateBurnoutRisk(recent);
        var burnoutStatus = burnoutRisk switch
        {
            < 25 => BurnoutStatus.Low,
            < 50 => BurnoutStatus.Medium,
            < 75 => BurnoutStatus.High,
            _ => BurnoutStatus.Critical
        };

        return new Metrics
        {
            ProductivityScore = CalculateProductivityScore(latest),
            EnergyLevel = CalculateEnergyLevel(latest),
            BurnoutRisk = burnoutRisk,
            LifeBalanceIndex = CalculateLifeBalance(latest),
            ConsistencyStreak = await GetConsistencyStreakAsync(),
            BurnoutStatus = burnoutStatus
        };
    }
}
