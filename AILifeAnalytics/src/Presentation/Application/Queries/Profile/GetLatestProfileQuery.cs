using AILifeAnalytics.Application.DTOs;
using AILifeAnalytics.Application.Services;
using AILifeAnalytics.Domain.Interfaces;
using MediatR;

namespace AILifeAnalytics.Application.Queries.Profile;

/// <summary>
/// Запрос последнего сгенерированного профиля личности пользователя
/// </summary>
public record GetLatestProfileQuery(Guid UserId) : IRequest<ProfileResponse?>;

/// <summary>
/// Обработчик запроса последнего профиля личности
/// </summary>
public class GetLatestProfileHandler : IRequestHandler<GetLatestProfileQuery, ProfileResponse?>
{
    private readonly IPersonalityProfileRepository _profileRepo;

    public GetLatestProfileHandler(IPersonalityProfileRepository profileRepo) => _profileRepo = profileRepo;

    public async Task<ProfileResponse?> Handle(GetLatestProfileQuery query, CancellationToken cancellationToken)
    {
        var profile = await _profileRepo.GetLatestByUserAsync(query.UserId);
        return profile is null ? null : PersonalityProfileService.MapToResponse(profile);
    }
}