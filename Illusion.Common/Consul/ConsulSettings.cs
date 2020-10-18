namespace Illusion.Common.Consul
{
    public class ConsulSettings
    {
        public static string SectionName => "Consul:Agent";

        /// <summary>
        /// e.g. http://localhost:8500
        /// </summary>
        public string Host { get; set; }
    }
}