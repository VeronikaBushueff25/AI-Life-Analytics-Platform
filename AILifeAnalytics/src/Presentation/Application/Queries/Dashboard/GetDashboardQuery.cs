using AILifeAnalytics.Application.DTOs;
using AILifeAnalytics.Application.Services;
using MediatR;

namespace AILifeAnalytics.Application.Queries.Dashboard;

/// <summary>
/// Запрос данных дашборда для конкретного пользователя
/// </summary>
public sealed record GetDashboardQuery(Guid UserId) : IRequest<DashboardResponse>;

/// <summary>
/// Обработчик запроса дашборда
/// </summary>
public class GetDashboardHandler : IRequestHandler<GetDashboardQuery, DashboardResponse>
{
    private readonly ActivityService _activityService;

    public GetDashboardHandler(ActivityService activityService)=> _activityService = activityService;

    public Task<DashboardResponse> Handle(GetDashboardQuery query, CancellationToken cancellationToken)
        => _activityService.GetDashboardAsync(query.UserId);
}