using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AILifeAnalytics.Domain.Entities;
using AILifeAnalytics.Domain.Enums;
using AILifeAnalytics.Domain.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace AILifeAnalytics.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IUserSettingsRepository _settings;
    private readonly IConfiguration _config;

    public AuthService(
        IUserRepository users,
        IUserSettingsRepository settings,
        IConfiguration config)
    {
        _users = users;
        _settings = settings;
        _config = config;
    }

    public async Task<AuthResult> RegisterAsync(string email, string password, string name)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            return AuthResult.Fail("Некорректный email.");

        if (password.Length < 6)
            return AuthResult.Fail("Пароль должен содержать минимум 6 символов.");

        if (await _users.ExistsAsync(email))
            return AuthResult.Fail("Пользователь с таким email уже существует.");

        var user = new User
        {
            Email = email.ToLower().Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Name = name.Trim(),
            Role = UserRole.User
        };

        await _users.CreateAsync(user);
        await _settings.CreateAsync(new UserSettings { UserId = user.Id });

        var token = GenerateToken(user);
        return AuthResult.Ok(token, ToDto(user));
    }

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        var user = await _users.GetByEmailAsync(email.ToLower().Trim());

        if (user is null || !user.IsActive)
            return AuthResult.Fail("Неверный email или пароль.");

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return AuthResult.Fail("Неверный email или пароль.");

        user.LastLogin = DateTime.UtcNow;
        await _users.UpdateAsync(user);

        var token = GenerateToken(user);
        return AuthResult.Ok(token, ToDto(user));
    }

    public async Task<User?> GetUserByTokenAsync(string token)
    {
        try
        {
            var principal = ValidateToken(token);
            var idClaim = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (idClaim is null || !Guid.TryParse(idClaim, out var userId)) return null;
            return await _users.GetByIdAsync(userId);
        }
        catch { return null; }
    }

    /// <summary>
    /// JWT helpers
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>

    private string GenerateToken(User user)
    {
        var key = GetKey();
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddDays(_config.GetValue<int>("Jwt:ExpirationDays", 30));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"] ?? "AILifeAnalytics",
            audience: _config["Jwt:Audience"] ?? "AILifeAnalytics",
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private ClaimsPrincipal? ValidateToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var parameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = GetKey(),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
        return handler.ValidateToken(token, parameters, out _);
    }

    private SymmetricSecurityKey GetKey()
    {
        var secret = _config["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret not configured.");
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
    }

    private static UserDto ToDto(User u) => new()
    {
        Id = u.Id,
        Email = u.Email,
        Name = u.Name,
        Role = u.Role.ToString()
    };
}