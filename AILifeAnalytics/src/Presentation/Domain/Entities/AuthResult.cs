namespace AILifeAnalytics.Domain.Entities;

public class AuthResult
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? Error { get; set; }
    public UserDto? User { get; set; }

    public static AuthResult Ok(string token, UserDto user) => new() { Success = true, Token = token, User = user };

    public static AuthResult Fail(string error) => new() { Success = false, Error = error };
}

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}