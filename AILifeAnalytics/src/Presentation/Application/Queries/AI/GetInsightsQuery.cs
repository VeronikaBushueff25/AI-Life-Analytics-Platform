using AILifeAnalytics.Application.DTOs;
using AILifeAnalytics.Domain.Interfaces;
using MediatR;

namespace AILifeAnalytics.Application.Queries.AI;

/// <summary>
/// Запрос истории AI-инсайтов пользователя
/// </summary>
public record GetInsightsQuery(Guid UserId, int Count = 10) : IRequest<IEnumerable<InsightResponse>>;

/// <summary>
/// Обработчик запроса истории AI-инсайтов пользователя
/// </summary>
public class GetInsightsHandler : IRequestHandler<GetInsightsQuery, IEnumerable<InsightResponse>>
{
    private readonly IInsightRepository _insightRepo;

    public GetInsightsHandler(IInsightRepository insightRepo) => _insightRepo = insightRepo;

    public async Task<IEnumerable<InsightResponse>> Handle(GetInsightsQuery query, CancellationToken cancellationToken)
    {
        var insights = await _insightRepo.GetByUserAsync(query.UserId, query.Count);

        return insights.Select(i => new InsightResponse
        {
            Id = i.Id,
            Date = i.Date,
            Content = i.Content,
            AnalysisType = i.AnalysisType,
            ProductivityScore = i.ProductivityScore,
            BurnoutRisk = i.BurnoutRisk
        });
    }
}