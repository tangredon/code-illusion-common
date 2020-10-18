using System.Collections.Generic;

namespace Illusion.Common.Consul
{
    public class ConsulRegistrationSettings
    {
        public static string SectionName => "Consul:Registration";
        public string Name { get; set; }
        public string Id { get; set; }
        public string[] Tags { get; set; }
        public IDictionary<string, string> Meta { get; set; }
    }
}