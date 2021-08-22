namespace Illusion.Common.Telemetry
{
    public class NewRelicExporterOptions
    {
        public bool Enabled { get; set; } = false;
        public string Endpoint { get; set; }
        public string ApiKey { get; set; }
    }
}