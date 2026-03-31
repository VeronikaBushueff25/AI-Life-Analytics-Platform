using AILifeAnalytics.Domain.Entities;
using AILifeAnalytics.Domain.Interfaces;
using MediatR;

namespace AILifeAnalytics.Application.Commands.Auth;

/// <summary>
/// Команда входа пользователя
/// </summary>
public sealed record LoginCommand(string Email, string Password) : IRequest<AuthResult>;

/// <summary>
/// Проверяет учётные данные через BCrypt, обновляет LastLogin, генерирует JWT
/// </summary>
public sealed class LoginHandler : IRequestHandler<LoginCommand, AuthResult>
{
    private readonly IAuthService _authService;

    public LoginHandler(IAuthService authService) => _authService = authService;

    public async Task<AuthResult> Handle(LoginCommand command, CancellationToken cancellationToken)
        => await _authService.LoginAsync(command.Email, command.Password);
}