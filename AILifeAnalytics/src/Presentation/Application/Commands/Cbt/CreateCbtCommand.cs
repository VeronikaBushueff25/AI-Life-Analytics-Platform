using AILifeAnalytics.Application.DTOs;
using AILifeAnalytics.Application.Services;
using MediatR;

namespace AILifeAnalytics.Application.Commands.Cbt;

/// <summary>
/// Команда создания КПТ-записи (шаг 1 из 3)
/// </summary>
public sealed record CreateCbtCommand(Guid UserId, CreateCbtRequest Request) : IRequest<CbtRecordResponse>;

/// <summary>
/// Обработчик создания КПТ-сессии
/// </summary>
public class CreateCbtHandler : IRequestHandler<CreateCbtCommand, CbtRecordResponse>
{
    private readonly CbtService _cbtService;

    public CreateCbtHandler(CbtService cbtService) => _cbtService = cbtService;

    public async Task<CbtRecordResponse> Handle(CreateCbtCommand command, CancellationToken cancellationToken)
    {
        var record = await _cbtService.AnalyzeThoughtAsync(command.UserId, command.Request);

        return CbtService.MapToResponse(record);
    }
}