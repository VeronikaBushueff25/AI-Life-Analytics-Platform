using AILifeAnalytics.Application.DTOs;
using AILifeAnalytics.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AILifeAnalytics.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class DashboardController : ControllerBase
    {
        private readonly ActivityService _activityService;
        private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        public DashboardController(ActivityService activityService)
        {
            _activityService = activityService;
        }

        /// <summary>
        /// Get full dashboard data: metrics, charts, recent entries
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<DashboardResponse>>> GetDashboard()
        {
            var dashboard = await _activityService.GetDashboardAsync(UserId);
            return Ok(ApiResponse<DashboardResponse>.Ok(dashboard));
        }
    }
}
