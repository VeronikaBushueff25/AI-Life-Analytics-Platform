using AILifeAnalytics.Application.DTOs;
using AILifeAnalytics.Domain.Entities;
using AILifeAnalytics.Domain.Enums;
using AILifeAnalytics.Domain.Interfaces;
using MediatR;

namespace AILifeAnalytics.Application.Commands.AI;

/// <summary>
/// Команда генерации AI-инсайта для пользователя
/// </summary>
public sealed record GenerateInsightCommand(Guid UserId) : IRequest<InsightResponse>;

/// <summary>
/// Обработчик команды генерации AI-инсайта
/// </summary>
public class GenerateInsightHandler : IRequestHandler<GenerateInsightCommand, InsightResponse>
{
    private readonly IActivityRepository _activityRepo;
    private readonly IInsightRepository _insightRepo;
    private readonly IMetricsService _metricsService;
    private readonly IAIService _aiService;

    public GenerateInsightHandler(
        IActivityRepository activityRepo,
        IInsightRepository insightRepo,
        IMetricsService metricsService,
        IAIService aiService)
    {
        _activityRepo = activityRepo;
        _insightRepo = insightRepo;
        _metricsService = metricsService;
        _aiService = aiService;
    }

    public async Task<InsightResponse> Handle(GenerateInsightCommand command, CancellationToken cancellationToken)
    {
        var activities = (await _activityRepo.GetByUserAsync(command.UserId)).OrderByDescending(a => a.Date).Take(14).ToList();

        if (!activities.Any())
            throw new InvalidOperationException("No data available for analysis.");

        var metrics = await _metricsService.CalculateAsync(activities);
        var content = await _aiService.GenerateInsightAsync(activities, metrics);

        var insight = new Insight
        {
            UserId = command.UserId,
            Content = content,
            Date = DateTime.UtcNow,
            AnalysisType = AnalysisType.General,
            ProductivityScore = metrics.ProductivityScore,
            BurnoutRisk = metrics.BurnoutRisk
        };

        var saved = await _insightRepo.CreateAsync(insight);

        return new InsightResponse
        {
            Id = saved.Id,
            Date = saved.Date,
            Content = saved.Content,
            AnalysisType = saved.AnalysisType,
            ProductivityScore = saved.ProductivityScore,
            BurnoutRisk = saved.BurnoutRisk
        };
    }
}