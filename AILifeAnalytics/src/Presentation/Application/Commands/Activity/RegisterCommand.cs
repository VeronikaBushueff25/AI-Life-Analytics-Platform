using AILifeAnalytics.Domain.Entities;
using AILifeAnalytics.Domain.Interfaces;
using MediatR;

namespace AILifeAnalytics.Application.Commands.Auth;

/// <summary>
/// Команда регистрации нового пользователя
/// </summary>
public record RegisterCommand(string Email, string Password, string Name) : IRequest<AuthResult>;

/// <summary>
/// Обработчик команды регистрации
/// </summary>
public class RegisterHandler : IRequestHandler<RegisterCommand, AuthResult>
{
    private readonly IAuthService _authService;

    public RegisterHandler(IAuthService authService) => _authService = authService;

    public Task<AuthResult> Handle(RegisterCommand command, CancellationToken cancellationToken)
        => _authService.RegisterAsync(command.Email, command.Password, command.Name);
}