using System.Threading;
using System.Threading.Tasks;
using Illusion.Common.Core;
using MediatR;

namespace Illusion.Common.MediatR.Behaviors
{
    internal class TelemetryBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            using var activity = ActivityHelper.Source.StartActivity($"{typeof(TRequest).Name}");
            return await next();
        }
    }
}