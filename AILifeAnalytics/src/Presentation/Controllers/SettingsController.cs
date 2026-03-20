using AILifeAnalytics.Application.DTOs;
using AILifeAnalytics.Domain.Entities;
using AILifeAnalytics.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AILifeAnalytics.Controllers
{
    // ── DTOs ────────────────────────────────────────────────────────────────

    public class ProviderInfo
    {
        public string Name { get; set; } = string.Empty;
        public bool HasKey { get; set; }
        public bool IsActive { get; set; }
    }

    public class SaveSettingsRequest
    {
        public string ActiveProvider { get; set; } = "OpenAI";
        public Dictionary<string, string> ApiKeys { get; set; } = new();
        public ProxySettingsDto Proxy { get; set; } = new();
    }

    public class ProxySettingsDto
    {
        public bool Enabled { get; set; } = false;
        public string Host { get; set; } = "";
        public int Port { get; set; } = 1080;
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class SettingsResponse
    {
        public IEnumerable<ProviderInfo> Providers { get; set; } = [];
        public ProxySettingsDto Proxy { get; set; } = new();
    }

    // ── Controller ───────────────────────────────────────────────────────────

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

        /// <summary
        /// >Провайдеры + текущие настройки прокси
        /// </summary>
        [HttpGet("providers")]
        public async Task<ActionResult<ApiResponse<SettingsResponse>>> GetProviders()
        {
            var settings = await _settingsRepo.GetAsync();

            var providers = _providers.Select(p => new ProviderInfo
            {
                Name = p.ProviderName,
                HasKey = settings.ApiKeys.TryGetValue(p.ProviderName, out var key) && !string.IsNullOrWhiteSpace(key),
                IsActive = settings.ActiveProvider.Equals(p.ProviderName, StringComparison.OrdinalIgnoreCase)
            });

            var response = new SettingsResponse
            {
                Providers = providers,
                Proxy = new ProxySettingsDto
                {
                    Enabled = settings.Proxy.Enabled,
                    Host = settings.Proxy.Host,
                    Port = settings.Proxy.Port,
                    Username = settings.Proxy.Username,
                    Password = string.IsNullOrEmpty(settings.Proxy.Password) ? "" : "••••••••"
                }
            };

            return Ok(ApiResponse<SettingsResponse>.Ok(response));
        }

        /// <summary>
        /// Сохранить всё: провайдер, ключи, прокси
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
                if (knownNames.Contains(provider) && !string.IsNullOrWhiteSpace(key))
                    settings.ApiKeys[provider] = key;

            settings.Proxy.Enabled = request.Proxy.Enabled;
            settings.Proxy.Host = request.Proxy.Host;
            settings.Proxy.Port = request.Proxy.Port;
            settings.Proxy.Username = request.Proxy.Username;

            if (!string.IsNullOrWhiteSpace(request.Proxy.Password) && request.Proxy.Password != "••••••••")
            {
                settings.Proxy.Password = request.Proxy.Password;
            }

            await _settingsRepo.SaveAsync(settings);
            return Ok(ApiResponse<bool>.Ok(true));
        }

        /// <summary>Проверить соединение через текущий прокси</summary>
        [HttpPost("test-proxy")]
        public async Task<ActionResult<ApiResponse<string>>> TestProxy()
        {
            var settings = await _settingsRepo.GetAsync();

            if (!settings.Proxy.Enabled || string.IsNullOrWhiteSpace(settings.Proxy.Host))
                return Ok(ApiResponse<string>.Ok("ℹ Прокси отключён или хост не задан."));

            try
            {
                var webProxy = new System.Net.WebProxy(settings.Proxy.ToUrl(), false);

                if (!string.IsNullOrWhiteSpace(settings.Proxy.Username))
                    webProxy.Credentials = new System.Net.NetworkCredential(settings.Proxy.Username, settings.Proxy.Password);

                var handler = new HttpClientHandler
                {
                    Proxy = webProxy,
                    UseProxy = true
                };

                using var client = new HttpClient(handler)
                {
                    Timeout = TimeSpan.FromSeconds(10)
                };

                var resp = await client.GetAsync("https://httpbin.org/ip");
                var body = await resp.Content.ReadAsStringAsync();

                string ip;
                try
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(body);
                    ip = doc.RootElement.GetProperty("origin").GetString() ?? body;
                }
                catch
                {
                    ip = body.Trim();
                }

                return Ok(ApiResponse<string>.Ok($"✓ Прокси работает. Внешний IP: {ip}"));
            }
            catch (TaskCanceledException)
            {
                return Ok(ApiResponse<string>.Ok("✗ Тайм-аут (10 сек). Прокси не отвечает."));
            }
            catch (HttpRequestException ex)
            {
                return Ok(ApiResponse<string>.Ok($"✗ Ошибка соединения: {ex.Message}"));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<string>.Ok($"✗ Неожиданная ошибка: {ex.Message}"));
            }
        }
    }
}