using System;
using System.Linq;
using Base62;

namespace Illusion.Common.Extensions
{
    public static class Base62Convertor
    {
        public static Guid ConvertFrom(string base62) => new(base62.FromBase62().Reverse().ToArray());
        public static string ConvertTo(Guid guid) => guid.ToByteArray().Reverse().ToArray().ToBase62();
    }
}
