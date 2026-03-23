using System.Text.Json;
using AILifeAnalytics.Domain.Entities;
using AILifeAnalytics.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AILifeAnalytics.Infrastructure.Storage;

/// <summary>
/// JSON file-based storage. Designed for easy migration to SQL/MongoDB.
/// Each repository uses a separate JSON file for isolation.
/// </summary>
public abstract class JsonRepositoryBase<T>
{
    protected readonly string FilePath;
    protected readonly ILogger Logger;
    private readonly SemaphoreSlim _lock = new(1, 1);

    protected JsonRepositoryBase(string dataDirectory, string fileName, ILogger logger)
    {
        FilePath = Path.Combine(dataDirectory, fileName);
        Logger = logger;
        Directory.CreateDirectory(dataDirectory);
        if (!File.Exists(FilePath))
            File.WriteAllText(FilePath, "[]");
    }

    protected async Task<List<T>> ReadAllAsync()
    {
        await _lock.WaitAsync();
        try
        {
            var json = await File.ReadAllTextAsync(FilePath);
            return JsonSerializer.Deserialize<List<T>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
        }
        finally { _lock.Release(); }
    }

    protected async Task WriteAllAsync(List<T> items)
    {
        await _lock.WaitAsync();
        try
        {
            var json = JsonSerializer.Serialize(items, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(FilePath, json);
        }
        finally { _lock.Release(); }
    }
}

// Activity Repository

public class ActivityRepository : JsonRepositoryBase<Activity>, IActivityRepository
{
    public ActivityRepository(IConfiguration config, ILogger<ActivityRepository> logger) : base(config["DataDirectory"] ?? "data", "activities.json", logger) { }

    public async Task<IEnumerable<Activity>> GetAllAsync() => await ReadAllAsync();

    public async Task<IEnumerable<Activity>> GetByUserAsync(Guid userId)
    {
        var all = await ReadAllAsync();
        return all.Where(a => a.UserId == userId).OrderByDescending(a => a.Date);
    }

    public async Task<Activity?> GetByIdAsync(Guid id)
    {
        var all = await ReadAllAsync();
        return all.FirstOrDefault(a => a.Id == id);
    }

    public async Task<Activity?> GetByDateAsync(DateTime date)
    {
        var all = await ReadAllAsync();
        return all.FirstOrDefault(a => a.Date.Date == date.Date);
    }

    public async Task<Activity?> GetByUserAndDateAsync(Guid userId, DateTime date)
    {
        var all = await ReadAllAsync();
        return all.FirstOrDefault(a => a.UserId == userId && a.Date.Date == date.Date);
    }

    public async Task<IEnumerable<Activity>> GetByDateRangeAsync(DateTime from, DateTime to)
    {
        var all = await ReadAllAsync();
        return all.Where(a => a.Date.Date >= from.Date && a.Date.Date <= to.Date).OrderByDescending(a => a.Date);
    }

    public async Task<Activity> CreateAsync(Activity activity)
    {
        var all = await ReadAllAsync();
        all.Add(activity);
        await WriteAllAsync(all);
        return activity;
    }

    public async Task<Activity> UpdateAsync(Activity activity)
    {
        var all = await ReadAllAsync();
        var index = all.FindIndex(a => a.Id == activity.Id);
        if (index < 0) throw new KeyNotFoundException($"Activity {activity.Id} not found.");
        all[index] = activity;
        await WriteAllAsync(all);
        return activity;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var all = await ReadAllAsync();
        var removed = all.RemoveAll(a => a.Id == id);
        if (removed > 0) await WriteAllAsync(all);
        return removed > 0;
    }
}

// Insight Repository

public class InsightRepository : JsonRepositoryBase<Insight>, IInsightRepository
{
    public InsightRepository(IConfiguration config, ILogger<InsightRepository> logger) : base(config["DataDirectory"] ?? "data", "insights.json", logger) { }

    public async Task<IEnumerable<Insight>> GetAllAsync() => await ReadAllAsync();

    public async Task<IEnumerable<Insight>> GetByUserAsync(Guid userId, int count = 10)
    {
        var all = await ReadAllAsync();
        return all.Where(i => i.UserId == userId).OrderByDescending(i => i.CreatedAt).Take(count);
    }

    public async Task<Insight> CreateAsync(Insight insight)
    {
        var all = await ReadAllAsync();
        all.Add(insight);
        await WriteAllAsync(all);
        return insight;
    }

    public async Task<IEnumerable<Insight>> GetRecentAsync(int count = 10)
    {
        var all = await ReadAllAsync();
        return all.OrderByDescending(i => i.CreatedAt).Take(count);
    }
}

public class SettingsRepository : ISettingsRepository
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public SettingsRepository(IConfiguration config)
    {
        var dir = config["DataDirectory"] ?? "data";
        Directory.CreateDirectory(dir);
        _filePath = Path.Combine(dir, "settings.json");

        if (!File.Exists(_filePath))
        {
            var defaults = new AISettings();
            File.WriteAllText(_filePath, JsonSerializer.Serialize(defaults, new JsonSerializerOptions { WriteIndented = true }));
        }
    }

    public async Task<AISettings> GetAsync()
    {
        await _lock.WaitAsync();
        try
        {
            var json = await File.ReadAllTextAsync(_filePath);
            return JsonSerializer.Deserialize<AISettings>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new AISettings();
        }
        finally { _lock.Release(); }
    }

    public async Task SaveAsync(AISettings settings)
    {
        await _lock.WaitAsync();
        try
        {
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_filePath, json);
        }
        finally { _lock.Release(); }
    }
}