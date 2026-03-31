using AILifeAnalytics.Application.DTOs;
using AILifeAnalytics.Application.Services;
using AILifeAnalytics.Domain.Interfaces;
using MediatR;

namespace AILifeAnalytics.Application.Queries.CBT;

/// <summary>
/// Запрос истории КПТ-сессий пользователя
/// </summary>
public record GetCbtRecordsQuery(Guid UserId, int Count = 20) : IRequest<IEnumerable<CbtRecordResponse>>;

/// <summary>
/// Обработчик запроса истории КПТ-сессий
/// </summary>
public class GetCbtRecordsHandler : IRequestHandler<GetCbtRecordsQuery, IEnumerable<CbtRecordResponse>>
{
    private readonly ICbtRepository _cbtRepo;

    public GetCbtRecordsHandler(ICbtRepository cbtRepo) => _cbtRepo = cbtRepo;

    public async Task<IEnumerable<CbtRecordResponse>> Handle(GetCbtRecordsQuery query, CancellationToken cancellationToken)
    {
        var records = await _cbtRepo.GetByUserAsync(query.UserId, query.Count);
        return records.Select(CbtService.MapToResponse);
    }
}