using AILifeAnalytics.Application.DTOs;
using AILifeAnalytics.Domain.Entities;
using AILifeAnalytics.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AILifeAnalytics.Controllers
{
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

        /// <summary>
        /// Generate AI insight based on recent data
        /// </summary>
        [HttpPost("analyze")]
        public async Task<ActionResult<ApiResponse<InsightResponse>>> Analyze()
        {
            var activities = (await _activityRepo.GetAllAsync()).OrderByDescending(a => a.Date).Take(14).ToList();

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

        /// <summary>
        /// Analyze behavioral patterns over time
        /// </summary>
        [HttpPost("patterns")]
        public async Task<ActionResult<ApiResponse<InsightResponse>>> AnalyzePatterns()
        {
            var activities = (await _activityRepo.GetAllAsync()).OrderByDescending(a => a.Date).Take(14).ToList();
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

        /// <summary>
        /// Get insight history
        /// </summary>
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
}
