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

                Jaeger.Configuration.SenderConfiguration.DefaultSenderResolver = new SenderResolver(loggerFactory)
                    .RegisterSenderFactory<ThriftSenderFactory>();

                var config = new Jaeger.Configuration(serviceName, loggerFactory);

                config.SamplerConfig.WithSamplingEndpoint($"{hostname}:5778");
                config.ReporterConfig.SenderConfig.WithEndpoint($"{hostname}:6831");

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
