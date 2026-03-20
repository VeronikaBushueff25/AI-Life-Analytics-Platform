using AILifeAnalytics.Domain.Interfaces;

namespace AILifeAnalytics.Infrastructure.AI
{
    public class HuggingFaceProvider : OpenAICompatibleProvider
    {
        public override string ProviderName => "HuggingFace";
        protected override string BaseUrl => "https://router.huggingface.co/v1/chat/completions";
        protected override string ModelName => "moonshotai/Kimi-K2-Instruct-0905";

        public HuggingFaceProvider(
            IHttpClientFactory factory,
            ISettingsRepository settings,
            ILogger<HuggingFaceProvider> logger)
            : base(factory, settings, logger) { }
    }
}
