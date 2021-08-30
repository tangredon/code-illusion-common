using System.Diagnostics;

namespace Illusion.Common.Core
{
    public static class ActivityHelper
    {
        private static readonly Activity NullActivity = new Activity("null-activity");

        public static Activity Current => Activity.Current ?? NullActivity;
        public static ActivitySource Source => Current.Source;
    }
}