using AILifeAnalytics.Domain.Entities;
using System.Runtime;

namespace AILifeAnalytics.Domain.Interfaces;

public interface IInsightRepository
{
    Task<IEnumerable<Insight>> GetAllAsync();
    Task<IEnumerable<Insight>> GetByUserAsync(Guid userId, int count = 10); 
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
    Task<IEnumerable<Activity>> GetByUserAsync(Guid userId);           
    Task<Activity?> GetByIdAsync(Guid id);
    Task<Activity?> GetByDateAsync(DateTime date);
    Task<Activity?> GetByUserAndDateAsync(Guid userId, DateTime date); 
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
    Task<string> GenerateProfileAsync(string prompt);
    Task<string> GenerateCbtAnalysisAsync(string prompt);
}

/// <summary>
/// Фасад: выбирает нужного провайдера на основе настроек
/// Контроллеры работают только с этим интерфейсом
/// </summary>
public interface IAIService
{
    /// <summary>
    /// Генерация инстайтов
    /// </summary>
    /// <param name="activities"></param>
    /// <param name="metrics"></param>
    /// <returns></returns>
    Task<string> GenerateInsightAsync(IEnumerable<Activity> activities, Metrics metrics);

    /// <summary>
    /// Анализ патернов
    /// </summary>
    /// <param name="activities"></param>
    /// <returns></returns>
    Task<string> AnalyzePatternAsync(IEnumerable<Activity> activities);

    /// <summary>
    /// Генерация профиля
    /// </summary>
    /// <param name="prompt"></param>
    /// <returns></returns>
    Task<string> GenerateProfileAsync(string prompt);

    /// <summary>
    /// Генерация КПТ-практики
    /// </summary>
    /// <param name="prompt"></param>
    /// <returns></returns>
    Task<string> GenerateCbtAnalysisAsync(string prompt);
}

/// <summary>
/// Настройки AI: какой провайдер активен и ключи для каждого.
/// </summary>
public interface ISettingsRepository
{
    Task<AISettings> GetAsync();
    Task SaveAsync(AISettings settings);
}

/// <summary>
/// User
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task<bool> ExistsAsync(string email);
}

/// <summary>
/// Настройки user
/// </summary>
public interface IUserSettingsRepository
{
    Task<UserSettings?> GetByUserIdAsync(Guid userId);
    Task<UserSettings> CreateAsync(UserSettings settings);
    Task<UserSettings> UpdateAsync(UserSettings settings);
}

/// <summary>
/// Аунтефикация
/// </summary>
public interface IAuthService
{
    Task<AuthResult> RegisterAsync(string email, string password, string name);
    Task<AuthResult> LoginAsync(string email, string password);
    Task<User?> GetUserByTokenAsync(string token);
}

/// <summary>
/// AI-профиль личности
/// </summary>
public interface IPersonalityProfileRepository
{
    Task<PersonalityProfile> CreateAsync(PersonalityProfile profile);
    Task<PersonalityProfile?> GetLatestByUserAsync(Guid userId);
    Task<IEnumerable<PersonalityProfile>> GetHistoryByUserAsync(Guid userId, int count = 5);
}

/// <summary>
/// КПТ
/// </summary>
public interface ICbtRepository
{
    Task<CbtRecord> CreateAsync(CbtRecord record);
    Task<CbtRecord> UpdateAsync(CbtRecord record);
    Task<CbtRecord?> GetByIdAsync(Guid id);
    Task<IEnumerable<CbtRecord>> GetByUserAsync(Guid userId, int count = 20);
    Task<IEnumerable<CbtRecord>> GetCompletedByUserAsync(Guid userId);
    Task<bool> DeleteAsync(Guid id);

    // Аналитика
    Task<Dictionary<string, int>> GetDistortionStatsAsync(Guid userId);
}