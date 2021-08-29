using Illusion.Common.Domain.Events;
using Illusion.Common.Domain.Helpers;
using Xunit;

namespace Illusion.Common.Domain.UnitTests
{
    public class EventNameExtractorUnitTests
    {
        [Fact]
        public void GetEventName_Correct()
        {
            var eventName = EventNameExtractor.GetEventName(typeof(TestEvent));

            Assert.Equal("Test", eventName);
        }

        private class TestEvent : IEvent
        {
        }
    }
}
