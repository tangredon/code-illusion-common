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
                samplerConfig.WithSamplingEndpoint($"http://{hostname}:5778");
                config.WithSampler(samplerConfig);

                var reporterConfig = new Configuration.ReporterConfiguration(loggerFactory);
                var senderConfig = new Configuration.SenderConfiguration(loggerFactory);
                senderConfig.WithAgentHost(hostname);
                senderConfig.WithSenderFactory(ThriftSenderFactory.Name);
                senderConfig.WithSenderResolver(senderResolver);
                reporterConfig.WithSender(senderConfig);
                config.WithReporter(reporterConfig);

                var tracer = config.GetTracerBuilder()
                    .WithSampler(new ConstSampler(true))
                    .Build();

                GlobalTracer.Register(tracer);
                return tracer;
            });

            services.AddOpenTracing(builder =>
            {
                builder.AddCoreFx();
                builder.AddAspNetCore();
                builder.AddLoggerProvider();
            });

            return services;
        }
    }
}
