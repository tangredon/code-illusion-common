using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Splitio.Services.Client.Classes;

namespace Illusion.Common.FeatureFlags
{
    public class FeatureFlagsOptions
    {
        public static string SectionName = "FeatureFlags";

        public string ApiKey { get; set; }
    }

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFeatureFlags(this IServiceCollection services, IConfiguration configuration)
        {
            var options = configuration.GetSection(FeatureFlagsOptions.SectionName).Get<FeatureFlagsOptions>();

            var config = new ConfigurationOptions
            {
                StreamingEnabled = true
            };

            var factory = new SplitFactory(options.ApiKey, config);
            var sdk = factory.Client();

            services.AddSingleton<IFeatureFlagProvider, FeatureFlagProvider>(sp =>
            {
                return new FeatureFlagProvider(sp.GetRequiredService<IHttpContextAccessor>(), sdk, sp.GetRequiredService<ILogger<FeatureFlagProvider>>());
            });

            return services;
        }
    }
}