using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Illusion.Common.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Illusion.Common.Authentication
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOpenIdConnectionAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var openIdOptions = configuration.GetSection(OpenIdConnectOptions.SectionName).Get<OpenIdConnectOptions>();

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Add("scp", "scope");
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Add(ClaimTypes.Name, "name");

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.Authority = openIdOptions.Authority;
                    options.Audience = openIdOptions.Audience;
                    options.RequireHttpsMetadata = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                    };
                    options.SaveToken = true;

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = (context) => Task.CompletedTask,
                        OnTokenValidated = (context) =>
                        {
                            if (!(context.Principal is {Identity: ClaimsIdentity identity}))
                            {
                                return Task.CompletedTask;
                            }

                            // Okta base62 to Guid conversion
                            var uid = identity.Claims.First(c => c.Type == "uid").Value;

                            var uuid = Base62Convertor.ConvertFrom(uid);
                            identity.AddClaim(new Claim(identity.NameClaimType, uuid.ToString()));

                            // Map sub claim to email
                            var email = identity.Claims.First(c => c.Type == "sub").Value;
                            identity.AddClaim(new Claim(ClaimTypes.Email, email));

                            return Task.CompletedTask;
                        }
                    };
                });

            return services;
        }
    }
}
