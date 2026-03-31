using AILifeAnalytics.Application.DTOs;
using AILifeAnalytics.Application.Services;
using MediatR;

namespace AILifeAnalytics.Application.Queries.Activity;

/// <summary>
/// Запрос получения всех записей активности текущего пользователя
/// </summary>
public record GetAllActivitiesQuery(Guid UserId) : IRequest<IEnumerable<ActivityResponse>>;

/// <summary>
/// Обработчик запроса списка всех активностей пользователя
/// </summary>
public class GetAllActivitiesHandler : IRequestHandler<GetAllActivitiesQuery, IEnumerable<ActivityResponse>>
{
    private readonly ActivityService _activityService;

    public GetAllActivitiesHandler(ActivityService activityService) => _activityService = activityService;

    public Task<IEnumerable<ActivityResponse>> Handle(GetAllActivitiesQuery query, CancellationToken cancellationToken)
        => _activityService.GetAllAsync(query.UserId);
}