using AILifeAnalytics.Domain.Interfaces;

namespace AILifeAnalytics.Infrastructure.AI
{
    public class HuggingFaceProvider : OpenAICompatibleProvider
    {
        public override string ProviderName => "HuggingFace";
        protected override string BaseUrl => "https://api-inference.huggingface.co/models/your-model";
        protected override string ModelName => "gpt2-medium";

        public HuggingFaceProvider(
            IHttpClientFactory factory,
            ISettingsRepository settings,
            ILogger<HuggingFaceProvider> logger)
            : base(factory, settings, logger) { }
    }
}
