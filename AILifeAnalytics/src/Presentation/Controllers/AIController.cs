using AILifeAnalytics.Application.DTOs;
using AILifeAnalytics.Domain.Entities;
using AILifeAnalytics.Domain.Enums;
using AILifeAnalytics.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AILifeAnalytics.Controllers;

[Authorize]
[ApiController]
[Route("api/ai")]
[Produces("application/json")]
public class AIController : ControllerBase
{
    private readonly IAIService _aiService;
    private readonly IActivityRepository _activityRepo;
    private readonly IInsightRepository _insightRepo;
    private readonly IMetricsService _metricsService;
    private readonly ILogger<AIController> _logger;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public AIController(
        IAIService aiService,
        IActivityRepository activityRepo,
        IInsightRepository insightRepo,
        IMetricsService metricsService,
        ILogger<AIController> logger)
    {
        _aiService = aiService;
        _activityRepo = activityRepo;
        _insightRepo = insightRepo;
        _metricsService = metricsService;
        _logger = logger;
    }

    [HttpPost("analyze")]
    public async Task<ActionResult<ApiResponse<InsightResponse>>> Analyze()
    {
        var activities = (await _activityRepo.GetByUserAsync(UserId)).OrderByDescending(a => a.Date).Take(14).ToList();

        if (!activities.Any())
            return BadRequest(ApiResponse<InsightResponse>.Fail("No data available for analysis."));

        var metrics = await _metricsService.CalculateAsync(activities);
        var content = await _aiService.GenerateInsightAsync(activities, metrics);

        var insight = new Insight
        {
            UserId = UserId,
            Content = content,
            Date = DateTime.UtcNow,
            AnalysisType = AnalysisType.General,
            ProductivityScore = metrics.ProductivityScore,
            BurnoutRisk = metrics.BurnoutRisk
        };

        var saved = await _insightRepo.CreateAsync(insight);

        return Ok(ApiResponse<InsightResponse>.Ok(new InsightResponse
        {
            Id = saved.Id,
            Date = saved.Date,
            Content = saved.Content,
            AnalysisType = saved.AnalysisType,
            ProductivityScore = saved.ProductivityScore,
            BurnoutRisk = saved.BurnoutRisk
        }));
    }

    [HttpPost("patterns")]
    public async Task<ActionResult<ApiResponse<InsightResponse>>> AnalyzePatterns()
    {
        var activities = (await _activityRepo.GetByUserAsync(UserId)).OrderByDescending(a => a.Date).Take(14).ToList();
        var content = await _aiService.AnalyzePatternAsync(activities);

        var insight = new Insight
        {
            UserId = UserId,        
            Content = content,
            Date = DateTime.UtcNow,
            AnalysisType = AnalysisType.Patterns
        };

        var saved = await _insightRepo.CreateAsync(insight);

        return Ok(ApiResponse<InsightResponse>.Ok(new InsightResponse
        {
            Id = saved.Id,
            Date = saved.Date,
            Content = saved.Content,
            AnalysisType = saved.AnalysisType
        }));
    }

    [HttpGet("insights")]
    public async Task<ActionResult<ApiResponse<IEnumerable<InsightResponse>>>> GetInsights([FromQuery] int count = 10)
    {
        var insights = await _insightRepo.GetByUserAsync(UserId, count);

        var response = insights.Select(i => new InsightResponse
        {
            Id = i.Id,
            Date = i.Date,
            Content = i.Content,
            AnalysisType = i.AnalysisType,
            ProductivityScore = i.ProductivityScore,
            BurnoutRisk = i.BurnoutRisk
        });

        return Ok(ApiResponse<IEnumerable<InsightResponse>>.Ok(response));
    }
}