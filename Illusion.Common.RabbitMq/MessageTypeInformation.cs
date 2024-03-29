﻿using System;

namespace Illusion.Common.RabbitMq
{
    public class MessageTypeInformation
    {
        public MessageTypeInformation()
        {
            // Do not remove this; required during deserialization
        }

        public MessageTypeInformation(Type type)
        {
            ClrType = type;
            Type = type.Name;
            Version = type.Assembly.GetName().Version;
            Assembly = type.Assembly.GetName().Name;
        }

        public Type ClrType { get; init; }
        public string Type { get; init; }
        public Version Version { get; init; }
        public string Assembly { get; init; }
        public string Error { get; set; }
    }
}