using AILifeAnalytics.Application.DTOs;
using AILifeAnalytics.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AILifeAnalytics.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    /// <summary>
    /// Регистрация нового пользователя
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<object>>> Register(
        [FromBody] RegisterRequest req)
    {
        var result = await _auth.RegisterAsync(req.Email, req.Password, req.Name);

        if (!result.Success)
            return BadRequest(ApiResponse<object>.Fail(result.Error!));

        return Ok(ApiResponse<object>.Ok(new
        {
            token = result.Token,
            user = result.User
        }));
    }

    /// <summary>
    /// Вход в систему
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<object>>> Login([FromBody] LoginRequest req)
    {
        var result = await _auth.LoginAsync(req.Email, req.Password);

        if (!result.Success)
            return Unauthorized(ApiResponse<object>.Fail(result.Error!));

        return Ok(ApiResponse<object>.Ok(new
        {
            token = result.Token,
            user = result.User
        }));
    }

    /// <summary>
    /// Получить данные текущего пользователя
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();

        return Ok(ApiResponse<object>.Ok(new
        {
            id = userId,
            email = User.FindFirstValue(ClaimTypes.Email),
            name = User.FindFirstValue(ClaimTypes.Name),
            role = User.FindFirstValue(ClaimTypes.Role)
        }));
    }
}