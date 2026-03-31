using AILifeAnalytics.Application.Commands.Profile;
using AILifeAnalytics.Application.DTOs;
using AILifeAnalytics.Application.Queries.Profile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AILifeAnalytics.Controllers;

/// <summary>
/// AI-профиль личности
/// </summary>
[Authorize]
[ApiController]
[Route("api/profile")]
[Produces("application/json")]
public class ProfileController : ControllerBase
{
    private readonly IMediator _mediator;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public ProfileController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Сгенерировать новый профиль личности
    /// </summary>
    [HttpPost("generate")]
    public async Task<ActionResult<ApiResponse<ProfileResponse>>> Generate()
    {
        try
        {
            var result = await _mediator.Send(new GenerateProfileCommand(UserId));
            return Ok(ApiResponse<ProfileResponse>.Ok(result));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<ProfileResponse>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Получить последний сгенерированный профиль
    /// </summary>
    [HttpGet("latest")]
    public async Task<ActionResult<ApiResponse<ProfileResponse?>>> GetLatest()
    {
        var result = await _mediator.Send(new GetLatestProfileQuery(UserId));
        return Ok(ApiResponse<ProfileResponse?>.Ok(result));
    }

    /// <summary>
    /// История профилей — как менялся архетип со временем
    /// </summary>
    [HttpGet("history")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProfileResponse>>>> GetHistory([FromQuery] int count = 5)
    {
        var result = await _mediator.Send(new GetProfileHistoryQuery(UserId, count));
        return Ok(ApiResponse<IEnumerable<ProfileResponse>>.Ok(result));
    }
}