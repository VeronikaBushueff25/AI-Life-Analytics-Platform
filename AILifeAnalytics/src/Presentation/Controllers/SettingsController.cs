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
        private readonly IEnumerable<IAIProvider> _providers;

        public SettingsController(ISettingsRepository settingsRepo, IEnumerable<IAIProvider> providers)
        {
            _settingsRepo = settingsRepo;
            _providers = providers;
        }

        /// <summary>
        /// Список провайдеров берётся из DI — добавил провайдер в Program.cs, 
        /// он автоматически появляется в настройках.
        /// </summary>
        [HttpGet("providers")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProviderInfo>>>> GetProviders()
        {
            var settings = await _settingsRepo.GetAsync();

            var providers = _providers.Select(p => new ProviderInfo
            {
                Name = p.ProviderName,
                HasKey = settings.ApiKeys.TryGetValue(p.ProviderName, out var key) && !string.IsNullOrWhiteSpace(key),
                IsActive = settings.ActiveProvider.Equals(p.ProviderName, StringComparison.OrdinalIgnoreCase)
            });

            return Ok(ApiResponse<IEnumerable<ProviderInfo>>.Ok(providers));
        }

        /// <summary>
        /// Сохранить активный провайдер и/или обновить ключи
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<bool>>> Save([FromBody] SaveSettingsRequest request)
        {
            var knownNames = _providers.Select(p => p.ProviderName).ToHashSet();

            if (!knownNames.Contains(request.ActiveProvider))
                return BadRequest(ApiResponse<bool>.Fail($"Неизвестный провайдер: {request.ActiveProvider}"));

            var settings = await _settingsRepo.GetAsync();
            settings.ActiveProvider = request.ActiveProvider;
            
            foreach (var (provider, key) in request.ApiKeys)
            {
                if (knownNames.Contains(provider) && !string.IsNullOrWhiteSpace(key))
                    settings.ApiKeys[provider] = key;
            }

            await _settingsRepo.SaveAsync(settings);
            return Ok(ApiResponse<bool>.Ok(true));
        }
    }
}