using System;
using System.Linq;
using Illusion.Common.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Illusion.Common.Telemetry.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTelemetry(this IServiceCollection services, IConfiguration configuration)
        {
            var serviceOptions = configuration.GetSection(ServiceOptions.SectionName).Get<ServiceOptions>();
            var telemetryOptions = configuration.GetSection(TelemetryOptions.SectionName).Get<TelemetryOptions>();

            services.AddOpenTelemetryTracing(builder =>
            {
                builder.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceOptions.Name, serviceOptions.Namespace, serviceOptions.Version));
                builder.AddAspNetCoreInstrumentation(options =>
                {
                    options.Enrich = (activity, eventName, rawObject) =>
                    {
                        if (eventName == "OnStopActivity")
                        {
                            var v = activity.Tags.FirstOrDefault(t => t.Key == "http.method").Value;
                            activity.DisplayName = $"HTTP {v} {activity.DisplayName}";
                        }
                    };
                });
                builder.AddHttpClientInstrumentation(options =>
                {
                    options.Enrich = (activity, eventName, rawObject) => { };

                    options.Filter = (message) =>
                    {
                        var requestUri = message.RequestUri;
                        if (requestUri?.AbsoluteUri == "https://log-api.eu.newrelic.com/log/v1")
                        {
                            return false;
                        }

                        return true;
                    };
                });
                builder.AddGrpcClientInstrumentation();

                if (telemetryOptions.EnableConsoleExporter)
                {
                    builder.AddConsoleExporter();
                }

                if (telemetryOptions.Jaeger.Enabled)
                {
                    builder.AddJaegerExporter(options =>
                    {
                        options.AgentHost = telemetryOptions.Jaeger.AgentHost;
                        options.AgentPort = telemetryOptions.Jaeger.AgentPort;
                    });
                }

                if (telemetryOptions.NewRelic.Enabled)
                {
                    builder.AddNewRelicExporter(options =>
                    {
                        options.Endpoint = new Uri(telemetryOptions.NewRelic.Endpoint);
                        options.ApiKey = telemetryOptions.NewRelic.ApiKey;
                    });
                }
            });

            return services;
        }
    }
}