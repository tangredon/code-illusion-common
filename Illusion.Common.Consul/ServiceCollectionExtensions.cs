﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Illusion.Common.Consul
{
    public static class ServiceCollectionExtensions
    {
        public static IHttpClientBuilder AddHttpClientWithConsulDiscovery(this IServiceCollection services, string serviceName, PathString basePath)
        {
            return services
                .AddHttpClient(serviceName, (serviceProvider, client) =>
                {
                    var context = serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;

                    if (context.Request.Headers.ContainsKey("Authorization"))
                    {
                        client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(context.Request.Headers["Authorization"].ToString());
                    }

                    var consul = serviceProvider.GetRequiredService<IConsulClient>();

                    // todo: this is a terrible approach; create a hosted service for this
                    var result = consul.Agent.Services().Result.Response;

                    var agentServices = result
                        .Where((pair => pair.Value.Service == serviceName))
                        .Select(pair => pair.Value)
                        .ToList();

                    if (agentServices.Count == 0)
                    {
                        // todo: non retryable
                        throw new Exception($"Service {serviceName} could not be found using service discovery.");
                    }

                    var graphQLService = agentServices.First();
                    // todo: maybe add a parameter for the protocol (http or https)
                    client.BaseAddress = new Uri($"http://{graphQLService.Address}:{graphQLService.Port}{basePath}");
                    //client.Timeout = TimeSpan.FromSeconds(5);
                });
        }

        public static IServiceCollection AddConsul(this IServiceCollection services)
        {
            using var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            var settings = configuration.GetSection(ConsulSettings.SectionName).Get<ConsulSettings>();

            services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig =>
            {
                consulConfig.Address = new Uri(settings.Host);
            }));

            return services;
        }

        public static IServiceCollection AddConsul(this IServiceCollection services, IConfiguration configuration)
        {
            var settings = configuration.GetSection(ConsulSettings.SectionName).Get<ConsulSettings>();

            services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig =>
            {
                consulConfig.Address = new Uri(settings.Host);
            }));

            return services;
        }

        public static IServiceCollection AddConsul(this IServiceCollection services, ConsulSettings settings)
        {
            services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig =>
            {
                consulConfig.Address = new Uri(settings.Host);
            }));

            return services;
        }

        public static IApplicationBuilder UseConsul(this IApplicationBuilder app, bool healthCheck = false)
        {
            //var logger = app.ApplicationServices.GetRequiredService<ILoggerFactory>().CreateLogger("Illusion.Common.Consul");
            var logger = app.ApplicationServices.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(ServiceCollectionExtensions));

            var configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();

            var settings = configuration.GetSection(ConsulRegistrationSettings.SectionName).Get<ConsulRegistrationSettings>();

            settings.Id = $"{settings.Name}:{configuration.GetValue<string>("COMPUTERNAME")}";
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

            var checks = new List<AgentServiceCheck>();

            if (healthCheck)
            {
                var healthServiceCheck = new AgentServiceCheck
                {
                    HTTP = $"http://{uri.Host}:{uri.Port}/health",
                    TLSSkipVerify = true,
                    Interval = TimeSpan.FromSeconds(10),
                    Timeout = TimeSpan.FromSeconds(1),
                    DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1),
                };

                checks.Add(healthServiceCheck);
            }

            var registration = new AgentServiceRegistration()
            {
                Meta = settings.Meta,
                Tags = settings.Tags,
                ID = settings.Id,
                Name = settings.Name,
                Address = uri.Host,
                Port = uri.Port,
                Checks = checks.ToArray()
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
