namespace Illusion.Common.FeatureFlags
{
    public interface IFeatureFlagProvider
    {
        public bool GetFeatureFlag(string feature);
    }
}