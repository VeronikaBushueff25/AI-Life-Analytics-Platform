using AILifeAnalytics.Domain.Entities;
using AILifeAnalytics.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AILifeAnalytics.Infrastructure.Persistence;

/// <summary>
/// Activity
/// </summary>

public class EfActivityRepository : IActivityRepository
{
    private readonly AppDbContext _db;
    public EfActivityRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<Activity>> GetAllAsync() => await _db.Activities.OrderByDescending(a => a.Date).ToListAsync();

    /// <summary>
    /// Перегрузка с userId — для мультипользовательского режима
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<IEnumerable<Activity>> GetByUserAsync(Guid userId) =>
        await _db.Activities.Where(a => a.UserId == userId).OrderByDescending(a => a.Date).ToListAsync();

    public async Task<Activity?> GetByIdAsync(Guid id) => await _db.Activities.FindAsync(id);

    public async Task<Activity?> GetByDateAsync(DateTime date) => await _db.Activities.FirstOrDefaultAsync(a => a.Date.Date == date.Date);

    public async Task<Activity?> GetByUserAndDateAsync(Guid userId, DateTime date) =>
        await _db.Activities.FirstOrDefaultAsync(a => a.UserId == userId && a.Date.Date == date.Date);

    public async Task<IEnumerable<Activity>> GetByDateRangeAsync(DateTime from, DateTime to) =>
        await _db.Activities.Where(a => a.Date.Date >= from.Date && a.Date.Date <= to.Date)
            .OrderByDescending(a => a.Date).ToListAsync();

    public async Task<Activity> CreateAsync(Activity activity)
    {
        _db.Activities.Add(activity);
        await _db.SaveChangesAsync();
        return activity;
    }

    public async Task<Activity> UpdateAsync(Activity activity)
    {
        _db.Activities.Update(activity);
        await _db.SaveChangesAsync();
        return activity;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _db.Activities.FindAsync(id);
        if (entity is null) return false;
        _db.Activities.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }
}

/// <summary>
/// Insight
/// </summary>

public class EfInsightRepository : IInsightRepository
{
    private readonly AppDbContext _db;
    public EfInsightRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<Insight>> GetAllAsync() => await _db.Insights.OrderByDescending(i => i.CreatedAt).ToListAsync();

    public async Task<IEnumerable<Insight>> GetByUserAsync(Guid userId, int count = 10) =>
        await _db.Insights.Where(i => i.UserId == userId)
            .OrderByDescending(i => i.CreatedAt).Take(count).ToListAsync();

    public async Task<Insight> CreateAsync(Insight insight)
    {
        _db.Insights.Add(insight);
        await _db.SaveChangesAsync();
        return insight;
    }

    public async Task<IEnumerable<Insight>> GetRecentAsync(int count = 10) =>
        await _db.Insights.OrderByDescending(i => i.CreatedAt).Take(count).ToListAsync();
}

/// <summary>
/// User
/// </summary>

public class EfUserRepository : IUserRepository
{
    private readonly AppDbContext _db;
    public EfUserRepository(AppDbContext db) => _db = db;

    public async Task<User?> GetByIdAsync(Guid id) => await _db.Users.Include(u => u.Settings).FirstOrDefaultAsync(u => u.Id == id);

    public async Task<User?> GetByEmailAsync(string email) =>
        await _db.Users.Include(u => u.Settings).FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

    public async Task<User> CreateAsync(User user)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        _db.Users.Update(user);
        await _db.SaveChangesAsync();
        return user;
    }

    public async Task<bool> ExistsAsync(string email) => await _db.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());
}

/// <summary>
/// UserSettings
/// </summary>

public class EfUserSettingsRepository : IUserSettingsRepository
{
    private readonly AppDbContext _db;
    public EfUserSettingsRepository(AppDbContext db) => _db = db;

    public async Task<UserSettings?> GetByUserIdAsync(Guid userId) =>
        await _db.UserSettings.FirstOrDefaultAsync(s => s.UserId == userId);

    public async Task<UserSettings> CreateAsync(UserSettings settings)
    {
        _db.UserSettings.Add(settings);
        await _db.SaveChangesAsync();
        return settings;
    }

    public async Task<UserSettings> UpdateAsync(UserSettings settings)
    {
        settings.UpdatedAt = DateTime.UtcNow;
        _db.UserSettings.Update(settings);
        await _db.SaveChangesAsync();
        return settings;
    }
}