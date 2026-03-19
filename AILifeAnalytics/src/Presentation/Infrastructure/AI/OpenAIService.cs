using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AILifeAnalytics.Domain.Entities;
using AILifeAnalytics.Domain.Interfaces;

namespace AILifeAnalytics.Infrastructure.AI;

/// <summary>
/// Базовый класс для OpenAI-совместимых API (OpenAI, DeepSeek...).
/// Провайдер переопределяет BaseUrl, ModelName и способ получения ключа
/// </summary>
public abstract class OpenAICompatibleProvider : IAIProvider
{
    protected readonly IHttpClientFactory HttpClientFactory;
    protected readonly ILogger Logger;
    protected readonly ISettingsRepository SettingsRepository;

    protected abstract string BaseUrl { get; }
    protected abstract string ModelName { get; }
    public abstract string ProviderName { get; }

    protected OpenAICompatibleProvider(
        IHttpClientFactory httpClientFactory,
        ISettingsRepository settingsRepository,
        ILogger logger)
    {
        HttpClientFactory = httpClientFactory;
        SettingsRepository = settingsRepository;
        Logger = logger;
    }

    public async Task<string> GenerateInsightAsync(IEnumerable<Activity> activities, Metrics metrics)
    {
        var list = activities.OrderByDescending(a => a.Date).Take(7).ToList();
        if (!list.Any())
            return "Недостаточно данных для анализа. Добавьте хотя бы одну запись.";

        return await CallAsync(BuildInsightPrompt(list, metrics));
    }

    public async Task<string> AnalyzePatternAsync(IEnumerable<Activity> activities)
    {
        var list = activities.OrderByDescending(a => a.Date).Take(14).ToList();
        if (list.Count < 3)
            return "Для анализа паттернов нужно минимум 3 дня данных.";

        return await CallAsync(BuildPatternPrompt(list));
    }

    /// <summary>
    /// Промпты 
    /// </summary>
    /// <param name="activities"></param>
    /// <param name="metrics"></param>
    /// <returns></returns>

    private string BuildInsightPrompt(List<Activity> activities, Metrics metrics)
    {
        var summary = string.Join("\n", activities.Select(a =>
            $"- {a.Date:dd.MM}: сон {a.SleepHours}ч, работа {a.WorkHours}ч, " +
            $"фокус {a.FocusLevel}/10, настроение {a.Mood}/10" +
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

    protected virtual async Task<string> CallAsync(string prompt)
    {
        try
        {
            var settings = await SettingsRepository.GetAsync();
            var apiKey = settings.ApiKeys.GetValueOrDefault(ProviderName, "");

            if (string.IsNullOrWhiteSpace(apiKey))
                return $"API-ключ для {ProviderName} не настроен. Перейдите в Настройки.";

            var requestBody = new
            {
                model = ModelName,
                messages = new[] { new { role = "user", content = prompt } },
                max_tokens = 400,
                temperature = 0.7
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var client = HttpClientFactory.CreateClient(ProviderName);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var response = await client.PostAsync(BaseUrl, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Logger.LogError("{Provider} API error: {Status} - {Body}", ProviderName, response.StatusCode, responseBody);
                return HandleError(responseBody);
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
            Logger.LogError(ex, "Failed to call {Provider} API", ProviderName);
            return $"Ошибка при обращении к {ProviderName}. Проверьте соединение.";
        }
    }

    private string HandleError(string responseBody)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            var error = doc.RootElement.GetProperty("error");
            var code = error.GetProperty("code").GetString();
            var message = error.GetProperty("message").GetString();

            return code switch
            {
                "unsupported_country_region_territory" => $"{ProviderName} недоступен в вашем регионе. Попробуйте VPN.",
                "invalid_api_key" => $"Неверный API-ключ {ProviderName}. Проверьте настройки.",
                "insufficient_quota" =>  $"Закончился лимит API {ProviderName}. Пополните баланс.",
                "rate_limit_exceeded" => "Слишком много запросов. Подождите несколько секунд.",
                _ =>
                    $"Ошибка {ProviderName}: {message}"
            };
        }
        catch
        {
            return $"Не удалось обработать ошибку {ProviderName}. Попробуйте позже.";
        }
    }
}
public class OpenAIProvider : OpenAICompatibleProvider
{
    public override string ProviderName => "OpenAI";
    protected override string BaseUrl => "https://api.openai.com/v1/chat/completions";
    protected override string ModelName => "gpt-4o-mini";

    public OpenAIProvider(
        IHttpClientFactory factory,
        ISettingsRepository settings,
        ILogger<OpenAIProvider> logger)
        : base(factory, settings, logger) { }
}