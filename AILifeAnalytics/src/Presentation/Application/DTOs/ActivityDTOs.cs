using AILifeAnalytics.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace AILifeAnalytics.Application.DTOs;

/// <summary>
/// Requests
/// </summary>
public class CreateActivityRequest
{
    [Required]
    public DateTime Date { get; set; }

    [Range(0, 12, ErrorMessage = "Sleep hours must be between 0 and 12")]
    public double SleepHours { get; set; }

    [Range(0, 16, ErrorMessage = "Work hours must be between 0 and 16")]
    public double WorkHours { get; set; }

    [Range(1, 10, ErrorMessage = "Focus level must be between 1 and 10")]
    public int FocusLevel { get; set; }

    [Range(1, 10, ErrorMessage = "Mood must be between 1 and 10")]
    public int Mood { get; set; }

    [MaxLength(1000)]
    public string Notes { get; set; } = string.Empty;
}

public class UpdateActivityRequest : CreateActivityRequest { }

/// <summary>
/// Responses
/// </summary>

public class ActivityResponse
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public double SleepHours { get; set; }
    public double WorkHours { get; set; }
    public int FocusLevel { get; set; }
    public int Mood { get; set; }
    public string Notes { get; set; } = string.Empty;
    public double ProductivityScore { get; set; }
    public double EnergyLevel { get; set; }
}

public class DashboardResponse
{
    public MetricsResponse Metrics { get; set; } = new();
    public IEnumerable<ActivityResponse> RecentActivities { get; set; } = [];
    public IEnumerable<ChartDataPoint> ProductivityChart { get; set; } = [];
    public IEnumerable<ChartDataPoint> MoodChart { get; set; } = [];
    public IEnumerable<ChartDataPoint> SleepChart { get; set; } = [];
    public TimeDistribution TimeDistribution { get; set; } = new();
    public InsightResponse? LatestInsight { get; set; }
}

public class MetricsResponse
{
    public double ProductivityScore { get; set; }
    public double EnergyLevel { get; set; }
    public double BurnoutRisk { get; set; }
    public double LifeBalanceIndex { get; set; }
    public int ConsistencyStreak { get; set; }
    public BurnoutStatus BurnoutStatus { get; set; } 
}

public class ChartDataPoint
{
    public string Label { get; set; } = string.Empty;
    public double Value { get; set; }
}

public class TimeDistribution
{
    public double AvgSleepHours { get; set; }
    public double AvgWorkHours { get; set; }
    public double AvgLeisureHours { get; set; }
}

public class InsightResponse
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public string Content { get; set; } = string.Empty;
    public AnalysisType AnalysisType { get; set; }
    public double ProductivityScore { get; set; }
    public double BurnoutRisk { get; set; }
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }

    public static ApiResponse<T> Ok(T data) => new() { Success = true, Data = data };
    public static ApiResponse<T> Fail(string error) => new() { Success = false, Error = error };
}

/// <summary>
/// DTOs для настроек
/// </summary>

public class ProviderInfo
{
    public string Name { get; set; } = string.Empty;
    public bool HasKey { get; set; }       
    public bool IsActive { get; set; }
}

public class SaveSettingsRequest
{
    public string ActiveProvider { get; set; } = "OpenAI";
    public Dictionary<string, string> ApiKeys { get; set; } = new();
}

public class ProxySettings
{
    public bool Enabled { get; set; } = false;
    public string Host { get; set; } = "";  
    public int Port { get; set; } = 1080; 
    public string Username { get; set; } = "";   // опционально
    public string Password { get; set; } = "";   // опционально

    /// <summary>
    /// Собрать Uri для WebProxy
    /// </summary>
    public string ToUrl() => $"http://{Host}:{Port}";
}

public class ProxySettingsDto
{
    public bool Enabled { get; set; } = false;
    public string Host { get; set; } = "";
    public int Port { get; set; } = 1080;
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}

public class SettingsResponse
{
    public IEnumerable<ProviderInfo> Providers { get; set; } = [];
    public ProxySettingsDto Proxy { get; set; } = new();
}

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
