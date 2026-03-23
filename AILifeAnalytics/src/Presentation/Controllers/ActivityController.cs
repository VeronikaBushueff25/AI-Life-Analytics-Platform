using AILifeAnalytics.Application.DTOs;
using AILifeAnalytics.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AILifeAnalytics.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ActivityController : ControllerBase
{
    private readonly ActivityService _activityService;
    private readonly ILogger<ActivityController> _logger;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public ActivityController(ActivityService activityService, ILogger<ActivityController> logger)
    {
        _activityService = activityService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ActivityResponse>>>> GetAll()
    {
        var result = await _activityService.GetAllAsync(UserId);
        return Ok(ApiResponse<IEnumerable<ActivityResponse>>.Ok(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ActivityResponse>>> Create(
        [FromBody] CreateActivityRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<ActivityResponse>.Fail(string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))));
        try
        {
            var result = await _activityService.CreateAsync(request, UserId);
            return CreatedAtAction(nameof(GetAll), ApiResponse<ActivityResponse>.Ok(result));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<ActivityResponse>.Fail(ex.Message));
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ActivityResponse>>> Update(Guid id, [FromBody] UpdateActivityRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<ActivityResponse>.Fail("Invalid data."));
        try
        {
            var result = await _activityService.UpdateAsync(id, request, UserId);
            return Ok(ApiResponse<ActivityResponse>.Ok(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<ActivityResponse>.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        var deleted = await _activityService.DeleteAsync(id, UserId);
        return deleted ? Ok(ApiResponse<bool>.Ok(true)) : NotFound(ApiResponse<bool>.Fail("Activity not found."));
    }
}