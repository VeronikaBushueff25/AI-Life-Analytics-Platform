using AILifeAnalytics.Application.Commands.Activity;
using AILifeAnalytics.Application.DTOs;
using AILifeAnalytics.Application.Queries.Activity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AILifeAnalytics.Controllers;

/// <summary>
/// Управление ежедневными записями активности
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ActivityController : ControllerBase
{
    private readonly IMediator _mediator;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public ActivityController(IMediator mediator) => _mediator = mediator;

    /// <summary>Получить все записи пользователя, отсортированные по дате</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ActivityResponse>>>> GetAll()
    {
        var result = await _mediator.Send(new GetAllActivitiesQuery(UserId));
        return Ok(ApiResponse<IEnumerable<ActivityResponse>>.Ok(result));
    }

    /// <summary>
    /// Создать новую дневную запись (одна запись на дату)
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ActivityResponse>>> Create([FromBody] CreateActivityRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<ActivityResponse>.Fail(string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))));
        try
        {
            var result = await _mediator.Send(new CreateActivityCommand(UserId, request));
            return CreatedAtAction(nameof(GetAll), ApiResponse<ActivityResponse>.Ok(result));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<ActivityResponse>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Обновить существующую запись
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ActivityResponse>>> Update(Guid id, [FromBody] UpdateActivityRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<ActivityResponse>.Fail("Invalid data."));
        try
        {
            var result = await _mediator.Send(new UpdateActivityCommand(id, UserId, request));
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

    /// <summary>
    /// Удалить запись
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        var deleted = await _mediator.Send(new DeleteActivityCommand(id, UserId));
        return deleted ? Ok(ApiResponse<bool>.Ok(true)) : NotFound(ApiResponse<bool>.Fail("Activity not found."));
    }
}