using System;
using RawRabbit.Enrichers.MessageContext.Context;

namespace Illusion.Common.RabbitMq
{
    public class IllusionMessageContext : IMessageContext
    {
        public Guid GlobalRequestId { get; set; }
        public string SourceTraceId { get; set; }
        public string TraceId { get; set; }
        public string SpanId { get; set; }
    }
}
