using OpenTelemetry.Exporter;

namespace Illusion.Common.Telemetry
{
    public class JaegerExporterExtendedOptions : JaegerExporterOptions
    {
        public bool Enabled { get; set; } = false;
    }
}