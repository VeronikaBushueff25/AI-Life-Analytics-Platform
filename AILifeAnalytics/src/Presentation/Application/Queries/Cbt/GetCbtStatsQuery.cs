using AILifeAnalytics.Application.DTOs;
using AILifeAnalytics.Application.Services;
using MediatR;

namespace AILifeAnalytics.Application.Queries.Cbt;

/// <summary>
/// Запрос статистики КПТ-практик пользователя
/// </summary>
public sealed record GetCbtStatsQuery(Guid UserId) : IRequest<CbtStatsResponse>;

/// <summary>
/// Обработчик запроса статистики КПТ-практик пользователя
/// </summary>
public class GetCbtStatsHandler : IRequestHandler<GetCbtStatsQuery, CbtStatsResponse>
{
    private readonly CbtService _cbtService;

    public GetCbtStatsHandler(CbtService cbtService) => _cbtService = cbtService;

    public Task<CbtStatsResponse> Handle(GetCbtStatsQuery query, CancellationToken cancellationToken)
        => _cbtService.GetStatsAsync(query.UserId);
}