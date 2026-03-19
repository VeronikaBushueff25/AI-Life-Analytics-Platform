using AILifeAnalytics.Application.DTOs;
using AILifeAnalytics.Application.Services;
using AILifeAnalytics.Domain.Entities;
using AILifeAnalytics.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AILifeAnalytics.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ActivityController : ControllerBase
{
    private readonly ActivityService _activityService;
    private readonly ILogger<ActivityController> _logger;

    public ActivityController(ActivityService activityService, ILogger<ActivityController> logger)
    {
        _activityService = activityService;
        _logger = logger;
    }

    /// <summary>Get all activities ordered by date descending</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ActivityResponse>>>> GetAll()
    {
        var result = await _activityService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<ActivityResponse>>.Ok(result));
    }

    /// <summary>Create a new daily activity entry</summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ActivityResponse>>> Create([FromBody] CreateActivityRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<ActivityResponse>.Fail(
                string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))));

        try
        {
            var result = await _activityService.CreateAsync(request);
            return CreatedAtAction(nameof(GetAll), ApiResponse<ActivityResponse>.Ok(result));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<ActivityResponse>.Fail(ex.Message));
        }
    }

    /// <summary>Update an existing activity</summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ActivityResponse>>> Update(Guid id, [FromBody] UpdateActivityRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<ActivityResponse>.Fail("Invalid data."));

        try
        {
            var result = await _activityService.UpdateAsync(id, request);
            return Ok(ApiResponse<ActivityResponse>.Ok(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<ActivityResponse>.Fail(ex.Message));
        }
    }

    /// <summary>Delete an activity</summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        var deleted = await _activityService.DeleteAsync(id);
        return deleted
            ? Ok(ApiResponse<bool>.Ok(true))
            : NotFound(ApiResponse<bool>.Fail("Activity not found."));
    }
}

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly ActivityService _activityService;

    public DashboardController(ActivityService activityService)
    {
        _activityService = activityService;
    }

    /// <summary>Get full dashboard data: metrics, charts, recent entries</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<DashboardResponse>>> GetDashboard()
    {
        var dashboard = await _activityService.GetDashboardAsync();
        return Ok(ApiResponse<DashboardResponse>.Ok(dashboard));
    }
}

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

    /// <summary>Generate AI insight based on recent data</summary>
    [HttpPost("analyze")]
    public async Task<ActionResult<ApiResponse<InsightResponse>>> Analyze()
    {
        var activities = (await _activityRepo.GetAllAsync())
            .OrderByDescending(a => a.Date)
            .Take(14)
            .ToList();

        if (!activities.Any())
            return BadRequest(ApiResponse<InsightResponse>.Fail("No data available for analysis."));

        var metrics = await _metricsService.CalculateAsync(activities);
        var content = await _aiService.GenerateInsightAsync(activities, metrics);

        var insight = new Insight
        {
            Content = content,
            Date = DateTime.UtcNow,
            AnalysisType = "general",
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

    /// <summary>Analyze behavioral patterns over time</summary>
    [HttpPost("patterns")]
    public async Task<ActionResult<ApiResponse<InsightResponse>>> AnalyzePatterns()
    {
        var activities = (await _activityRepo.GetAllAsync())
            .OrderByDescending(a => a.Date)
            .Take(14)
            .ToList();

        var content = await _aiService.AnalyzePatternAsync(activities);

        var insight = new Insight
        {
            Content = content,
            Date = DateTime.UtcNow,
            AnalysisType = "patterns"
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

    /// <summary>Get insight history</summary>
    [HttpGet("insights")]
    public async Task<ActionResult<ApiResponse<IEnumerable<InsightResponse>>>> GetInsights([FromQuery] int count = 10)
    {
        var insights = await _insightRepo.GetRecentAsync(count);
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
