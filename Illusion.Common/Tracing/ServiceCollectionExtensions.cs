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
        public static IServiceCollection AddCustomOpenTracing(this IServiceCollection services, string serviceName, string hostname)
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
                senderConfig
                    .WithAgentHost(hostname); // using WithEndpoint will switch from Udp to Http sender

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
                builder.AddAspNetCore();
                builder.AddLoggerProvider();
            });

            return services;
        }
    }
}
