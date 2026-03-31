using AILifeAnalytics.Application.DTOs;
using AILifeAnalytics.Application.Services;
using AILifeAnalytics.Domain.Interfaces;
using MediatR;

namespace AILifeAnalytics.Application.Queries.Profile;

/// <summary>
/// Запрос истории профилей личности
/// </summary>
public record GetProfileHistoryQuery(Guid UserId, int Count = 5) : IRequest<IEnumerable<ProfileResponse>>;

/// <summary>
/// Обработчик запроса истории профилей личности
/// </summary>
public class GetProfileHistoryHandler : IRequestHandler<GetProfileHistoryQuery, IEnumerable<ProfileResponse>>
{
    private readonly IPersonalityProfileRepository _profileRepo;

    public GetProfileHistoryHandler(IPersonalityProfileRepository profileRepo) => _profileRepo = profileRepo;

    public async Task<IEnumerable<ProfileResponse>> Handle(GetProfileHistoryQuery query, CancellationToken cancellationToken)
    {
        var profiles = await _profileRepo.GetHistoryByUserAsync(query.UserId, query.Count);
        return profiles.Select(PersonalityProfileService.MapToResponse);
    }
}
