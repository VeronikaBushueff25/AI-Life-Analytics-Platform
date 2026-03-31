using AILifeAnalytics.Application.Services;
using AILifeAnalytics.Domain.Interfaces;
using MediatR;

namespace AILifeAnalytics.Application.Commands.Activity;

/// <summary>
/// Команда удаления записи
/// </summary>
public sealed record DeleteActivityCommand(Guid ActivityId, Guid UserId) : IRequest<bool>;

/// <summary>
/// Обработчик команды удаления записи активности
/// </summary>
public class DeleteActivityHandler : IRequestHandler<DeleteActivityCommand, bool>
{
    private readonly ActivityService _activityService;

    public DeleteActivityHandler(ActivityService activityService) => _activityService = activityService;

    public Task<bool> Handle(DeleteActivityCommand command, CancellationToken cancellationToken)
        => _activityService.DeleteAsync(command.ActivityId, command.UserId);
}