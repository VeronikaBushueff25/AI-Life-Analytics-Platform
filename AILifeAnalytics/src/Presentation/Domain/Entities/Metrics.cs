namespace AILifeAnalytics.Domain.Entities;

public class Metrics
{
    public double ProductivityScore { get; set; }   // 0-100
    public double EnergyLevel { get; set; }         // 0-100
    public double BurnoutRisk { get; set; }         // 0-100
    public double LifeBalanceIndex { get; set; }    // 0-100
    public int ConsistencyStreak { get; set; }      // days
    public string BurnoutStatus { get; set; } = "Low"; // Low | Medium | High | Critical
}
