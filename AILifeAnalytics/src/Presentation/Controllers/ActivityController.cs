using AILifeAnalytics.Application.DTOs;
using AILifeAnalytics.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AILifeAnalytics.Controllers
{
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

        /// <summary>
        /// Get all activities ordered by date descending
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<ActivityResponse>>>> GetAll()
        {
            var result = await _activityService.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<ActivityResponse>>.Ok(result));
        }

        /// <summary>
        /// Create a new daily activity entry
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<ActivityResponse>>> Create([FromBody] CreateActivityRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<ActivityResponse>.Fail(string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))));

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

        /// <summary>
        /// Update an existing activity
        /// </summary>
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

        /// <summary>
        /// Delete an activity
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
        {
            var deleted = await _activityService.DeleteAsync(id);
            return deleted ? Ok(ApiResponse<bool>.Ok(true)) : NotFound(ApiResponse<bool>.Fail("Activity not found."));
        }
    }
}
