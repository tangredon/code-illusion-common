namespace Illusion.Common.Consul
{
    public class ConsulRegistrationSettings
    {
        public static string SectionName => "consul:registration";
        public string Name { get; set; }
        public string Id { get; set; }
    }
}