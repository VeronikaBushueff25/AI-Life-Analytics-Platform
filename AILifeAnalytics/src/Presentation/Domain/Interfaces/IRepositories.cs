using AILifeAnalytics.Domain.Entities;

namespace AILifeAnalytics.Domain.Interfaces;

public interface IActivityRepository
{
    Task<IEnumerable<Activity>> GetAllAsync();
    Task<Activity?> GetByIdAsync(Guid id);
    Task<Activity?> GetByDateAsync(DateTime date);
    Task<IEnumerable<Activity>> GetByDateRangeAsync(DateTime from, DateTime to);
    Task<Activity> CreateAsync(Activity activity);
    Task<Activity> UpdateAsync(Activity activity);
    Task<bool> DeleteAsync(Guid id);
}

public interface IInsightRepository
{
    Task<IEnumerable<Insight>> GetAllAsync();
    Task<Insight> CreateAsync(Insight insight);
    Task<IEnumerable<Insight>> GetRecentAsync(int count = 10);
}

public interface IMetricsService
{
    Task<Metrics> CalculateAsync(IEnumerable<Activity> activities);
    double CalculateProductivityScore(Activity activity);
    double CalculateEnergyLevel(Activity activity);
    double CalculateBurnoutRisk(IEnumerable<Activity> recentActivities);
    double CalculateLifeBalance(Activity activity);
    Task<int> GetConsistencyStreakAsync();
}

public interface IAIService
{
    Task<string> GenerateInsightAsync(IEnumerable<Activity> activities, Metrics metrics);
    Task<string> AnalyzePatternAsync(IEnumerable<Activity> activities);
}
