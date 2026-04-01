using AILifeAnalytics.Application.DTOs;
using AILifeAnalytics.Application.Services;
using AILifeAnalytics.Domain.Interfaces;
using MediatR;

namespace AILifeAnalytics.Application.Commands.Activity;

/// <summary>
/// Команда создания новой дневной записи
/// </summary>
public record CreateActivityCommand(Guid UserId, CreateActivityRequest Request) : IRequest<CreateActivityResult>;

/// <summary>
/// После создания записи проверяет достижения
/// Новые достижения сохраняются в БД и возвращаются в ответе
/// </summary>
public class CreateActivityHandler : IRequestHandler<CreateActivityCommand, CreateActivityResult>
{
    private readonly ActivityService _activityService;
    private readonly AchievementService _achievementService;
    private readonly IActivityRepository _activityRepo;

    public CreateActivityHandler(
        ActivityService activityService,
        AchievementService achievementService,
        IActivityRepository activityRepo)
    {
        _activityService = activityService;
        _achievementService = achievementService;
        _activityRepo = activityRepo;
    }

    public async Task<CreateActivityResult> Handle(CreateActivityCommand command, CancellationToken cancellationToken)
    {
        var activity = await _activityService.CreateAsync(command.Request, command.UserId);
        var allActivities = (await _activityRepo.GetByUserAsync(command.UserId)).OrderByDescending(a => a.Date).ToList();
        var newAchievements = await _achievementService.CheckAfterActivityAsync(command.UserId, allActivities);

        return new CreateActivityResult
        {
            Activity = activity,
            NewAchievements = newAchievements.Select(a =>
            {
                var info = AchievementService.GetInfo(a.Type);
                return new AchievementResponse
                {
                    Id = a.Id,
                    Type = a.Type.ToString(),
                    Emoji = info.Emoji,
                    Title = info.Title,
                    Description = info.Description,
                    Context = a.Context,
                    UnlockedAt = a.UnlockedAt,
                    IsNew = true
                };
            }).ToList()
        };
    }
}

/// <summary>
/// Результат создания активности — включает новые достижения.
/// </summary>
public class CreateActivityResult
{
    public ActivityResponse Activity { get; set; } = new();
    public List<AchievementResponse> NewAchievements { get; set; } = [];
}