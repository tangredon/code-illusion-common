﻿using System;
using RawRabbit.Common;
using RawRabbit.Enrichers.MessageContext.Context;

namespace Illusion.Common.RawRabbit
{
    public class MessageContext : IMessageContext
    {
        public Guid GlobalRequestId { get; set; }
        public string Source { get; set; }
        public Guid OwnerId { get; set; }
        public MessageTypeInformation TypeInfo { get; set; }
        public RetryInformation RetryInfo { get; set; }
        public string RoutingKey { get; set; }
        public string SpanContext { get; set; }
    }
}
