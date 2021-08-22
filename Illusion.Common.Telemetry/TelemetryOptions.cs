namespace Illusion.Common.Telemetry
{
    public class TelemetryOptions
    {
        public static string SectionName = "Telemetry";

        public bool Enabled { get; set; }
        public JaegerExporterExtendedOptions Jaeger { get; set; }
        public NewRelicExporterOptions NewRelic { get; set; }
        public bool EnableConsoleExporter { get; set; } = false;
    }
}