using System;

namespace Illusion.Common.Framework.Events
{
    public class RenameItemEvent : IEvent
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}