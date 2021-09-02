using System;
using RabbitMQ.Client;
using RawRabbit.Common;
using RawRabbit.Enrichers.MessageContext.Context;

namespace Illusion.Common.RabbitMq
{
    public class IllusionMessageContext : IMessageContext
    {
        public Guid GlobalRequestId { get; set; }
        public string Source { get; set; }
        public Guid OwnerId { get; set; }
        public RetryInformation RetryInfo { get; set; }
        public string RoutingKey { get; set; }
        public string TraceId { get; set; }
        public string SpanId { get; set; }
    }

    public class ConsumerIllusionMessageContext : IllusionMessageContext
    {
        public IBasicProperties BasicProperties { get; set; }
    }
}
