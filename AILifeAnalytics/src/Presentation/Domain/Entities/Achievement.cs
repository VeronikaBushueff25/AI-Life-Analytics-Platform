using AILifeAnalytics.Domain.Enums;

namespace AILifeAnalytics.Domain.Entities;

/// <summary>
/// Достижение пользователя. Создаётся один раз при выполнении условия
/// </summary>
public class Achievement
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public AchievementType Type { get; set; }
    public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// доп. данные
    /// </summary>
    public string Context { get; set; } = string.Empty; 

    /// <summary>
    /// не просмотрено
    /// </summary>
    public bool IsNew { get; set; } = true; 

    /// <summary>
    /// Navigation
    /// </summary>
    public User? User { get; set; }
}