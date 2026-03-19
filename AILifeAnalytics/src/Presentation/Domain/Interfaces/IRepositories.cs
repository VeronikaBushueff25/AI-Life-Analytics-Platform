using AILifeAnalytics.Domain.Entities;
using System.Runtime;

namespace AILifeAnalytics.Domain.Interfaces;

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

/// <summary>
/// Контракт для любого AI-провайдера. Добавить нового — реализовать интерфейс
/// </summary>
public interface IAIProvider
{
    string ProviderName { get; } 
    Task<string> GenerateInsightAsync(IEnumerable<Activity> activities, Metrics metrics);
    Task<string> AnalyzePatternAsync(IEnumerable<Activity> activities);
}

/// <summary>
/// Фасад: выбирает нужного провайдера на основе настроек
/// Контроллеры работают только с этим интерфейсом
/// </summary>
public interface IAIService
{
    Task<string> GenerateInsightAsync(IEnumerable<Activity> activities, Metrics metrics);
    Task<string> AnalyzePatternAsync(IEnumerable<Activity> activities);
}

/// <summary>
/// Настройки AI: какой провайдер активен и ключи для каждого.
/// </summary>
public interface ISettingsRepository
{
    Task<AISettings> GetAsync();
    Task SaveAsync(AISettings settings);
}