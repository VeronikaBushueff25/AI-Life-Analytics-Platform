using AILifeAnalytics.Application.DTOs;
using MediatR;

namespace AILifeAnalytics.Application.Commands.Cbt;

/// <summary>
/// Команда завершения КПТ-сессии (шаг 3 из 3)
/// </summary>
public sealed record CompleteCbtCommand(Guid RecordId, Guid UserId, CompleteReframingRequest Request) : IRequest<CbtRecordResponse>;