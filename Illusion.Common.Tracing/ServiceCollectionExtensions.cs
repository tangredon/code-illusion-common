using System;
using Grpc.Core;
using Jaeger;
using Jaeger.Reporters;
using Jaeger.Samplers;
using Jaeger.Senders.Grpc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTracing;
using OpenTracing.Util;

namespace Illusion.Common.Tracing
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCustomOpenTracing(this IServiceCollection services, TracingOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            services.AddSingleton<ITracer>(serviceProvider =>
            {
                var loggerFactory = serviceProvider.GetService<ILoggerFactory>();

                var sampler = new ConstSampler(sample: true);
                var reporter = new RemoteReporter.Builder()
                    .WithLoggerFactory(loggerFactory)
                    .WithSender(new GrpcSender($"{options.Host}:{options.Port}", ChannelCredentials.Insecure, 0))
                    .Build();

                var tracer = new Tracer.Builder(options.ServiceName)
                    .WithLoggerFactory(loggerFactory)
                    .WithSampler(sampler)
                    .WithReporter(reporter)
                    .Build();


                GlobalTracer.Register(tracer);
                return tracer;
            });

            services.AddOpenTracing();
            services.AddOpenTracingCoreServices(builder =>
            {
                builder.AddLoggerProvider();
                builder.AddEntityFrameworkCore();
                builder.AddGenericDiagnostics();
                builder.AddHttpHandler();
                builder.AddMicrosoftSqlClient();
                builder.AddSystemSqlClient();

                if (AssemblyExists("Microsoft.AspNetCore.Hosting"))
                {
                    builder
                        .AddAspNetCore()
                        .ConfigureAspNetCore(aspNetCoreOptions =>
                        {
                            aspNetCoreOptions.Hosting.OperationNameResolver = (httpContext) => $"HTTP {httpContext.Request.Method} {httpContext.Request.Path}";
                        });
                }
            });

            return services;
        }

        private static bool AssemblyExists(string assemblyName)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                if (assembly.FullName.StartsWith(assemblyName))
                    return true;
            }
            return false;
        }
    }
}
