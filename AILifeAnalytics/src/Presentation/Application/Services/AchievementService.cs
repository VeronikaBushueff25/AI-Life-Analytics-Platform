using AILifeAnalytics.Domain.Entities;
using AILifeAnalytics.Domain.Enums;
using AILifeAnalytics.Domain.Interfaces;

namespace AILifeAnalytics.Application.Services;

/// <summary>
/// Сервис проверки и выдачи достижений
/// </summary>
public class AchievementService
{
    private readonly IAchievementRepository _repo;
    private readonly IActivityRepository _activityRepo;
    private readonly ICbtRepository _cbtRepo;

    /// <summary>
    /// Мета-данные достижений
    /// </summary>
    private static readonly Dictionary<AchievementType, AchievementMeta> Meta = new()
    {
        [AchievementType.FirstEntry] = new("🌱", "Первые шаги", "Вы добавили первую запись!"),
        [AchievementType.Streak7Days] = new("🔥", "Неделя силы", "7 дней подряд без пропусков"),
        [AchievementType.Streak30Days] = new("🚀", "Месяц роста", "30 дней подряд — это серьёзно"),
        [AchievementType.Streak100Days] = new("👑", "Легенда", "100 дней подряд. Вы — легенда"),
        [AchievementType.ProductivityRecord] = new("⚡", "Личный рекорд", "Новый рекорд продуктивности!"),
        [AchievementType.ProductivitySuperstar] = new("🌟", "Суперзвезда", "Продуктивность >90 три дня подряд"),
        [AchievementType.ProductivityConsistent] = new("💎", "Стабильность", "Продуктивность >70 семь дней подряд"),
        [AchievementType.SleepMaster] = new("😴", "Мастер сна", "8ч+ сна пять дней подряд"),
        [AchievementType.NightOwl] = new("🦉", "Ночная сова", "Спали <6ч пять раз. Берегите себя!"),
        [AchievementType.SleepRecovered] = new("☀️", "Восстановление", "После недосыпания вернулись к норме"),
        [AchievementType.WorkLifeBalance] = new("☯️", "Баланс", "Баланс >80 три дня подряд"),
        [AchievementType.NoOverwork] = new("🌿", "Без переработок", "Ни одной переработки за неделю"),
        [AchievementType.DeepFocus] = new("🎯", "Глубокий фокус", "Фокус 9-10 пять дней подряд"),
        [AchievementType.FirstCbt] = new("🧠", "Первая сессия КПТ", "Вы начали работу над собой"),
        [AchievementType.CbtPractitioner] = new("💬", "КПТ-практик", "10 завершённых КПТ-сессий"),
        [AchievementType.CbtMaster] = new("🏆", "Мастер КПТ", "30 завершённых КПТ-сессий"),
        [AchievementType.MindShift] = new("✨", "Сдвиг мышления", "Снизили интенсивность эмоции >50%"),
        [AchievementType.FirstProfile] = new("🔬", "Самопознание", "Построен первый AI-профиль"),
        [AchievementType.ProfileEvolved] = new("🦋", "Эволюция", "Ваш архетип изменился!"),
        [AchievementType.Veteran] = new("🎖️", "Ветеран", "50 записей в дневнике"),
        [AchievementType.Legend] = new("🌌", "Легенда", "200 записей. Невероятно!"),
        [AchievementType.BurnoutPrevented] = new("🛡️", "Защитник себя", "Снизили риск выгорания с High до Low"),
    };

    public AchievementService(IAchievementRepository repo, IActivityRepository activityRepo, ICbtRepository cbtRepo)
    {
        _repo = repo;
        _activityRepo = activityRepo;
        _cbtRepo = cbtRepo;
    }

    /// <summary>
    /// вызывается после новой активности
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="recentActivities"></param>
    /// <returns></returns>

    public async Task<List<Achievement>> CheckAfterActivityAsync(Guid userId, IEnumerable<Activity> recentActivities)
    {
        var activities = recentActivities.OrderByDescending(a => a.Date).ToList();

        var unlocked = new List<Achievement>();

        unlocked.AddRange(await CheckFirstEntry(userId, activities));
        unlocked.AddRange(await CheckStreaks(userId, activities));
        unlocked.AddRange(await CheckProductivity(userId, activities));
        unlocked.AddRange(await CheckSleep(userId, activities));
        unlocked.AddRange(await CheckBalance(userId, activities));
        unlocked.AddRange(await CheckVeteran(userId, activities));

        return unlocked;
    }

    /// <summary>
    /// Вызывается после КПТ-сессии
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="isCompleted"></param>
    /// <param name="emotionShift"></param>
    /// <returns></returns>

