using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Illusion.Common.Domain.Events;

namespace Illusion.Common.Domain.Helpers
{
    public class EventTypeMapper
    {
        public static Dictionary<string, Type> CreateMap(Type type)
        {
            var eventType = typeof(IEvent);
            var assembly = Assembly.GetAssembly(type);

            if (assembly == null)
            {
                throw new InvalidOperationException($"Assembly for {nameof(type)} could not be found.");
            }
            var derivedTypes = assembly.GetTypes().Where(t => t != eventType && eventType.IsAssignableFrom(t)).ToList();

            var dictionary = new Dictionary<string, Type>();

            foreach (var derivedType in derivedTypes)
            {
                var eventName = EventNameExtractor.GetEventName(derivedType);
                dictionary.Add(eventName, derivedType);
            }

            return dictionary;
        }
    }
}
