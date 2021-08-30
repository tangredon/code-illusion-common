using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Splitio.Services.Client.Classes;

namespace Illusion.Common.FeatureFlags
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFeatureFlags(this IServiceCollection services, IConfiguration configuration)
        {
            var options = configuration.GetSection(FeatureFlagsOptions.SectionName).Get<FeatureFlagsOptions>();

            services.AddSingleton<IFeatureFlagProvider>(sp =>
            {
                try
                {
                    if (options != null)
                    {
                        var config = new ConfigurationOptions
                        {
                            StreamingEnabled = true,
                            LocalhostFilePath = options.LocalFilePath
                        };

                        var factory = new SplitFactory(options.ApiKey, config);
                        var sdk = factory.Client();

                        return new FeatureFlagProvider(sp.GetRequiredService<IHttpContextAccessor>(), sdk, sp.GetRequiredService<ILogger<FeatureFlagProvider>>());
                    }
                }
                catch (Exception e)
                {
                    sp.GetRequiredService<ILoggerFactory>().CreateLogger<FeatureFlagProvider>().LogError(e, "Invalid configuration; fallback to dev implementation");
                }

                return new DevelopmentFeatureFlagProvider(sp.GetRequiredService<ILogger<DevelopmentFeatureFlagProvider>>());
            });

            return services;
        }
    }
}