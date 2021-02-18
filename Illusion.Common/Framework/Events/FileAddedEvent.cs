using System;

namespace Illusion.Common.Framework.Events
{
    public class FileAddedEvent : IEvent
    {
        public Guid Id { get; set; }
        public Guid ParentId { get; set; }
        public string Name { get; set; }
        public string MimeType { get; set; }
        public long Size { get; set; }
    }
}