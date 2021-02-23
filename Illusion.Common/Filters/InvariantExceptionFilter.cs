using Illusion.Common.Framework;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OpenTracing;

namespace Illusion.Common.Filters
{
    public class InvariantExceptionFilter : ExceptionFilterAttribute
    {
        private readonly ITracer _tracer;

        public InvariantExceptionFilter(ITracer tracer)
        {
            _tracer = tracer;
        }

        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is InvariantValidationException exception)
            {
                var details = new ProblemDetails
                {
                    Instance = context.HttpContext.Request.Path,
                    Status = StatusCodes.Status400BadRequest,
                    Title = "A problem occurred",
                    Detail = exception.Message,
                    Type = "erm ...",
                    Extensions =
                    {
                        {"traceId", _tracer.ActiveSpan.Context.TraceId }
                    }
                };

                context.Result = new BadRequestObjectResult(details)
                {
                    ContentTypes = {"application/problem+json"}
                };
            }
        }
    }
}
