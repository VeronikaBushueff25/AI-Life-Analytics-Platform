using AILifeAnalytics.Application.DTOs;
using AILifeAnalytics.Application.Services;
using MediatR;

namespace AILifeAnalytics.Application.Commands.CBT;

/// <summary>
/// Команда завершения переосмысления (шаг 2)
/// </summary>
public record CompleteReframingCommand(Guid RecordId, Guid UserId, CompleteReframingRequest Request) : IRequest<CbtRecordResponse>;

/// <summary>
/// Обработчик завершения переосмысления
/// </summary>
public class CompleteReframingHandler : IRequestHandler<CompleteReframingCommand, CbtRecordResponse>
{
    private readonly CbtService _cbtService;
    private readonly AchievementService _achievementService;
    public CompleteReframingHandler(CbtService cbtService, AchievementService achievementService)
    {
        _cbtService = cbtService;
        _achievementService = achievementService;
    }
        

    public async Task<CbtRecordResponse> Handle(CompleteReframingCommand command, CancellationToken cancellationToken)
    {
        var record = await _cbtService.CompleteReframingAsync(command.RecordId, command.UserId, command.Request);
        await _achievementService.CheckAfterCbtAsync(command.UserId, record.IsCompleted, record.EmotionShift);

        return CbtService.MapToResponse(record);
    }
}