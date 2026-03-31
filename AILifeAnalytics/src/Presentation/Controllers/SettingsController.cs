using AILifeAnalytics.Application.Commands.Settings;
using AILifeAnalytics.Application.DTOs;
using AILifeAnalytics.Application.Queries.Settings;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AILifeAnalytics.Controllers;

/// <summary>
/// Настройки AI-провайдеров, API-ключей и HTTP-прокси
/// </summary>
[ApiController]
[Route("api/settings")]
[Produces("application/json")]
public class SettingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SettingsController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Список AI-провайдеров: статус ключа, активный + настройки прокси
    /// </summary>
    [HttpGet("providers")]
    public async Task<ActionResult<ApiResponse<SettingsResponse>>> GetProviders()
    {
        var result = await _mediator.Send(new GetProvidersQuery());
        return Ok(ApiResponse<SettingsResponse>.Ok(result));
    }

    /// <summary>
    /// Сохранить: активный провайдер, API-ключи, прокси
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<bool>>> Save([FromBody] SaveSettingsRequest request)
    {
        var result = await _mediator.Send(new SaveSettingsCommand(request));
        return Ok(ApiResponse<bool>.Ok(result));
    }

    /// <summary>
    /// Проверить HTTP-прокси
    /// </summary>
    [HttpPost("test-proxy")]
    public async Task<ActionResult<ApiResponse<string>>> TestProxy([FromBody] ProxySettings request)
    {
        var result = await _mediator.Send(new TestProxyCommand(
            request.Host,
            request.Port,
            request.Username,
            request.Password));

        bool isSuccess = result.StartsWith("Прокси работает");
        return isSuccess ? Ok(ApiResponse<string>.Ok(result)) : Ok(ApiResponse<string>.Fail(result));
    }
}