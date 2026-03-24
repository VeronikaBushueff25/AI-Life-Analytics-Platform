using System.Text.Json;
using AILifeAnalytics.Application.DTOs;
using AILifeAnalytics.Application.Services;
using AILifeAnalytics.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AILifeAnalytics.Controllers;

[Authorize]
[ApiController]
[Route("api/cbt")]
[Produces("application/json")]
public class CbtController : ControllerBase
{
    private readonly CbtService _cbtService;
    private readonly ICbtRepository _cbtRepo;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public CbtController(CbtService cbtService, ICbtRepository cbtRepo)
    {
        _cbtService = cbtService;
        _cbtRepo = cbtRepo;
    }

    /// <summary>
    /// Создать запись и получить AI-анализ
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<CbtRecordResponse>>> Create([FromBody] CreateCbtRequest request)
    {
        try
        {
            var record = await _cbtService.AnalyzeThoughtAsync(UserId, request);
            return Ok(ApiResponse<CbtRecordResponse>.Ok(MapToResponse(record)));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<CbtRecordResponse>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Завершить переосмысление
    /// </summary>
    [HttpPut("{id:guid}/complete")]
    public async Task<ActionResult<ApiResponse<CbtRecordResponse>>> Complete(
        Guid id, [FromBody] CompleteReframingRequest request)
    {
        try
        {
            var record = await _cbtService.CompleteReframingAsync(id, UserId, request);
            return Ok(ApiResponse<CbtRecordResponse>.Ok(MapToResponse(record)));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<CbtRecordResponse>.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Получить все записи пользователя
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<CbtRecordResponse>>>> GetAll([FromQuery] int count = 20)
    {
        var records = await _cbtRepo.GetByUserAsync(UserId, count);
        return Ok(ApiResponse<IEnumerable<CbtRecordResponse>>.Ok(records.Select(MapToResponse)));
    }

    /// <summary>
    /// Получить одну запись
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CbtRecordResponse>>> GetById(Guid id)
    {
        var record = await _cbtRepo.GetByIdAsync(id);
        if (record is null || record.UserId != UserId)
            return NotFound(ApiResponse<CbtRecordResponse>.Fail("Запись не найдена."));
        return Ok(ApiResponse<CbtRecordResponse>.Ok(MapToResponse(record)));
    }

    /// <summary>
    /// Удалить запись
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        var record = await _cbtRepo.GetByIdAsync(id);
        if (record is null || record.UserId != UserId)
            return NotFound(ApiResponse<bool>.Fail("Запись не найдена."));
        await _cbtRepo.DeleteAsync(id);
        return Ok(ApiResponse<bool>.Ok(true));
    }

    /// <summary>
    /// Статистика: топ искажений, прогресс
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<CbtStatsResponse>>> GetStats()
    {
        var stats = await _cbtService.GetStatsAsync(UserId);
        return Ok(ApiResponse<CbtStatsResponse>.Ok(stats));
    }

    private static CbtRecordResponse MapToResponse(
        AILifeAnalytics.Domain.Entities.CbtRecord r) => new()
        {
            Id = r.Id,
            CreatedAt = r.CreatedAt,
            IsCompleted = r.IsCompleted,
            Situation = r.Situation,
            AutomaticThought = r.AutomaticThought,
            ThoughtBelief = r.ThoughtBelief,
            PrimaryEmotion = r.PrimaryEmotion.ToString(),
            EmotionIntensity = r.EmotionIntensity,
            Behavior = r.Behavior,
            DetectedDistortions = JsonSerializer
            .Deserialize<List<string>>(r.DetectedDistortions) ?? [],
            AiChallenge = r.AiChallenge,
            AiQuestions = JsonSerializer
            .Deserialize<List<string>>(r.AiQuestions) ?? [],
            EvidenceFor = r.EvidenceFor,
            EvidenceAgainst = r.EvidenceAgainst,
            ReframedThought = r.ReframedThought,
            NewThoughtBelief = r.NewThoughtBelief,
            NewEmotionIntensity = r.NewEmotionIntensity,
            Insight = r.Insight,
            AiSummary = r.AiSummary,
            EmotionShift = r.IsCompleted ? r.EmotionIntensity - r.NewEmotionIntensity : 0
        };
}