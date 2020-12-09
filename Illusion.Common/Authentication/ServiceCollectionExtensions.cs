using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Base62;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Illusion.Common.Authentication
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOpenIdConnectionAuthentication(this IServiceCollection services, OpenIdConnectSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.Authority = settings.Authority;
                    options.Audience = settings.Audience;
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                    };
                    options.SaveToken = true;

                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = (context) =>
                        {
                            var identity = context.Principal?.Identity as ClaimsIdentity;
                            if (identity == null)
                            {
                                return Task.CompletedTask;
                            }

                            var uid = identity.Claims.First(c => c.Type == "uid");
                            var uidBytes = uid.Value.FromBase62();
                            uidBytes = uidBytes.Reverse().ToArray();

                            var uuid = new Guid(uidBytes);
                            identity.AddClaim(new Claim(identity.NameClaimType, uuid.ToString()));

                            return Task.CompletedTask;
                        }
                    };
                });

            return services;
        }
    }
}
