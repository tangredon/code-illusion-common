using System.Reflection;

namespace Illusion.Common.Helpers
{
    public static class VersionInfo
    {
        public static string Version => Assembly.GetExecutingAssembly()?.GetName()?.Version?.ToString() ?? "unknown";
    }
}