using AILifeAnalytics.Domain.Interfaces;
using MediatR;

namespace AILifeAnalytics.Application.Commands.CBT;

/// <summary>
/// Команда удаления КПТ-сессии
/// </summary>
public record DeleteCbtCommand(Guid RecordId, Guid UserId) : IRequest<bool>;

/// <summary>
/// Обработчик команды удаления КПТ-сессии.
/// Проверяет владельца перед удалением.
/// </summary>
public class DeleteCbtHandler : IRequestHandler<DeleteCbtCommand, bool>
{
    private readonly ICbtRepository _cbtRepo;

    public DeleteCbtHandler(ICbtRepository cbtRepo) => _cbtRepo = cbtRepo;

    public async Task<bool> Handle(DeleteCbtCommand command, CancellationToken cancellationToken)
    {
        var record = await _cbtRepo.GetByIdAsync(command.RecordId);
        if (record is null || record.UserId != command.UserId) return false;
        return await _cbtRepo.DeleteAsync(command.RecordId);
    }
}