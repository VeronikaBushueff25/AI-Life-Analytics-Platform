using AILifeAnalytics.Domain.Enums;

namespace AILifeAnalytics.Domain.Entities;

public class Insight
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }   
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string Content { get; set; } = string.Empty;
    public AnalysisType AnalysisType { get; set; } = AnalysisType.General;
    public double ProductivityScore { get; set; }
    public double BurnoutRisk { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User? User { get; set; }
}
