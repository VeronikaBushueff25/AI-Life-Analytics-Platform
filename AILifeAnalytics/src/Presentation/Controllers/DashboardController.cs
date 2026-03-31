using AILifeAnalytics.Application.DTOs;
using AILifeAnalytics.Application.Queries.Dashboard;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AILifeAnalytics.Controllers;

/// <summary>
/// Дашборд: KPI-метрики, данные для графиков, распределение времени
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public DashboardController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Получить полные данные дашборда для текущего пользователя
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<DashboardResponse>>> GetDashboard()
    {
        var result = await _mediator.Send(new GetDashboardQuery(UserId));
        return Ok(ApiResponse<DashboardResponse>.Ok(result));
    }
}