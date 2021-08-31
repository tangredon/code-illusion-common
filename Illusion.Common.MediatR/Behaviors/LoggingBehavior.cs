using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Illusion.Common.MediatR.Behaviors
{
    internal class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly ILogger _logger;
        public LoggingBehavior(ILogger<TRequest> logger)
        {
            _logger = logger;
        }
        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            //Request
            //_logger.LogInformation($"Handling {typeof(TRequest).Name}");
            //Type myType = request.GetType();
            //IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties());
            //foreach (PropertyInfo prop in props)
            //{
            //    object propValue = prop.GetValue(request, null);
            //    _logger.LogInformation("{Property} : {@Value}", prop.Name, propValue);
            //}

            var name = typeof(TRequest).Name;

            _logger.LogInformation("Handling Request: {Name} {@Request}", name, request);

            var response = await next();
            //Response
            _logger.LogInformation($"Handled {typeof(TResponse).Name}");
            return response;
        }
    }
}
