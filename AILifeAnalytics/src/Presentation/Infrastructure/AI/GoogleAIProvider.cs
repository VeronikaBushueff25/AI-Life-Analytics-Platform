using AILifeAnalytics.Domain.Interfaces;

namespace AILifeAnalytics.Infrastructure.AI
{
    public class GoogleAIProvider : OpenAICompatibleProvider
    {
        public override string ProviderName => "GoogleAI";
        protected override string BaseUrl => "https://generativelanguage.googleapis.com/v1beta2/models/text-bison-001:generateText";
        protected override string ModelName => "text-bison-001";

        public GoogleAIProvider(
            IHttpClientFactory factory,
            ISettingsRepository settings,
            ILogger<GoogleAIProvider> logger)
            : base(factory, settings, logger)
        { }
    }
}
