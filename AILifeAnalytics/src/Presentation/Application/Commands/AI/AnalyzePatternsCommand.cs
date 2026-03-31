using AILifeAnalytics.Application.DTOs;
using AILifeAnalytics.Domain.Entities;
using AILifeAnalytics.Domain.Enums;
using AILifeAnalytics.Domain.Interfaces;
using MediatR;

namespace AILifeAnalytics.Application.Commands.AI;

/// <summary>
/// Команда анализа поведенческих паттернов за последние 14 дней
/// </summary>
public record AnalyzePatternsCommand(Guid UserId) : IRequest<InsightResponse>;

/// <summary>
/// Обработчик команды анализа поведенческих паттернов
/// </summary>
public class AnalyzePatternsHandler : IRequestHandler<AnalyzePatternsCommand, InsightResponse>
{
    private readonly IActivityRepository _activityRepo;
    private readonly IInsightRepository _insightRepo;
    private readonly IAIService _aiService;

    public AnalyzePatternsHandler(
        IActivityRepository activityRepo,
        IInsightRepository insightRepo,
        IAIService aiService)
    {
        _activityRepo = activityRepo;
        _insightRepo = insightRepo;
        _aiService = aiService;
    }

    public async Task<InsightResponse> Handle(AnalyzePatternsCommand command, CancellationToken cancellationToken)
    {
        var activities = (await _activityRepo.GetByUserAsync(command.UserId)).OrderByDescending(a => a.Date).Take(14).ToList();
        var content = await _aiService.AnalyzePatternAsync(activities);

        var insight = new Insight
        {
            UserId = command.UserId,
            Content = content,
            Date = DateTime.UtcNow,
            AnalysisType = AnalysisType.Patterns
        };

        var saved = await _insightRepo.CreateAsync(insight);

        return new InsightResponse
        {
            Id = saved.Id,
            Date = saved.Date,
            Content = saved.Content,
            AnalysisType = saved.AnalysisType
        };
    }
}