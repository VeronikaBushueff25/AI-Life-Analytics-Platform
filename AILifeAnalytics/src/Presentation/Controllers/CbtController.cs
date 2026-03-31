using AILifeAnalytics.Application.Commands.Cbt;
using AILifeAnalytics.Application.Commands.CBT;
using AILifeAnalytics.Application.DTOs;
using AILifeAnalytics.Application.Queries.Cbt;
using AILifeAnalytics.Application.Queries.CBT;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AILifeAnalytics.Controllers;

/// <summary>
/// КПТ-практики
/// </summary>
[Authorize]
[ApiController]
[Route("api/cbt")]
[Produces("application/json")]
public class CbtController : ControllerBase
{
    private readonly IMediator _mediator;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public CbtController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Создать сессию (шаг 1): описать ситуацию, получить AI-анализ
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<CbtRecordResponse>>> Create([FromBody] CreateCbtRequest request)
    {
        try
        {
            var result = await _mediator.Send(new CreateCbtCommand(UserId, request));
            return Ok(ApiResponse<CbtRecordResponse>.Ok(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<CbtRecordResponse>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Завершить переосмысление (шаг 2): сформулировать новую мысль
    /// </summary>
    [HttpPut("{id:guid}/complete")]
    public async Task<ActionResult<ApiResponse<CbtRecordResponse>>> Complete(Guid id, [FromBody] CompleteReframingRequest request)
    {
        try
        {
            var result = await _mediator.Send(new CompleteReframingCommand(id, UserId, request));
            return Ok(ApiResponse<CbtRecordResponse>.Ok(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<CbtRecordResponse>.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// История КПТ-сессий пользователя
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<CbtRecordResponse>>>> GetAll([FromQuery] int count = 20)
    {
        var result = await _mediator.Send(
            new GetCbtRecordsQuery(UserId, count));
        return Ok(ApiResponse<IEnumerable<CbtRecordResponse>>.Ok(result));
    }

    /// <summary>
    /// Получить одну КПТ-сессию по ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CbtRecordResponse>>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetCbtByIdQuery(id, UserId));
        if (result is null)
            return NotFound(ApiResponse<CbtRecordResponse>.Fail("Запись не найдена."));
        return Ok(ApiResponse<CbtRecordResponse>.Ok(result));
    }

    /// <summary>
    /// Удалить КПТ-сессию
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        var deleted = await _mediator.Send(new DeleteCbtCommand(id, UserId));
        return deleted ? Ok(ApiResponse<bool>.Ok(true)) : NotFound(ApiResponse<bool>.Fail("Запись не найдена."));
    }

    /// <summary>
    /// Статистика: топ когнитивных искажений, средний сдвиг эмоций
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<CbtStatsResponse>>> GetStats()
    {
        var result = await _mediator.Send(new GetCbtStatsQuery(UserId));
        return Ok(ApiResponse<CbtStatsResponse>.Ok(result));
    }
}