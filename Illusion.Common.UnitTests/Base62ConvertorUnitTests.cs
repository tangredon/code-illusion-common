using System;
using Illusion.Common.Extensions;
using Xunit;

namespace Illusion.Common.UnitTests
{
    public class Base62ConvertorUnitTests
    {
        [Fact]
        public void Base62_Guid_Base62()
        {
            var base62 = "00uawhkxQbMHrZaTl5d5";

            var guid = Base62Convertor.ConvertFrom(base62);
            var converted = Base62Convertor.ConvertTo(guid);

            Assert.Equal(base62, converted);
        }

        [Fact]
        public void Guid_Base62_Guid()
        {
            var guid = Guid.NewGuid();

            var base62 = Base62Convertor.ConvertTo(guid);
            var converted = Base62Convertor.ConvertFrom(base62);

            Assert.Equal(guid, converted);
        }
    }
}
