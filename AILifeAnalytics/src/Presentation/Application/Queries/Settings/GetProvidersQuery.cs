using AILifeAnalytics.Application.DTOs;
using AILifeAnalytics.Domain.Interfaces;
using MediatR;

namespace AILifeAnalytics.Application.Queries.Settings;

/// <summary>
/// Запрос списка AI-провайдеров с их статусом
/// </summary>
public record GetProvidersQuery : IRequest<SettingsResponse>;

/// <summary>
/// Обработчик запроса списка AI-провайдеров.
/// Список провайдеров берётся из DI — всегда актуален.
/// </summary>
public class GetProvidersHandler : IRequestHandler<GetProvidersQuery, SettingsResponse>
{
    private readonly ISettingsRepository _settingsRepo;
    private readonly IEnumerable<IAIProvider> _providers;

    public GetProvidersHandler(ISettingsRepository settingsRepo, IEnumerable<IAIProvider> providers)
    {
        _settingsRepo = settingsRepo;
        _providers = providers;
    }

    public async Task<SettingsResponse> Handle(GetProvidersQuery query, CancellationToken cancellationToken)
    {
        var settings = await _settingsRepo.GetAsync();

        var providers = _providers.Select(p => new ProviderInfo
        {
            Name = p.ProviderName,
            HasKey = settings.ApiKeys.TryGetValue(p.ProviderName, out var key) && !string.IsNullOrWhiteSpace(key),
            IsActive = settings.ActiveProvider.Equals(p.ProviderName, StringComparison.OrdinalIgnoreCase)
        });

        return new SettingsResponse
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
    }
}