using System;
using System.Linq;
using Base62;

namespace Illusion.Common.Extensions
{
    public static class Base62Convertor
    {
        public static Guid ConvertFrom(string base62)
        {
            if (string.IsNullOrEmpty(base62))
            {
                return Guid.Empty;
            }

            return new Guid(base62.FromBase62(true).ToArray());
        }

        public static string ConvertTo(Guid guid)
        {
            if (guid == Guid.Empty)
            {
                return null;
            }

            return guid.ToByteArray().ToArray().ToBase62(true);
        }
    }
}
