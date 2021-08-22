using System.Diagnostics;

namespace Illusion.Common.Telemetry
{
    public static class ActivityHelper
    {
        public static Activity Current => Activity.Current ?? new Activity("null-activity");
    }
}