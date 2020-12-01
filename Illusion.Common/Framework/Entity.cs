using System;
using Illusion.Common.Framework.Events;

namespace Illusion.Common.Framework
{
    public abstract class Entity<TId> : IInternalEventHandler
    {
        private readonly Action<IEvent> _applier;
        public TId Id { get; protected set; }

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
