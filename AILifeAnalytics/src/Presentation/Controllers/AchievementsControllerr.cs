using AILifeAnalytics.Application.DTOs;
using AILifeAnalytics.Application.Queries.Achievements;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AILifeAnalytics.Controllers;

/// <summary>
/// Достижения пользователя: разблокированные, заблокированные, прогресс
/// </summary>
[Authorize]
[ApiController]
[Route("api/achievements")]
[Produces("application/json")]
public class AchievementsController : ControllerBase
{
    private readonly IMediator _mediator;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public AchievementsController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Страница достижений: разблокированные + заблокированные + прогресс.
    /// Помечает новые достижения как просмотренные.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<AchievementsPageResponse>>> Get()
    {
        var result = await _mediator.Send(new GetAchievementsQuery(UserId));
        return Ok(ApiResponse<AchievementsPageResponse>.Ok(result));
    }
}