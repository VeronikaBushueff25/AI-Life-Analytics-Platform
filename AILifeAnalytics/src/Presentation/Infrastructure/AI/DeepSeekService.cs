using AILifeAnalytics.Domain.Interfaces;

namespace AILifeAnalytics.Infrastructure.AI;

/// <summary>
/// DeepSeek — OpenAI-совместимый API, только меняется URL и модель.
/// Добавить нового провайдера = создать такой же класс на 10 строк.
/// </summary>
public class DeepSeekProvider : OpenAICompatibleProvider
{
    public override string ProviderName => "DeepSeek";
    protected override string BaseUrl => "https://api.deepseek.com/v1/chat/completions";
    protected override string ModelName => "deepseek-chat";

    public DeepSeekProvider(
        IHttpClientFactory factory,
        ISettingsRepository settings,
        ILogger<DeepSeekProvider> logger)
        : base(factory, settings, logger) { }
}