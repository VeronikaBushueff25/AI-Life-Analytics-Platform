using AILifeAnalytics.Application.DTOs;
using AILifeAnalytics.Application.Services;
using AILifeAnalytics.Domain.Interfaces;
using MediatR;

namespace AILifeAnalytics.Application.Queries.Achievements;

/// <summary>
/// Запрос страницы достижений: разблокированные + заблокированные
/// </summary>
public record GetAchievementsQuery(Guid UserId) : IRequest<AchievementsPageResponse>;

/// <summary>
/// Обработчик страницы достижений
/// </summary>
public class GetAchievementsHandler : IRequestHandler<GetAchievementsQuery, AchievementsPageResponse>
{
    private readonly IAchievementRepository _repo;

    public GetAchievementsHandler(IAchievementRepository repo) => _repo = repo;

    public async Task<AchievementsPageResponse> Handle(GetAchievementsQuery query, CancellationToken cancellationToken)
    {
        var all = (await _repo.GetByUserAsync(query.UserId)).ToList();
        var unseen = all.Count(a => a.IsNew);
        await _repo.MarkAllSeenAsync(query.UserId);

        var unlocked = all.Select(a =>
        {
            var info = AchievementService.GetInfo(a.Type);
            return new AchievementResponse
            {
                Id = a.Id,
                Type = a.Type.ToString(),
                Emoji = info.Emoji,
                Title = info.Title,
                Description = info.Description,
                Context = a.Context,
                UnlockedAt = a.UnlockedAt,
                IsNew = a.IsNew
            };
        }).ToList();

        var unlockedTypes = all.Select(a => a.Type).ToHashSet();
        var allMeta = AchievementService.GetAllMeta();

        var locked = allMeta.Where(kvp => !unlockedTypes.Contains(kvp.Key))
            .Select(kvp => new LockedAchievement
            {
                Type = kvp.Key.ToString(),
                Emoji = kvp.Value.Emoji,
                Title = kvp.Value.Title,
                Description = kvp.Value.Description
            }).ToList();

        var totalCount = allMeta.Count;
        var unlockedCount = unlockedTypes.Count;

        return new AchievementsPageResponse
        {
            Unlocked = unlocked,
            Locked = locked,
            TotalCount = totalCount,
            UnlockedCount = unlockedCount,
            UnseenCount = unseen,
            Progress = totalCount > 0 ? Math.Round((double)unlockedCount / totalCount * 100, 1) : 0
        };
    }
}