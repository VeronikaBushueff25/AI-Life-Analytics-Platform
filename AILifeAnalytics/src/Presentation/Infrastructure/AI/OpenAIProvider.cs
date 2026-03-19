using AILifeAnalytics.Domain.Interfaces;

namespace AILifeAnalytics.Infrastructure.AI
{
    public class OpenAIProvider : OpenAICompatibleProvider
    {
        public override string ProviderName => "OpenAI";
        protected override string BaseUrl => "https://api.openai.com/v1/chat/completions";
        protected override string ModelName => "gpt-4o-mini";

        public OpenAIProvider(
            IHttpClientFactory factory,
            ISettingsRepository settings,
            ILogger<OpenAIProvider> logger)
            : base(factory, settings, logger) { }
    }
}
