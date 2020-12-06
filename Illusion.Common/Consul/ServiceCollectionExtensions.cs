﻿using System;
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
            var configuration = services.BuildServiceProvider().GetService<IConfiguration>();

            var settings = new ConsulSettings();
            configuration.GetSection(ConsulSettings.SectionName).Bind(settings);

            services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig =>
            {
                var address = settings.Host;
                consulConfig.Address = new Uri(address);
            }));

            services.Configure<ConsulRegistrationSettings>(configuration.GetSection(ConsulRegistrationSettings.SectionName));

            return services;
        }

        //private static ConsulRegistrationSettings BuildConsulSettings(IWebHostEnvironment env, IOptions<ConsulRegistrationSettings> consulOptions)
        //{
        //    var consul = consulOptions.Value;
        //    consul.Id = $"{consul.Name}:{Guid.NewGuid()}";
        //    consul.Meta = new Dictionary<string, string>()
        //    {
        //        {"Version", VersionInfo.Version}
        //    };

        //    if (env.IsDevelopment())
        //    {
        //        consul.Tags = new[] { "dev" };
        //    }

        //    return consul;
        //}

        public static IApplicationBuilder UseConsul(this IApplicationBuilder app)
        {
            var logger = app.ApplicationServices.GetRequiredService<ILoggerFactory>().CreateLogger("Illusion.Common.Consul");

            var consulSettings = app.ApplicationServices.GetService<IOptions<ConsulRegistrationSettings>>();
            if (consulSettings == null)
            {
                throw new InvalidOperationException("Could not resolve consul settings. Ensure \"AddConsul\" is added to \"ConfigureServices\".");
            }

            var settings = consulSettings.Value;

            settings.Id = $"{settings.Name}:{Guid.NewGuid()}";
            //settings.Meta = new Dictionary<string, string>()
            //{
            //    {"Version", VersionInfo.Version}
            //};

            var consulClient = app.ApplicationServices.GetRequiredService<IConsulClient>();
            
            var lifetime = app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();

            if (!(app.Properties["server.Features"] is FeatureCollection features)) return app;

            var addresses = features.Get<IServerAddressesFeature>();
            var address = addresses.Addresses.First();

            var uri = new Uri(address);
            var registration = new AgentServiceRegistration()
            {
                Meta = settings.Meta,
                Tags = settings.Tags,
                ID = settings.Id,
                Name = settings.Name,
                Address = $"{uri.Host}",
                Port = uri.Port
            };

            logger.LogDebug("Registering {Registration}", registration);
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
