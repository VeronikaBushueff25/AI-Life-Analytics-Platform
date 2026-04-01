using AILifeAnalytics.Domain.Enums;

namespace AILifeAnalytics.Domain.Entities;

/// <summary>
/// Запись КПТ-практики. Одна запись = одна сессия работы с мыслью
/// Хранит все этапы ABC-модели + AI-анализ
/// </summary>
public class CbtRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public CbtRecordType Type { get; set; } = CbtRecordType.ThoughtDiary;

    /// <summary>
    /// A: Activating event (ситуация)
    /// </summary>
    public string Situation { get; set; } = string.Empty; // что произошло
    public string PhysicalState { get; set; } = string.Empty; // как тело реагировало

    /// <summary>
    /// B: Beliefs (автоматические мысли)
    /// </summary>
    public string AutomaticThought { get; set; } = string.Empty; // первая мысль
    public int ThoughtBelief { get; set; } = 50;           // уверенность 0-100%

    /// <summary>
    /// C: Consequences (эмоции и поведение)
    /// </summary>
    public EmotionType PrimaryEmotion { get; set; } = EmotionType.Other;
    public int EmotionIntensity { get; set; } = 50; // 0-100%
    public string AdditionalEmotions { get; set; } = string.Empty;
    public string Behavior { get; set; } = string.Empty; // что сделал

    /// <summary>
    /// AI анализ 
    /// </summary>
    public string DetectedDistortions { get; set; } = "[]"; 
    public string AiChallenge { get; set; } = string.Empty; // AI оспаривает мысль
    public string AiQuestions { get; set; } = "[]"; // сократовские вопросы
    public string EvidenceFor { get; set; } = string.Empty; // доказательства ЗА
    public string EvidenceAgainst { get; set; } = string.Empty; // доказательства ПРОТИВ

    /// <summary>
    /// D: Disputation (оспаривание) — заполняет пользователь
    /// </summary>
    public string ReframedThought { get; set; } = string.Empty; // новая мысль
    public int NewThoughtBelief { get; set; } = 50; // уверенность в новой мысли
    public int NewEmotionIntensity { get; set; } = 50; // новая интенсивность эмоции

    /// <summary>
    /// Итог 
    /// </summary>
    public string Insight { get; set; } = string.Empty; // что понял
    public bool IsCompleted { get; set; } = false;
    public string AiSummary { get; set; } = string.Empty; // итоговый AI-комментарий
    public int EmotionShift => EmotionIntensity - NewEmotionIntensity;

    /// <summary>
    /// Navigation
    /// </summary>
    public User? User { get; set; }
}