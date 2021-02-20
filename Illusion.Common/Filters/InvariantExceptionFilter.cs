using Illusion.Common.Framework;
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

                context.Result = new BadRequestObjectResult(exceptionObject);
            }
        }
    }
}
