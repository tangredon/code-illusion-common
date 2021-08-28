using System.Reflection;

namespace Illusion.Common.Core
{
    public static class VersionInfo
    {
        public static string Version => Assembly.GetExecutingAssembly()?.GetName()?.Version?.ToString() ?? "0.0.0";
    }

    public class ServiceOptions
    {
        public static string SectionName = "Service";

        public string Name { get; set; }
        public string Namespace { get; set; }
        public string Version { get; } = VersionInfo.Version;
    }
}