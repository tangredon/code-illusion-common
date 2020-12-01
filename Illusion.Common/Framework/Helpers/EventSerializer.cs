using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Illusion.Common.Framework.Helpers
{
    public class EventSerializer
    {
        private static readonly JsonSerializerOptions Options;

        static EventSerializer()
        {
            Options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            };
        }

        public static string SerializeToString<T>(T @event) => JsonSerializer.Serialize((object)@event, Options);
        public static byte[] SerializeToBytes<T>(T @event) => JsonSerializer.SerializeToUtf8Bytes((object)@event, Options);
        public static T Deserialize<T>(Type eventClrType, in ReadOnlySpan<byte> data) => (T)JsonSerializer.Deserialize(data, eventClrType, Options);
        public static T Deserialize<T>(Type eventClrType, string data) => (T)JsonSerializer.Deserialize(data, eventClrType, Options);
    }
}