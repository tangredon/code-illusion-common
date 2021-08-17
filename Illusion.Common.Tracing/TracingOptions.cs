using System.ComponentModel.DataAnnotations;

namespace Illusion.Common.Tracing
{
    public class TracingOptions
    {
        public static string SectionName => "Tracing";

        [Required]
        public string Host { get; set; }

        [Required]
        public string Port { get; set; }

        [Required]
        public string ServiceName { get; set; }
    }
}