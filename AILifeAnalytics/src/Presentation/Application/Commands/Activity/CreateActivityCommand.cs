using AILifeAnalytics.Application.DTOs;
using AILifeAnalytics.Application.Services;
using MediatR;

namespace AILifeAnalytics.Application.Commands.Activity;

/// <summary>
/// Команда создания новой дневной записи
/// </summary>
public sealed record CreateActivityCommand(Guid UserId, CreateActivityRequest Request) : IRequest<ActivityResponse>;

/// <summary>
/// Обработчик команды создания записи активности.
/// Делегирует логику в ActivityService, который проверяет дубликат
/// по дате и пользователю перед созданием.
/// </summary>
public class CreateActivityHandler : IRequestHandler<CreateActivityCommand, ActivityResponse>
{
    private readonly ActivityService _activityService;

    public CreateActivityHandler(ActivityService activityService) => _activityService = activityService;

    public Task<ActivityResponse> Handle(CreateActivityCommand command, CancellationToken cancellationToken)
        => _activityService.CreateAsync(command.Request, command.UserId);
}