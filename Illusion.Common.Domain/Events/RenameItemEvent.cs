using System;

namespace Illusion.Common.Domain.Events
{
    public class RenameItemEvent : IEvent
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}