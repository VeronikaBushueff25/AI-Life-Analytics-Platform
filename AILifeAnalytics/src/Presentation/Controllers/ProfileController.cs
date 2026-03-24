using AILifeAnalytics.Application.DTOs;
using AILifeAnalytics.Application.Services;
using AILifeAnalytics.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace AILifeAnalytics.Controllers;

[Authorize]
[ApiController]
[Route("api/profile")]
[Produces("application/json")]
public class ProfileController : ControllerBase
{
    private readonly PersonalityProfileService _profileService;
    private readonly IPersonalityProfileRepository _profileRepo;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public ProfileController(PersonalityProfileService profileService, IPersonalityProfileRepository profileRepo)
    {
        _profileService = profileService;
        _profileRepo = profileRepo;
    }

    /// <summary>Сгенерировать новый профиль личности</summary>
    [HttpPost("generate")]
    public async Task<ActionResult<ApiResponse<ProfileResponse>>> Generate()
    {
        try
        {
            var profile = await _profileService.GenerateAsync(UserId);
            return Ok(ApiResponse<ProfileResponse>.Ok(MapToResponse(profile)));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<ProfileResponse>.Fail(ex.Message));
        }
    }

    /// <summary>Получить последний профиль</summary>
    [HttpGet("latest")]
    public async Task<ActionResult<ApiResponse<ProfileResponse?>>> GetLatest()
    {
        var profile = await _profileRepo.GetLatestByUserAsync(UserId);
        if (profile is null)
            return Ok(ApiResponse<ProfileResponse?>.Ok(null));
        return Ok(ApiResponse<ProfileResponse?>.Ok(MapToResponse(profile)));
    }

    /// <summary>История профилей — смотреть как менялся со временем</summary>
    [HttpGet("history")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProfileResponse>>>> GetHistory(
        [FromQuery] int count = 5)
    {
        var profiles = await _profileRepo.GetHistoryByUserAsync(UserId, count);
        return Ok(ApiResponse<IEnumerable<ProfileResponse>>.Ok(
            profiles.Select(MapToResponse)));
    }

    private static ProfileResponse MapToResponse(AILifeAnalytics.Domain.Entities.PersonalityProfile p) => new()
        {
            Id = p.Id,
            GeneratedAt = p.GeneratedAt,
            PeriodFrom = p.PeriodFrom,
            PeriodTo = p.PeriodTo,
            DaysAnalyzed = p.DaysAnalyzed,
            ArchetypeName = p.ArchetypeName,
            ArchetypeEmoji = p.ArchetypeEmoji,
            ArchetypeDescription = p.ArchetypeDescription,
            PeakPerformancePattern = p.PeakPerformancePattern,
            EnergyPattern = p.EnergyPattern,
            StressPattern = p.StressPattern,
            Superpowers = JsonSerializer
            .Deserialize<List<string>>(p.Superpowers) ?? [],
            Vulnerabilities = JsonSerializer
            .Deserialize<List<string>>(p.Vulnerabilities) ?? [],
            Recommendations = JsonSerializer
            .Deserialize<List<string>>(p.Recommendations) ?? [],
            OptimalSleepHours = p.OptimalSleepHours,
            OptimalWorkHours = p.OptimalWorkHours,
            MostProductiveDayOfWeek = p.MostProductiveDayOfWeek,
            CorrelationSleepFocus = p.CorrelationSleepFocus,
            CorrelationStressMood = p.CorrelationStressMood,
            ForecastText = p.ForecastText,
            ForecastRisk = p.ForecastRisk,
            FullAnalysis = p.FullAnalysis
        };
}