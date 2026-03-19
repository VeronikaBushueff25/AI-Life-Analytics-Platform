namespace AILifeAnalytics.Domain.Entities;

public class Activity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Date { get; set; }
    public double SleepHours { get; set; }   // 0-12
    public double WorkHours { get; set; }    // 0-16
    public int FocusLevel { get; set; }      // 1-10
    public int Mood { get; set; }            // 1-10
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
