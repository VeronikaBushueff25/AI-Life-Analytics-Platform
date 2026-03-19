using AILifeAnalytics.Application.DTOs;
using AILifeAnalytics.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AILifeAnalytics.Controllers
{
    [ApiController]
    [Route("api/settings")]
    [Produces("application/json")]
    public class SettingsController : ControllerBase
    {
        private readonly ISettingsRepository _settingsRepo;
        private static readonly string[] KnownProviders = ["OpenAI", "DeepSeek"];

        public SettingsController(ISettingsRepository settingsRepo)
        {
            _settingsRepo = settingsRepo;
        }

        /// <summary>
        /// Список провайдеров и статус настройки (без возврата ключей)
        /// </summary>
        [HttpGet("providers")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProviderInfo>>>> GetProviders()
        {
            var settings = await _settingsRepo.GetAsync();

            var providers = KnownProviders.Select(name => new ProviderInfo
            {
                Name = name,
                HasKey = settings.ApiKeys.TryGetValue(name, out var key) && !string.IsNullOrWhiteSpace(key),
                IsActive = settings.ActiveProvider.Equals(name, StringComparison.OrdinalIgnoreCase)
            });

            return Ok(ApiResponse<IEnumerable<ProviderInfo>>.Ok(providers));
        }

        /// <summary>
        /// Сохранить активный провайдер и/или обновить ключи
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<bool>>> Save([FromBody] SaveSettingsRequest request)
        {
            if (!KnownProviders.Contains(request.ActiveProvider))
                return BadRequest(ApiResponse<bool>.Fail($"Неизвестный провайдер: {request.ActiveProvider}"));

            var settings = await _settingsRepo.GetAsync();
            settings.ActiveProvider = request.ActiveProvider;

            foreach (var (provider, key) in request.ApiKeys)
            {
                if (!string.IsNullOrWhiteSpace(key))
                    settings.ApiKeys[provider] = key;
            }

            await _settingsRepo.SaveAsync(settings);
            return Ok(ApiResponse<bool>.Ok(true));
        }
    }
}
