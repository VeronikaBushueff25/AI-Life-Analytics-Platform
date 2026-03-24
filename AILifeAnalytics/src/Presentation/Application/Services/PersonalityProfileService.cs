using System.Text.Json;
using AILifeAnalytics.Domain.Entities;
using AILifeAnalytics.Domain.Interfaces;

namespace AILifeAnalytics.Application.Services;

public class PersonalityProfileService
{
    private readonly IActivityRepository _activityRepo;
    private readonly IPersonalityProfileRepository _profileRepo;
    private readonly IAIService _aiService;

    public PersonalityProfileService(
        IActivityRepository activityRepo,
        IPersonalityProfileRepository profileRepo,
        IAIService aiService)
    {
        _activityRepo = activityRepo;
        _profileRepo = profileRepo;
        _aiService = aiService;
    }

    public async Task<PersonalityProfile> GenerateAsync(Guid userId)
    {
        var activities = (await _activityRepo.GetByUserAsync(userId)).OrderByDescending(a => a.Date).Take(90).ToList();

        if (activities.Count < 7)
            throw new InvalidOperationException( "Нужно минимум 7 дней записей для построения профиля.");

        var stats = CalculateStats(activities);
        var prompt = BuildProfilePrompt(activities, stats);
        var aiJson = await _aiService.GenerateProfileAsync(prompt);
        var aiResult = ParseAIResponse(aiJson);

        var profile = new PersonalityProfile
        {
            UserId = userId,
            GeneratedAt = DateTime.UtcNow,
            PeriodFrom = activities.Last().Date,
            PeriodTo = activities.First().Date,
            DaysAnalyzed = activities.Count,

            ArchetypeName = aiResult.ArchetypeName,
            ArchetypeDescription = aiResult.ArchetypeDescription,
            ArchetypeEmoji = aiResult.ArchetypeEmoji,

            PeakPerformancePattern = aiResult.PeakPerformancePattern,
            EnergyPattern = aiResult.EnergyPattern,
            StressPattern = aiResult.StressPattern,

            Superpowers = JsonSerializer.Serialize(aiResult.Superpowers),
            Vulnerabilities = JsonSerializer.Serialize(aiResult.Vulnerabilities),
            Recommendations = JsonSerializer.Serialize(aiResult.Recommendations),

            OptimalSleepHours = stats.OptimalSleepHours,
            OptimalWorkHours = stats.OptimalWorkHours,
            MostProductiveDayOfWeek = stats.BestDayOfWeek,
            CorrelationSleepFocus = stats.CorrelationSleepFocus,
            CorrelationStressMood = stats.CorrelationStressMood,

            ForecastText = aiResult.ForecastText,
            ForecastRisk = aiResult.ForecastRisk,
            FullAnalysis = aiResult.FullAnalysis
        };

        return await _profileRepo.CreateAsync(profile);
    }

    /// <summary>
    /// Числовая статистика 
    /// </summary>
    /// <param name="activities"></param>
    /// <returns></returns>

    private ProfileStats CalculateStats(List<Activity> activities)
    {
        var sorted = activities
            .Select(a => new
            {
                a.SleepHours,
                a.WorkHours,
                a.StressLevel,
                Productivity = CalculateProductivity(a),
                DayOfWeek = a.Date.DayOfWeek
            }).OrderByDescending(x => x.Productivity).ToList();

        var top25Percent = sorted.Take(Math.Max(1, sorted.Count / 4)).ToList();
        var sleepValues = activities.Select(a => a.SleepHours).ToList();
        var focusValues = activities.Select(a => (double)a.FocusLevel).ToList();
        var moodValues = activities.Select(a => (double)a.Mood).ToList();
        var stressValues = activities.Select(a => (double)a.StressLevel).ToList();

        var byDayOfWeek = activities.GroupBy(a => a.Date.DayOfWeek)
            .Select(g => new
            {
                Day = g.Key,
                AvgP = g.Average(a => CalculateProductivity(a))
            }).OrderByDescending(x => x.AvgP) .First();

        return new ProfileStats
        {
            OptimalSleepHours = Math.Round(top25Percent.Average(x => x.SleepHours), 1),
            OptimalWorkHours = Math.Round(top25Percent.Average(x => x.WorkHours), 1),
            BestDayOfWeek = GetDayNameRu(byDayOfWeek.Day),
            CorrelationSleepFocus = Math.Round(PearsonCorrelation(sleepValues, focusValues), 2),
            CorrelationStressMood = Math.Round(PearsonCorrelation(stressValues, moodValues), 2),
            AvgProductivity = Math.Round(activities.Average(a => CalculateProductivity(a)), 1),
            AvgSleep = Math.Round(activities.Average(a => a.SleepHours), 1),
            AvgWork = Math.Round(activities.Average(a => a.WorkHours), 1),
            AvgStress = Math.Round(activities.Average(a => a.StressLevel), 1),
            DaysOverworked = activities.Count(a => a.WorkHours > 10),
            DaysUnderlsept = activities.Count(a => a.SleepHours < 6),
            BestProductivity = Math.Round(sorted.First().Productivity, 1),
            WorstProductivity = Math.Round(sorted.Last().Productivity, 1)
        };
    }

