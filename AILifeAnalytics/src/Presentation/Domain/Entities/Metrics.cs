using AILifeAnalytics.Domain.Enums;

namespace AILifeAnalytics.Domain.Entities;

public class Metrics
{
    public double ProductivityScore { get; set; }
    public double EnergyLevel { get; set; }
    public double BurnoutRisk { get; set; }
    public double LifeBalanceIndex { get; set; }
    public int ConsistencyStreak { get; set; }
    public BurnoutStatus BurnoutStatus { get; set; } = BurnoutStatus.Low;
}
