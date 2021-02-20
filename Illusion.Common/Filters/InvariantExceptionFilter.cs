using Illusion.Common.Framework;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Illusion.Common.Filters
{
    internal class ExceptionObject
    {
        public string Message { get; set; }
    }

    public class InvariantExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is InvariantValidationException exception)
            {
                var exceptionObject = new ExceptionObject
                {
                    Message = exception.Message
                };

                var details = new ProblemDetails
                {
                    Instance = context.HttpContext.Request.Path,
                    Status = StatusCodes.Status400BadRequest,
                    Title = "A problem occurred",
                    Detail = exception.Message,
                    Type = "erm ..."
                };

                context.Result = new BadRequestObjectResult(details)
                {
                    ContentTypes = {"application/problem+json"}
                };
            }
        }
    }
}
