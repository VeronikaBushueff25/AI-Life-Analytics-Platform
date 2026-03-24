using System.Text.Json;
using AILifeAnalytics.Application.DTOs;
using AILifeAnalytics.Domain.Entities;
using AILifeAnalytics.Domain.Enums;
using AILifeAnalytics.Domain.Interfaces;

namespace AILifeAnalytics.Application.Services;

public class CbtService
{
    private readonly ICbtRepository _cbtRepo;
    private readonly IAIService _aiService;

    public CbtService(ICbtRepository cbtRepo, IAIService aiService)
    {
        _cbtRepo = cbtRepo;
        _aiService = aiService;
    }

    /// <summary>
    /// Шаг 1: Пользователь описал ситуацию и мысль.
    /// AI анализирует искажения и задаёт сократовские вопросы.
    /// </summary>
    public async Task<CbtRecord> AnalyzeThoughtAsync(Guid userId, CreateCbtRequest request)
    {
        var record = new CbtRecord
        {
            UserId = userId,
            Type = CbtRecordType.ThoughtDiary,
            Situation = request.Situation,
            PhysicalState = request.PhysicalState,
            AutomaticThought = request.AutomaticThought,
            ThoughtBelief = request.ThoughtBelief,
            PrimaryEmotion = request.PrimaryEmotion,
            EmotionIntensity = request.EmotionIntensity,
            Behavior = request.Behavior
        };

        var prompt = BuildAnalysisPrompt(record);
        var aiJson = await _aiService.GenerateCbtAnalysisAsync(prompt);
        var aiResult = ParseAnalysisResult(aiJson);

        record.DetectedDistortions = JsonSerializer.Serialize(aiResult.Distortions);
        record.AiChallenge = aiResult.Challenge;
        record.AiQuestions = JsonSerializer.Serialize(aiResult.Questions);
        record.EvidenceFor = aiResult.EvidenceFor;
        record.EvidenceAgainst = aiResult.EvidenceAgainst;

        return await _cbtRepo.CreateAsync(record);
    }

    /// <summary>
    /// Шаг 2: Пользователь ответил на вопросы и сформулировал новую мысль.
    /// AI даёт итоговый комментарий.
    /// </summary>
    public async Task<CbtRecord> CompleteReframingAsync(Guid id, Guid userId, CompleteReframingRequest request)
    {
        var record = await _cbtRepo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Запись КПТ не найдена.");

        if (record.UserId != userId)
            throw new UnauthorizedAccessException();

        record.ReframedThought = request.ReframedThought;
        record.NewThoughtBelief = request.NewThoughtBelief;
        record.NewEmotionIntensity = request.NewEmotionIntensity;
        record.Insight = request.Insight;
        record.IsCompleted = true;

        // AI даёт итоговый комментарий
        var prompt = BuildSummaryPrompt(record);
        record.AiSummary = await _aiService.GenerateCbtAnalysisAsync(prompt);

        return await _cbtRepo.UpdateAsync(record);
    }

    public async Task<CbtStatsResponse> GetStatsAsync(Guid userId)
    {
        var all = (await _cbtRepo.GetByUserAsync(userId, 100)).ToList();
        var completed = all.Where(r => r.IsCompleted).ToList();
        var stats = await _cbtRepo.GetDistortionStatsAsync(userId);

        // Средний сдвиг интенсивности эмоций
        double avgShift = completed.Any() ? completed.Average(r => r.EmotionIntensity - r.NewEmotionIntensity) : 0;

        // Самое частое искажение
        var topDistortion = stats.Any() ? stats.OrderByDescending(x => x.Value).First() : new KeyValuePair<string, int>("нет данных", 0);

        return new CbtStatsResponse
        {
            TotalSessions = all.Count,
            CompletedSessions = completed.Count,
            AvgEmotionShift = Math.Round(avgShift, 1),
            TopDistortion = topDistortion.Key,
            TopDistortionCount = topDistortion.Value,
            DistortionStats = stats
                .OrderByDescending(x => x.Value)
                .Take(5)
                .ToDictionary(x => x.Key, x => x.Value)
        };
    }

    /// <summary>
    /// Промпты 
    /// </summary>
    /// <param name="r"></param>
    /// <returns></returns>

