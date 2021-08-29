using Microsoft.Extensions.Logging;

namespace Illusion.Common.FeatureFlags
{
    internal class DevelopmentFeatureFlagProvider : IFeatureFlagProvider
    {
        public DevelopmentFeatureFlagProvider(ILogger<DevelopmentFeatureFlagProvider> logger)
        {
            logger.LogDebug("[DEV] Feature Flag Provider set to Dev Implementation. All features set to FALSE.");
        }

        public bool GetFeatureFlag(string feature) => false;
    }
}