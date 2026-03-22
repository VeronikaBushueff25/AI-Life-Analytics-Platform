namespace AILifeAnalytics.Domain.Entities;

public class Activity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }          
    public DateTime Date { get; set; }
    public double SleepHours { get; set; }
    public double WorkHours { get; set; }
    public int FocusLevel { get; set; }          
    public int Mood { get; set; }          
    public int StressLevel { get; set; }        
    public string Notes { get; set; } = string.Empty;
    public string MoodReason { get; set; } = string.Empty;  
    public string HighlightsOfDay { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public User? User { get; set; }
}
