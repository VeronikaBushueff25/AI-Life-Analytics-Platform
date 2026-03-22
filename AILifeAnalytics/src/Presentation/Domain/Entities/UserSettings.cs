using AILifeAnalytics.Domain.Enums;

namespace AILifeAnalytics.Domain.Entities;

public class UserSettings
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public ProviderName ActiveProvider { get; set; } = ProviderName.OpenAI;

    /// <summary>
    /// API ключи — в реальном проекте шифровать (AES)
    /// </summary>
    public string OpenAIKey { get; set; } = string.Empty;
    public string DeepSeekKey { get; set; } = string.Empty;
    public string HuggingFaceKey { get; set; } = string.Empty;
    public string GoogleAIKey { get; set; } = string.Empty;

    /// <summary>
    /// Прокси
    /// </summary>
    public bool ProxyEnabled { get; set; } = false;
    public string ProxyHost { get; set; } = string.Empty;
    public int ProxyPort { get; set; } = 1080;
    public string ProxyUsername { get; set; } = string.Empty;
    public string ProxyPassword { get; set; } = string.Empty;

    /// <summary>
    /// Личные цели
    /// </summary>
    public double SleepGoal { get; set; } = 8;
    public double FocusGoal { get; set; } = 7;
    public double MoodGoal { get; set; } = 7;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation
    /// </summary>
    public User? User { get; set; }
}