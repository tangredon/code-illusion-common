using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OpenTracing;

namespace Illusion.Common.Middleware
{
    public class TracedAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ITracer _tracer;

        public TracedAuthenticationMiddleware(RequestDelegate next, IAuthenticationSchemeProvider schemes, ITracer tracer)
        {
            _next = next;
            Schemes = schemes;
            _tracer = tracer;
        }

        public IAuthenticationSchemeProvider Schemes { get; set; }

        public async Task Invoke(HttpContext context)
        {
            using (var scope = _tracer.BuildSpan("Authentication").StartActive())
            {
                context.Features.Set<IAuthenticationFeature>(new AuthenticationFeature
                {
                    OriginalPath = context.Request.Path,
                    OriginalPathBase = context.Request.PathBase
                });

                // Give any IAuthenticationRequestHandler schemes a chance to handle the request
                var handlers = context.RequestServices.GetRequiredService<IAuthenticationHandlerProvider>();
                foreach (var scheme in await Schemes.GetRequestHandlerSchemesAsync())
                {
                    if (await handlers.GetHandlerAsync(context, scheme.Name) is IAuthenticationRequestHandler handler && await handler.HandleRequestAsync())
                    {
                        return;
                    }
                }

                var defaultAuthenticate = await Schemes.GetDefaultAuthenticateSchemeAsync();
                if (defaultAuthenticate != null)
                {
                    var result = await context.AuthenticateAsync(defaultAuthenticate.Name);
                    if (result?.Principal != null)
                    {
                        context.User = result.Principal;
                    }
                }
            }

            await _next(context);
        }

    }
}
