using Illusion.Common.Domain.Events;

namespace Illusion.Common.Domain
{
    public interface IInternalEventHandler
    {
        void Handle(IEvent @event);
    }
}