    /// <summary>
    /// Промпт
    /// </summary>
    /// <param name="activities"></param>
    /// <param name="stats"></param>
    /// <returns></returns>

    private string BuildProfilePrompt(List<Activity> activities, ProfileStats stats)
    {
        var notesExamples = activities.Where(a => !string.IsNullOrEmpty(a.Notes))
            .Take(10).Select(a => $"- {a.Date:dd.MM}: {a.Notes}").ToList();

        var moodReasons = activities.Where(a => !string.IsNullOrEmpty(a.MoodReason))
            .Take(10).Select(a => $"- {a.MoodReason}").ToList();

        return @$"
            Ты — психолог-аналитик и коуч по производительности. 
            Проанализируй данные пользователя за {stats.DaysAnalyzed} дней 
            и создай его поведенческий профиль. Отвечай ТОЛЬКО валидным JSON.

            СТАТИСТИКА:
            - Средняя продуктивность: {stats.AvgProductivity}/100
            - Лучший результат: {stats.BestProductivity}/100
            - Средний сон: {stats.AvgSleep}ч
            - Оптимальный сон (в лучшие дни): {stats.OptimalSleepHours}ч  
            - Средняя работа: {stats.AvgWork}ч
            - Оптимальная работа (без выгорания): {stats.OptimalWorkHours}ч
            - Лучший день недели: {stats.BestDayOfWeek}
            - Корреляция сон↔фокус: {stats.CorrelationSleepFocus} (от -1 до 1)
            - Корреляция стресс↔настроение: {stats.CorrelationStressMood}
            - Дней переработки (>10ч): {stats.DaysOverworked}
            - Дней недосыпания (<6ч): {stats.DaysUnderlsept}

            ЗАМЕТКИ ПОЛЬЗОВАТЕЛЯ:
            {(notesExamples.Any() ? string.Join("\n", notesExamples) : "нет заметок")}

            ПРИЧИНЫ НАСТРОЕНИЯ:
            {(moodReasons.Any() ? string.Join("\n", moodReasons) : "не указывались")}

            ЗАДАНИЕ — верни JSON строго в этом формате:
            {{
              ""archetypeName"": ""Название архетипа (2-3 слова, поэтично)"",
              ""archetypeEmoji"": ""один эмодзи"",
              ""archetypeDescription"": ""2 предложения — кто этот человек по своей природе"",
              ""peakPerformancePattern"": ""1 предложение — когда этот человек в потоке"",
              ""energyPattern"": ""1 предложение — как восстанавливается энергия"",
              ""stressPattern"": ""1 предложение — что убивает продуктивность"",
              ""superpowers"": [""суперсила 1"", ""суперсила 2"", ""суперсила 3""],
              ""vulnerabilities"": [""уязвимость 1"", ""уязвимость 2""],
              ""recommendations"": [""действие 1"", ""действие 2"", ""действие 3""],
              ""forecastText"": ""2 предложения — что будет через месяц если продолжит так же"",
              ""forecastRisk"": ""Low/Medium/High"",
              ""fullAnalysis"": ""Развёрнутый анализ 4-5 предложений, глубоко и лично""
            }}";
    }

    /// <summary>
    /// Парсинг AI-ответа
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>

