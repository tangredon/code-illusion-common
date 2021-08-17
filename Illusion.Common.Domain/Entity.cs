using System;
using Illusion.Common.Domain.Events;

namespace Illusion.Common.Domain
{
    public abstract class BaseEntity<TId>
    {
        public TId Id { get; protected set; }
    }

    public abstract class Entity : BaseEntity<Guid>, IInternalEventHandler
    {
        private readonly Action<IEvent> _applier;

        protected Entity(Action<IEvent> applier) => _applier = applier;

        protected abstract void When(IEvent @event);

        protected void Apply(IEvent @event)
        {
            When(@event);
            _applier(@event);
        }

        void IInternalEventHandler.Handle(IEvent @event) => When(@event);
    }
}