    public async Task<List<Achievement>> CheckAfterCbtAsync(Guid userId, bool isCompleted, int emotionShift)
    {
        var unlocked = new List<Achievement>();
        var completed = (await _cbtRepo.GetCompletedByUserAsync(userId)).ToList();

        unlocked.AddRange(await CheckCbtMilestones(userId, completed.Count));

        if (isCompleted && emotionShift >= 50)
            unlocked.AddRange(await Grant(userId, AchievementType.MindShift, $"Сдвиг: {emotionShift}%", repeatable: true));

        return unlocked;
    }

    /// <summary>
    /// Вызывается после генерации профиля
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="archetypeName"></param>
    /// <returns></returns>

    public async Task<List<Achievement>> CheckAfterProfileAsync(Guid userId, string archetypeName)
    {
        var unlocked = new List<Achievement>();

        if (!await _repo.HasAsync(userId, AchievementType.FirstProfile))
            unlocked.AddRange(await Grant(userId, AchievementType.FirstProfile, archetypeName));

        return unlocked;
    }

    /// <summary>
    /// Получить статистику достижений 
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>

    public static AchievementInfo GetInfo(AchievementType type) =>
        Meta.TryGetValue(type, out var m) ? new AchievementInfo(type.ToString(), m.Emoji, m.Title, m.Description) : new AchievementInfo(type.ToString(), "🏅", type.ToString(), "");

    public static Dictionary<AchievementType, AchievementMeta> GetAllMeta() => Meta;

    private async Task<List<Achievement>> CheckFirstEntry(Guid userId, List<Activity> activities)
    {
        var result = new List<Achievement>();
        if (activities.Count == 1 && !await _repo.HasAsync(userId, AchievementType.FirstEntry))
            result.AddRange(await Grant(userId, AchievementType.FirstEntry));
        return result;
    }

    private async Task<List<Achievement>> CheckStreaks(Guid userId, List<Activity> activities)
    {
        var result = new List<Achievement>();
        var streak = CalculateStreak(activities);

        if (streak >= 7 && !await _repo.HasAsync(userId, AchievementType.Streak7Days))
            result.AddRange(await Grant(userId, AchievementType.Streak7Days, $"{streak} дней"));

        if (streak >= 30 && !await _repo.HasAsync(userId, AchievementType.Streak30Days))
            result.AddRange(await Grant(userId, AchievementType.Streak30Days, $"{streak} дней"));

        if (streak >= 100 && !await _repo.HasAsync(userId, AchievementType.Streak100Days))
            result.AddRange(await Grant(userId, AchievementType.Streak100Days, $"{streak} дней"));

        return result;
    }

    private async Task<List<Achievement>> CheckProductivity(Guid userId, List<Activity> activities)
    {
        var result = new List<Achievement>();
        var scores = activities.Select(a => CalculateProductivity(a)).ToList();

        if (scores.Any())
        {
            var current = scores.First();
            var previous = scores.Skip(1).DefaultIfEmpty(0).Max();
            if (current > previous && current >= 85)
                result.AddRange(await Grant(userId, AchievementType.ProductivityRecord, $"{Math.Round(current, 0)}/100", repeatable: true));
        }

        if (scores.Take(3).Count(s => s > 90) == 3 && !await _repo.HasAsync(userId, AchievementType.ProductivitySuperstar))
            result.AddRange(await Grant(userId, AchievementType.ProductivitySuperstar));

        if (scores.Take(7).Count() == 7 && scores.Take(7).All(s => s >= 70) && !await _repo.HasAsync(userId, AchievementType.ProductivityConsistent))
            result.AddRange(await Grant(userId, AchievementType.ProductivityConsistent));

        return result;
    }

    private async Task<List<Achievement>> CheckSleep(Guid userId, List<Activity> activities)
    {
        var result = new List<Achievement>();
        var recent = activities.Take(7).ToList();

        if (recent.Count >= 5 && recent.Take(5).All(a => a.SleepHours >= 8) && !await _repo.HasAsync(userId, AchievementType.SleepMaster))
            result.AddRange(await Grant(userId, AchievementType.SleepMaster));

        if (recent.Count(a => a.SleepHours < 6) >= 5)
            result.AddRange(await Grant(userId, AchievementType.NightOwl, "", repeatable: true));

        if (activities.Count >= 2 &&  activities[1].SleepHours < 6 && activities[0].SleepHours >= 7 && !await _repo.HasAsync(userId, AchievementType.SleepRecovered))
            result.AddRange(await Grant(userId, AchievementType.SleepRecovered));

        return result;
    }

