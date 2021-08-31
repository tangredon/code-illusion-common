using System;
using FluentValidation;
using Illusion.Common.MediatR.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Illusion.Common.MediatR.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMediatRCustom(this IServiceCollection services, Type assembly)
        {
            return services.AddMediatRCustom(assembly, true, true);
        }

        public static IServiceCollection AddMediatRCustom(this IServiceCollection services, Type assembly, bool logging, bool validation)
        {
            services.AddMediatR(assembly);
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TelemetryBehavior<,>));

            if (logging)
            {
                services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            }

            if (validation)
            {
                services.AddValidatorsFromAssembly(assembly.Assembly);
                services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            }

            return services;
        }
    }
}
