using Illusion.Common.Framework.Events;

namespace Illusion.Common.Framework
{
    public interface IInternalEventHandler
    {
        void Handle(IEvent @event);
    }
}