using System;

namespace Illusion.Common.Framework.Events
{
    public class StorageInitializedEvent : IEvent
    {
        public Guid Id { get; set; }
    }
}