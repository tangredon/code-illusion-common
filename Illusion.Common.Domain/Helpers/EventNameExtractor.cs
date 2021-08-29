using System;

namespace Illusion.Common.Domain.Helpers
{
    public static class EventNameExtractor
    {
        public static string GetEventName(Type type)
        {
            // todo: shouldn't the event implement IEvent?

            var fullName = type.Name;

            if (!fullName.EndsWith("Event")) throw new ArgumentException(nameof(type));

            var eventIndex = fullName.IndexOf("Event", StringComparison.InvariantCultureIgnoreCase);

            return fullName.Substring(0, eventIndex);
        }
    }
}