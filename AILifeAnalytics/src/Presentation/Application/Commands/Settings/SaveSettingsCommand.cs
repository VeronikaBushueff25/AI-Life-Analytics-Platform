using AILifeAnalytics.Application.DTOs;
using AILifeAnalytics.Domain.Interfaces;
using MediatR;
using System.Linq;

namespace AILifeAnalytics.Application.Commands.Settings;

/// <summary>
/// Команда сохранения настроек AI-провайдера пользователя
/// </summary>
public record SaveSettingsCommand(SaveSettingsRequest Request) : IRequest<bool>;

/// <summary>
/// Обработчик сохранения настроек AI-провайдера
/// </summary>
public class SaveSettingsHandler : IRequestHandler<SaveSettingsCommand, bool>
{
    private readonly ISettingsRepository _settingsRepo;
    private readonly IEnumerable<IAIProvider> _providers;

    public SaveSettingsHandler(ISettingsRepository settingsRepo, IEnumerable<IAIProvider> providers)
    {
        _settingsRepo = settingsRepo;
        _providers = providers;
    }

    public async Task<bool> Handle(SaveSettingsCommand command, CancellationToken cancellationToken)
    {
        var knownNames = _providers.Select(p => p.ProviderName).ToHashSet();
        var settings = await _settingsRepo.GetAsync();
        settings.ActiveProvider = command.Request.ActiveProvider;

        foreach (var (provider, key) in command.Request.ApiKeys)
            if (knownNames.Contains(provider) && !string.IsNullOrWhiteSpace(key))
                settings.ApiKeys[provider] = key;

        var proxy = command.Request.Proxy;
        settings.Proxy.Enabled = proxy.Enabled;
        settings.Proxy.Host = proxy.Host;
        settings.Proxy.Port = proxy.Port;
        settings.Proxy.Username = proxy.Username;

        if (!string.IsNullOrWhiteSpace(proxy.Password) && proxy.Password != "••••••••")
            settings.Proxy.Password = proxy.Password;

        await _settingsRepo.SaveAsync(settings);
        return true;
    }
}