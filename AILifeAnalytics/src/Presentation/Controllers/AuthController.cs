using AILifeAnalytics.Application.Commands.Auth;
using AILifeAnalytics.Application.DTOs;
using AILifeAnalytics.Application.Queries.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AILifeAnalytics.Controllers;

/// <summary>
/// Аутентификация: регистрация, вход, данные текущего пользователя 
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Регистрация нового пользователя
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<object>>> Register([FromBody] RegisterRequest req)
    {
        var result = await _mediator.Send(new RegisterCommand(req.Email, req.Password, req.Name));

        if (!result.Success)
            return BadRequest(ApiResponse<object>.Fail(result.Error!));

        return Ok(ApiResponse<object>.Ok(new
        {
            token = result.Token,
            user = result.User
        }));
    }

    /// <summary>
    /// Вход
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<object>>> Login([FromBody] LoginRequest req)
    {
        var result = await _mediator.Send(new LoginCommand(req.Email, req.Password));

        if (!result.Success)
            return Unauthorized(ApiResponse<object>.Fail(result.Error!));

        return Ok(ApiResponse<object>.Ok(new
        {
            token = result.Token,
            user = result.User
        }));
    }

    /// <summary>
    /// Данные текущего авторизованного пользователя
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<object>>> Me()
    {
        var result = await _mediator.Send(new GetCurrentUserQuery(
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
            User.FindFirstValue(ClaimTypes.Email) ?? "",
            User.FindFirstValue(ClaimTypes.Name) ?? "",
            User.FindFirstValue(ClaimTypes.Role) ?? ""
        ));
        return Ok(ApiResponse<object>.Ok(result));
    }
}