    private AIProfileResult ParseAIResponse(string json)
    {
        try
        {
            var clean = json.Replace("```json", "").Replace("```", "").Trim();
            return JsonSerializer.Deserialize<AIProfileResult>(clean, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? GetFallbackResult();
        }
        catch
        {
            return GetFallbackResult();
        }
    }

    private AIProfileResult GetFallbackResult() => new()
    {
        ArchetypeName = "Искатель баланса",
        ArchetypeEmoji = "⚖️",
        ArchetypeDescription = "Человек в поиске своего оптимального ритма.",
        PeakPerformancePattern = "Лучшие результаты после полноценного сна.",
        EnergyPattern = "Энергия восстанавливается через отдых и смену деятельности.",
        StressPattern = "Продуктивность снижается при переработках.",
        Superpowers = ["Стремление к самоанализу"],
        Vulnerabilities = ["Нестабильный режим"],
        Recommendations = ["Стабилизировать время сна", "Ограничить рабочие часы"],
        ForecastText = "Продолжайте вести дневник для более точного анализа.",
        ForecastRisk = "Medium",
        FullAnalysis = "Недостаточно данных для полного анализа."
    };

    /// <summary>
    /// Утилиты
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>

    private static double CalculateProductivity(Activity a)
    {
        var sleepQ = Math.Min(a.SleepHours / 8.0, 1.0) * 100;
        var overwork = a.WorkHours > 10 ? (a.WorkHours - 10) * 5 : 0;
        return Math.Clamp(sleepQ * 0.3 + a.FocusLevel * 10 * 0.45 + a.Mood * 10 * 0.25 - overwork, 0, 100);
    }

    private static double PearsonCorrelation(List<double> x, List<double> y)
    {
        if (x.Count != y.Count || x.Count < 2) return 0;
        var meanX = x.Average();
        var meanY = y.Average();
        var num = x.Zip(y, (xi, yi) => (xi - meanX) * (yi - meanY)).Sum();
        var denX = Math.Sqrt(x.Sum(xi => Math.Pow(xi - meanX, 2)));
        var denY = Math.Sqrt(y.Sum(yi => Math.Pow(yi - meanY, 2)));
        return (denX * denY) == 0 ? 0 : num / (denX * denY);
    }

    private static string GetDayNameRu(DayOfWeek day) => day switch
    {
        DayOfWeek.Monday => "Понедельник",
        DayOfWeek.Tuesday => "Вторник",
        DayOfWeek.Wednesday => "Среда",
        DayOfWeek.Thursday => "Четверг",
        DayOfWeek.Friday => "Пятница",
        DayOfWeek.Saturday => "Суббота",
        DayOfWeek.Sunday => "Воскресенье",
        _ => "Неизвестно"
    };
}

public class ProfileStats
{
    public double OptimalSleepHours { get; set; }
    public double OptimalWorkHours { get; set; }
    public string BestDayOfWeek { get; set; } = string.Empty;
    public double CorrelationSleepFocus { get; set; }
    public double CorrelationStressMood { get; set; }
    public double AvgProductivity { get; set; }
    public double AvgSleep { get; set; }
    public double AvgWork { get; set; }
    public double AvgStress { get; set; }
    public int DaysOverworked { get; set; }
    public int DaysUnderlsept { get; set; }
    public double BestProductivity { get; set; }
    public double WorstProductivity { get; set; }
    public int DaysAnalyzed { get; set; }
}

public class AIProfileResult
{
    public string ArchetypeName { get; set; } = string.Empty;
    public string ArchetypeEmoji { get; set; } = string.Empty;
    public string ArchetypeDescription { get; set; } = string.Empty;
    public string PeakPerformancePattern { get; set; } = string.Empty;
    public string EnergyPattern { get; set; } = string.Empty;
    public string StressPattern { get; set; } = string.Empty;
    public List<string> Superpowers { get; set; } = [];
    public List<string> Vulnerabilities { get; set; } = [];
    public List<string> Recommendations { get; set; } = [];
    public string ForecastText { get; set; } = string.Empty;
    public string ForecastRisk { get; set; } = string.Empty;
    public string FullAnalysis { get; set; } = string.Empty;
}