using System;
using Illusion.Common.Core;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OpenTracing.Util;
using RawRabbit;
using RawRabbit.Configuration;
using RawRabbit.DependencyInjection.ServiceCollection;
using RawRabbit.Enrichers.GlobalExecutionId;
using RawRabbit.Enrichers.MessageContext;
using RawRabbit.Instantiation;
using RawRabbit.Serialization;
using JsonSerializer = RawRabbit.Serialization.JsonSerializer;

namespace Illusion.Common.RabbitMq
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRabbitCustom(this IServiceCollection services, RawRabbitConfiguration configuration)
        {
            services.AddRawRabbit(new RawRabbitOptions
            {
                ClientConfiguration = configuration,
                Plugins = p => p
                    .UseGlobalExecutionId()
                    .UseHttpContext()
                    .UseMessageContext(c => new IllusionMessageContext
                    {
                        GlobalRequestId = Guid.Parse(c.GetGlobalExecutionId()),
                        SpanId = GlobalTracer.Instance.ActiveSpan.Context.ToString()
                    })
                    .UseContextForwarding()
                    .UseRetryLater()
                    //.UsePolly(new PolicyOptions
                    //{
                    //    ConnectionPolicies = new ConnectionPolicies
                    //    {
                    //        Connect = Policy.NoOpAsync(),
                    //        CreateChannel = Policy.NoOpAsync(),
                    //        GetConnection = Policy.NoOpAsync()
                    //    },
                    //    PolicyAction = c => c
                    //        .UsePolicy(Policy.NoOpAsync(), PolicyKeys.QueueDeclare)
                    //        .UsePolicy(Policy.NoOpAsync(), PolicyKeys.ExchangeDeclare)
                    //        .UsePolicy(Policy.NoOpAsync())
                    //})
            });

            services.Remove(ServiceDescriptor.Singleton<ISerializer, JsonSerializer>());

            var jsonSerializer = new Newtonsoft.Json.JsonSerializer
            {
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                Formatting = Formatting.None,
                CheckAdditionalContent = true,
                ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() },
                ObjectCreationHandling = ObjectCreationHandling.Auto,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                NullValueHandling = NullValueHandling.Ignore
            };

            // todo: improve this
            jsonSerializer.Error += (sender, args) =>
            {
                switch (args.CurrentObject)
                {
                    case MessageTypeInformation typeInfo:
                        typeInfo.Error = args.ErrorContext.Error.Message;
                        break;
                }

                Console.WriteLine(args.ErrorContext.Error.GetType().Name);
                args.ErrorContext.Handled = true;
            };

            services.AddSingleton<ISerializer>(resolver => new JsonSerializer(jsonSerializer));

            return services;
        }
    }
}