    private async Task<List<Achievement>> CheckBalance(Guid userId, List<Activity> activities)
    {
        var result = new List<Achievement>();
        var recent = activities.Take(7).ToList();

        if (activities.Take(3).All(a => CalculateBalance(a) >= 80) && !await _repo.HasAsync(userId, AchievementType.WorkLifeBalance))
            result.AddRange(await Grant(userId, AchievementType.WorkLifeBalance));

        if (recent.Count >= 7 && recent.All(a => a.WorkHours <= 9) && !await _repo.HasAsync(userId, AchievementType.NoOverwork))
            result.AddRange(await Grant(userId, AchievementType.NoOverwork));

        if (activities.Take(5).All(a => a.FocusLevel >= 9) && !await _repo.HasAsync(userId, AchievementType.DeepFocus))
            result.AddRange(await Grant(userId, AchievementType.DeepFocus));

        return result;
    }

    private async Task<List<Achievement>> CheckVeteran( Guid userId, List<Activity> activities)
    {
        var result = new List<Achievement>();
        var count = activities.Count;

        if (count >= 50 && !await _repo.HasAsync(userId, AchievementType.Veteran))
            result.AddRange(await Grant(userId, AchievementType.Veteran, $"{count} записей"));

        if (count >= 200 && !await _repo.HasAsync(userId, AchievementType.Legend))
            result.AddRange(await Grant(userId, AchievementType.Legend, $"{count} записей"));

        return result;
    }

    private async Task<List<Achievement>> CheckCbtMilestones(Guid userId, int completedCount)
    {
        var result = new List<Achievement>();

        if (completedCount >= 1 && !await _repo.HasAsync(userId, AchievementType.FirstCbt))
            result.AddRange(await Grant(userId, AchievementType.FirstCbt));

        if (completedCount >= 10 && !await _repo.HasAsync(userId, AchievementType.CbtPractitioner))
            result.AddRange(await Grant(userId, AchievementType.CbtPractitioner, $"{completedCount} сессий"));

        if (completedCount >= 30 && !await _repo.HasAsync(userId, AchievementType.CbtMaster))
            result.AddRange(await Grant(userId, AchievementType.CbtMaster, $"{completedCount} сессий"));

        return result;
    }

    /// <summary>
    /// Утилиты
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="type"></param>
    /// <param name="context"></param>
    /// <param name="repeatable"></param>
    /// <returns></returns>

    private async Task<List<Achievement>> Grant(Guid userId, AchievementType type, string context = "", bool repeatable = false)
    {
        if (!repeatable && await _repo.HasAsync(userId, type))
            return [];

        if (repeatable)
        {
            var existing = (await _repo.GetByUserAsync(userId))
                .Where(a => a.Type == type)
                .OrderByDescending(a => a.UnlockedAt)
                .FirstOrDefault();

            if (existing?.UnlockedAt.Date == DateTime.UtcNow.Date)
                return [];
        }

        var achievement = new Achievement
        {
            UserId = userId,
            Type = type,
            Context = context,
            IsNew = true
        };

        await _repo.CreateAsync(achievement);
        return [achievement];
    }

    private static int CalculateStreak(List<Activity> activities)
    {
        if (!activities.Any()) return 0;
        var streak = 1;
        var checkDate = activities.First().Date.Date;

        foreach (var a in activities.Skip(1))
        {
            if (checkDate - a.Date.Date == TimeSpan.FromDays(1))
            {
                streak++;
                checkDate = a.Date.Date;
            }
            else break;
        }
        return streak;
    }

    private static double CalculateProductivity(Activity a)
    {
        var sleepQ = Math.Min(a.SleepHours / 8.0, 1.0) * 100;
        var overwork = a.WorkHours > 10 ? (a.WorkHours - 10) * 5 : 0;
        return Math.Clamp(
            sleepQ * 0.3 + a.FocusLevel * 10 * 0.45 +
            a.Mood * 10 * 0.25 - overwork, 0, 100);
    }

    private static double CalculateBalance(Activity a)
    {
        var sleepB = 1 - Math.Abs(a.SleepHours - 8) / 8.0;
        var workB = 1 - Math.Abs(a.WorkHours - 7) / 9.0;
        var leisure = Math.Max(0, 24 - a.SleepHours - a.WorkHours);
        var leisureB = Math.Min(leisure / 6.0, 1.0);
        return Math.Clamp(sleepB * 35 + workB * 35 + leisureB * 30, 0, 100);
    }
}

public record AchievementMeta(string Emoji, string Title, string Description);

public record AchievementInfo(string Type, string Emoji, string Title, string Description);