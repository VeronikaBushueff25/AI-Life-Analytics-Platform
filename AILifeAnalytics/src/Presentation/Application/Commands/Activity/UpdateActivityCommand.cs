using AILifeAnalytics.Application.DTOs;
using AILifeAnalytics.Application.Services;
using MediatR;

namespace AILifeAnalytics.Application.Commands.Activity;

/// <summary>
/// Команда обновления существующей записи активности
/// </summary>
public record UpdateActivityCommand(Guid ActivityId, Guid UserId,UpdateActivityRequest Request) : IRequest<ActivityResponse>;

/// <summary>
/// Обработчик команды обновления записи активности
/// </summary>
public class UpdateActivityHandler : IRequestHandler<UpdateActivityCommand, ActivityResponse>
{
    private readonly ActivityService _activityService;

    public UpdateActivityHandler(ActivityService activityService) => _activityService = activityService;

    public Task<ActivityResponse> Handle(UpdateActivityCommand command, CancellationToken cancellationToken)
        => _activityService.UpdateAsync(command.ActivityId, command.Request, command.UserId);
}