    private string BuildAnalysisPrompt(CbtRecord r) => $$"""
        Ты — психолог, специалист по КПТ (когнитивно-поведенческой терапии).
        Проанализируй запись пользователя и помоги переосмыслить автоматическую мысль.
        Отвечай ТОЛЬКО валидным JSON.

        СИТУАЦИЯ: {{r.Situation}}
        АВТОМАТИЧЕСКАЯ МЫСЛЬ: "{{r.AutomaticThought}}" (уверенность: {{r.ThoughtBelief}}%)
        ЭМОЦИЯ: {{r.PrimaryEmotion}} (интенсивность: {{r.EmotionIntensity}}%)
        ПОВЕДЕНИЕ: {{r.Behavior}}
        ФИЗИЧЕСКОЕ СОСТОЯНИЕ: {{r.PhysicalState}}

        ЗАДАНИЕ — верни JSON строго в этом формате:
        {
        "distortions": ["название искажения 1", "название искажения 2"],
        "challenge": "Мягкое, поддерживающее оспаривание мысли. 2-3 предложения.",
        "questions": [
            "Сократовский вопрос 1 — помогает увидеть другую перспективу",
            "Сократовский вопрос 2",
            "Сократовский вопрос 3"
        ],
        "evidenceFor": "Какие факты ПОДДЕРЖИВАЮТ эту мысль? Честно.",
        "evidenceAgainst": "Какие факты ОПРОВЕРГАЮТ эту мысль? Что говорит против?"
        }

        Список возможных когнитивных искажений:
        Катастрофизация, Чёрно-белое мышление, Чтение мыслей, Предсказание будущего,
        Сверхобобщение, Навешивание ярлыков, Долженствование, Персонализация,
        Эмоциональные рассуждения, Ментальный фильтр

        Тон: тёплый, не осуждающий, поддерживающий. Не ставь диагнозов.
    """;

    private string BuildSummaryPrompt(CbtRecord r) => $"""
        Пользователь завершил упражнение по КПТ. Дай итоговый поддерживающий комментарий.

        ИСХОДНАЯ МЫСЛЬ: "{r.AutomaticThought}" (уверенность {r.ThoughtBelief}%)
        ИСХОДНАЯ ЭМОЦИЯ: {r.PrimaryEmotion} ({r.EmotionIntensity}%)

        НОВАЯ МЫСЛЬ: "{r.ReframedThought}" (уверенность {r.NewThoughtBelief}%)
        НОВАЯ ИНТЕНСИВНОСТЬ ЭМОЦИИ: {r.NewEmotionIntensity}%
        ИНСАЙТ ПОЛЬЗОВАТЕЛЯ: {r.Insight}

        Напиши 2-3 тёплых предложения:
        1. Отметь прогресс (снижение интенсивности эмоции на {r.EmotionIntensity - r.NewEmotionIntensity}%)
        2. Похвали за работу с мыслью
        3. Дай один конкретный совет на будущее

        Тон: как добрый психолог. Без диагнозов. На русском языке.
        """;

    private CbtAnalysisResult ParseAnalysisResult(string json)
    {
        try
        {
            var clean = json.Replace("```json", "").Replace("```", "").Trim();
            return JsonSerializer.Deserialize<CbtAnalysisResult>(clean, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? GetFallbackAnalysis();
        }
        catch { return GetFallbackAnalysis(); }
    }

    private static CbtAnalysisResult GetFallbackAnalysis() => new()
    {
        Distortions = ["Требует анализа"],
        Challenge = "Попробуйте взглянуть на ситуацию со стороны.",
        Questions = ["Что бы вы сказали другу в такой ситуации?",
                          "Какие есть альтернативные объяснения?",
                          "Как вы будете думать об этом через год?"],
        EvidenceFor = "Требует заполнения.",
        EvidenceAgainst = "Требует заполнения."
    };
}


public class CbtAnalysisResult
{
    public List<string> Distortions { get; set; } = [];
    public string Challenge { get; set; } = string.Empty;
    public List<string> Questions { get; set; } = [];
    public string EvidenceFor { get; set; } = string.Empty;
    public string EvidenceAgainst { get; set; } = string.Empty;
}