namespace AILifeAnalytics.Domain.Entities;

public class Insight
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string Content { get; set; } = string.Empty;
    public string AnalysisType { get; set; } = "general"; // general | weekly | burnout_alert
    public double ProductivityScore { get; set; }
    public double BurnoutRisk { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
