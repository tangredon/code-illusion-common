namespace Illusion.Common.FeatureFlags
{
    public class FeatureFlagsOptions
    {
        public static string SectionName = "FeatureFlags";

        public string ApiKey { get; set; }
        public string LocalFilePath { get; set; } = "flags.yaml";
    }
}