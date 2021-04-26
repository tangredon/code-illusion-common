using System;
using Jaeger;
using Jaeger.Samplers;
using Jaeger.Senders;
using Jaeger.Senders.Thrift;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTracing;
using OpenTracing.Util;

namespace Illusion.Common.Tracing
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCustomOpenTracing(this IServiceCollection services, string serviceName, string hostname, bool http = false)
        {
            services.AddSingleton<ITracer>(serviceProvider =>
            {
                var loggerFactory = serviceProvider.GetService<ILoggerFactory>();

                var senderResolver = new SenderResolver(loggerFactory).RegisterSenderFactory<ThriftSenderFactory>();
                Jaeger.Configuration.SenderConfiguration.DefaultSenderResolver = senderResolver;

                var config = new Jaeger.Configuration(serviceName, loggerFactory);

                var samplerConfig = new Configuration.SamplerConfiguration(loggerFactory);
                samplerConfig
                    .WithType("const")
                    .WithParam(1);

                var senderConfig = new Configuration.SenderConfiguration(loggerFactory);
                if (http == false)
                {
                    senderConfig
                        .WithAgentHost(hostname)
                        .WithAgentPort(6831);
                }
                else
                {
                    senderConfig
                        .WithEndpoint($"http://{hostname}:14268");
                }

                var reporterConfig = new Configuration.ReporterConfiguration(loggerFactory);
                
                reporterConfig
                    .WithSender(senderConfig);

                config
                    .WithReporter(reporterConfig)
                    .WithSampler(samplerConfig);

                var tracer = config.GetTracerBuilder()
                    .Build();

                GlobalTracer.Register(tracer);
                return tracer;
            });

            services.AddOpenTracing(builder =>
            {
                builder.AddHttpHandler();
                builder.AddAspNetCore();
                builder.AddLoggerProvider();
            });

            return services;
        }
    }
}
