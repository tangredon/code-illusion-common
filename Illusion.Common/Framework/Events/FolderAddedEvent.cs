using System;

namespace Illusion.Common.Framework.Events
{
    public class FolderAddedEvent : IEvent
    {
        public Guid Id { get; set; }
        public Guid ParentId { get; set; }
        public string Name { get; set; }
    }
}