using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AILifeAnalytics.Domain.Entities;
using AILifeAnalytics.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AILifeAnalytics.Infrastructure.AI;

/// <summary>
/// Integrates with OpenAI API to generate personalized behavioral insights.
/// Prompt design follows principles: concise, actionable, no medical diagnoses.
/// </summary>
public class OpenAIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenAIService> _logger;
    private readonly string _apiKey;
    private const string ApiUrl = "https://api.openai.com/v1/chat/completions";

    public OpenAIService(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<OpenAIService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("OpenAI");
        _logger = logger;
        _apiKey = config["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI API key is not configured.");
    }

    public async Task<string> GenerateInsightAsync(IEnumerable<Activity> activities, Metrics metrics)
    {
        var activityList = activities.OrderByDescending(a => a.Date).Take(7).ToList();
        if (!activityList.Any())
            return "Недостаточно данных для анализа. Добавьте хотя бы одну запись.";

        var prompt = BuildInsightPrompt(activityList, metrics);
        return await CallOpenAIAsync(prompt);
    }

    public async Task<string> AnalyzePatternAsync(IEnumerable<Activity> activities)
    {
        var activityList = activities.OrderByDescending(a => a.Date).Take(14).ToList();
        if (activityList.Count < 3)
            return "Для анализа паттернов нужно минимум 3 дня данных.";

        var prompt = BuildPatternPrompt(activityList);
        return await CallOpenAIAsync(prompt);
    }

    private string BuildInsightPrompt(List<Activity> activities, Metrics metrics)
    {
        var summary = string.Join("\n", activities.Select(a =>
            $"- {a.Date:dd.MM}: сон {a.SleepHours}ч, работа {a.WorkHours}ч, фокус {a.FocusLevel}/10, настроение {a.Mood}/10" +
            (string.IsNullOrEmpty(a.Notes) ? "" : $", заметки: {a.Notes}")));

        return $"""
            Ты — аналитик личной эффективности. Проанализируй данные пользователя и дай конкретные рекомендации.
            
            ДАННЫЕ ЗА ПОСЛЕДНИЕ ДНИ:
            {summary}
            
            ТЕКУЩИЕ МЕТРИКИ:
            - Продуктивность: {metrics.ProductivityScore}/100
            - Уровень энергии: {metrics.EnergyLevel}/100
            - Риск выгорания: {metrics.BurnoutRisk}/100 ({metrics.BurnoutStatus})
            - Баланс жизни: {metrics.LifeBalanceIndex}/100
            - Стрик: {metrics.ConsistencyStreak} дней
            
            ТРЕБОВАНИЯ К ОТВЕТУ:
            1. Длина: 3-4 предложения
            2. Тон: поддерживающий, конкретный
            3. Включи: что идёт хорошо, что улучшить, конкретное действие на сегодня
            4. Не ставь медицинских диагнозов
            5. Отвечай на русском языке
            """;
    }

    private string BuildPatternPrompt(List<Activity> activities)
    {
        var avgSleep = activities.Average(a => a.SleepHours);
        var avgWork = activities.Average(a => a.WorkHours);
        var avgMood = activities.Average(a => a.Mood);
        var avgFocus = activities.Average(a => a.FocusLevel);

        var worstDay = activities.OrderBy(a => a.Mood).First();
        var bestDay = activities.OrderByDescending(a => a.Mood).First();

        return $"""
            Ты — аналитик поведенческих паттернов. Выяви закономерности и дай рекомендации.
            
            СТАТИСТИКА ЗА {activities.Count} ДНЕЙ:
            - Средний сон: {avgSleep:F1}ч (норма: 7-8ч)
            - Средняя работа: {avgWork:F1}ч
            - Среднее настроение: {avgMood:F1}/10
            - Средний фокус: {avgFocus:F1}/10
            - Лучший день: {bestDay.Date:dd.MM} (настроение {bestDay.Mood}, сон {bestDay.SleepHours}ч)
            - Сложный день: {worstDay.Date:dd.MM} (настроение {worstDay.Mood}, сон {worstDay.SleepHours}ч)
            
            Найди 2-3 чётких паттерна и предложи конкретные изменения. Ответ на русском, 4-5 предложений.
            """;
    }

    private async Task<string> CallOpenAIAsync(string prompt)
    {
        try
        {
            var requestBody = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                max_tokens = 400,
                temperature = 0.7
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiKey);

            var response = await _httpClient.PostAsync(ApiUrl, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("OpenAI API error: {StatusCode} - {Body}", response.StatusCode, responseBody);
                return "AI-анализ временно недоступен. Попробуйте позже.";
            }

            using var doc = JsonDocument.Parse(responseBody);
            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "Не удалось получить ответ.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to call OpenAI API");
            return "Ошибка при обращении к AI. Проверьте соединение и API-ключ.";
        }
    }
}
