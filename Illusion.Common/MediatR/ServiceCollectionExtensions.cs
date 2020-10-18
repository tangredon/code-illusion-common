using Illusion.Common.MediatR.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Illusion.Common.MediatR
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPipelineBehaviors(this IServiceCollection services)
        {
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            return services;
        }
    }
}
