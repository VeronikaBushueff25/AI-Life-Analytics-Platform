using AILifeAnalytics.Domain.Interfaces;

namespace AILifeAnalytics.Infrastructure.AI;

/// <summary>
/// Фабрика + фасад IAIService
/// </summary>
public class AIProviderFactory : IAIService
{
    private readonly ISettingsRepository _settings;
    private readonly IEnumerable<IAIProvider> _providers;

    public AIProviderFactory(
        ISettingsRepository settings,
        IEnumerable<IAIProvider> providers)
    {
        _settings = settings;
        _providers = providers;
    }

    private async Task<IAIProvider> GetActiveProviderAsync()
    {
        var settings = await _settings.GetAsync();
        var provider = _providers.FirstOrDefault(p => p.ProviderName.Equals(settings.ActiveProvider, StringComparison.OrdinalIgnoreCase));

        return provider ?? _providers.First();
    }

    public async Task<string> GenerateInsightAsync(IEnumerable<Domain.Entities.Activity> activities, Domain.Entities.Metrics metrics)
    {
        var provider = await GetActiveProviderAsync();
        return await provider.GenerateInsightAsync(activities, metrics);
    }

    public async Task<string> AnalyzePatternAsync(IEnumerable<Domain.Entities.Activity> activities)
    {
        var provider = await GetActiveProviderAsync();
        return await provider.AnalyzePatternAsync(activities);
    }
}