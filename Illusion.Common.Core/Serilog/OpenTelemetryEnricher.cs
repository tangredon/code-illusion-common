using System.Diagnostics;
using Serilog.Core;
using Serilog.Events;

namespace Illusion.Common.Core.Serilog
{
    public class OpenTelemetryEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var activity = Activity.Current;
            if (activity == null)
                return;

            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("hostname", "localhost"));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("service", "storage"));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("trace.id", activity.TraceId));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("span.id", activity.SpanId));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("parent.id", activity.ParentId));
        }
    }

    public class NewRelicEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("hostname", "localhost"));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("service.name", "storage"));
        }
    }
}
