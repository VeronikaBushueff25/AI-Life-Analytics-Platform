using AILifeAnalytics.Application.Commands.AI;
using AILifeAnalytics.Application.DTOs;
using AILifeAnalytics.Application.Queries.AI;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AILifeAnalytics.Controllers;

/// <summary>
/// AI-анализ: генерация инсайтов, анализ паттернов, история
/// </summary>
[Authorize]
[ApiController]
[Route("api/ai")]
[Produces("application/json")]
public class AIController : ControllerBase
{
    private readonly IMediator _mediator;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public AIController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Сгенерировать персональный инсайт на основе последних 14 записей
    /// </summary>
    [HttpPost("analyze")]
    public async Task<ActionResult<ApiResponse<InsightResponse>>> Analyze()
    {
        try
        {
            var result = await _mediator.Send(new GenerateInsightCommand(UserId));
            return Ok(ApiResponse<InsightResponse>.Ok(result));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<InsightResponse>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Проанализировать поведенческие паттерны за 14 дней
    /// </summary>
    [HttpPost("patterns")]
    public async Task<ActionResult<ApiResponse<InsightResponse>>> AnalyzePatterns()
    {
        var result = await _mediator.Send(new AnalyzePatternsCommand(UserId));
        return Ok(ApiResponse<InsightResponse>.Ok(result));
    }

    /// <summary>
    /// Получить историю AI-инсайтов
    /// </summary>
    [HttpGet("insights")]
    public async Task<ActionResult<ApiResponse<IEnumerable<InsightResponse>>>> GetInsights([FromQuery] int count = 10)
    {
        var result = await _mediator.Send(new GetInsightsQuery(UserId, count));
        return Ok(ApiResponse<IEnumerable<InsightResponse>>.Ok(result));
    }
}