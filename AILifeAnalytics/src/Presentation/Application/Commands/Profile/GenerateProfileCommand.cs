using AILifeAnalytics.Application.DTOs;
using AILifeAnalytics.Application.Services;
using MediatR;

namespace AILifeAnalytics.Application.Commands.Profile;

/// <summary>
/// Команда генерации AI-профиля личности
/// </summary>
public record GenerateProfileCommand(Guid UserId) : IRequest<ProfileResponse>;

/// <summary>
/// Обработчик генерации AI-профиля личности
/// </summary>
public class GenerateProfileHandler : IRequestHandler<GenerateProfileCommand, ProfileResponse>
{
    private readonly PersonalityProfileService _profileService;

    public GenerateProfileHandler(PersonalityProfileService profileService) => _profileService = profileService;

    public async Task<ProfileResponse> Handle(GenerateProfileCommand command, CancellationToken cancellationToken)
    {
        var profile = await _profileService.GenerateAsync(command.UserId);
        return PersonalityProfileService.MapToResponse(profile);
    }
}