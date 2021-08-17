using System;

namespace Illusion.Common.Domain.Events
{
    public class StorageInitializedEvent : IEvent
    {
        public Guid Id { get; set; }
    }
}