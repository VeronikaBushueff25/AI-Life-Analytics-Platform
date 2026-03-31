using AILifeAnalytics.Application.DTOs;
using AILifeAnalytics.Application.Services;
using AILifeAnalytics.Domain.Interfaces;
using MediatR;

namespace AILifeAnalytics.Application.Queries.CBT;

/// <summary>
/// Запрос одной КПТ-сессии по ID
/// </summary>
public record GetCbtByIdQuery(Guid RecordId, Guid UserId) : IRequest<CbtRecordResponse?>;

/// <summary>
/// Обработчик запроса одной КПТ-сессии по ID
/// </summary>
public class GetCbtByIdHandler : IRequestHandler<GetCbtByIdQuery, CbtRecordResponse?>
{
    private readonly ICbtRepository _cbtRepo;

    public GetCbtByIdHandler(ICbtRepository cbtRepo) => _cbtRepo = cbtRepo;

    public async Task<CbtRecordResponse?> Handle(GetCbtByIdQuery query, CancellationToken cancellationToken)
    {
        var record = await _cbtRepo.GetByIdAsync(query.RecordId);
        if (record is null || record.UserId != query.UserId) return null;
        return CbtService.MapToResponse(record);
    }
}