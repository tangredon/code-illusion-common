using System;
using System.Linq;
using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Illusion.Common.Consul
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddConsul(this IServiceCollection services)
        {
            using var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetService<IConfiguration>();

            var settings = configuration.GetSection(ConsulSettings.SectionName).Get<ConsulSettings>();

            services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig =>
            {
                consulConfig.Address = new Uri(settings.Host);
            }));

            services.Configure<ConsulSettings>(configuration.GetSection(ConsulSettings.SectionName));

            return services;
        }

        public static IApplicationBuilder UseConsul(this IApplicationBuilder app)
        {
            var logger = app.ApplicationServices.GetRequiredService<ILoggerFactory>().CreateLogger("Illusion.Common.Consul");

            var configuration = app.ApplicationServices.GetService<IConfiguration>();

            var settings = configuration.GetSection(ConsulRegistrationSettings.SectionName).Get<ConsulRegistrationSettings>();

            settings.Id = $"{settings.Name}:{Guid.NewGuid()}";
            //settings.Meta = new Dictionary<string, string>()
            //{
            //    {"Version", VersionInfo.Version}
            //};

            var consulClient = app.ApplicationServices.GetRequiredService<IConsulClient>();
            var lifetime = app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();

            if (!(app.Properties["server.Features"] is FeatureCollection features)) return app;

            var addresses = features.Get<IServerAddressesFeature>();
            var address = addresses.Addresses.FirstOrDefault();

            if (!Uri.TryCreate(address, UriKind.RelativeOrAbsolute, out var uri))
            {
                // fallback
                throw new Exception($"Invalid registration address {address}");
            }

            var registration = new AgentServiceRegistration()
            {
                Meta = settings.Meta,
                Tags = settings.Tags,
                ID = settings.Id,
                Name = settings.Name,
                Address = uri.Host,
                Port = uri.Port
            };

            logger.LogDebug("Register {@Registration}", registration);
            consulClient.Agent.ServiceDeregister(registration.ID).ConfigureAwait(true);
            consulClient.Agent.ServiceRegister(registration).ConfigureAwait(true);

            lifetime.ApplicationStopping.Register(() =>
            {
                logger.LogDebug($"Deregister {registration.ID} from Consul");
                consulClient.Agent.ServiceDeregister(registration.ID).ConfigureAwait(true);
            });

            return app;
        }
    }
}
