using AILifeAnalytics.Application.DTOs;

namespace AILifeAnalytics.Domain.Entities;

public class AISettings
{
    /// <summary>
    /// Имя активного провайдера: "OpenAI" | "DeepSeek" | "HuggingFace" | "GoogleAI"...
    /// </summary>
    public string ActiveProvider { get; set; } = "OpenAI";

    /// <summary>
    /// Настройки прокси. Если Enabled = false — прокси не используется.
    /// </summary>
    public ProxySettings Proxy { get; set; } = new();

    /// <summary>
    /// Словарь ключей: ProviderName → ApiKey
    /// </summary>
    public Dictionary<string, string> ApiKeys { get; set; } = new()
    {
        ["OpenAI"] = "",
        ["DeepSeek"] = "",
        ["HuggingFace"] = "",
        ["GoogleAI"] = ""
    };
}