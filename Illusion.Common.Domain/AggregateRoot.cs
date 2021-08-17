using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Illusion.Common.Domain.Events;

namespace Illusion.Common.Domain
{
    public abstract class BaseAggregateRoot<TId>
    {
        public TId Id { get; protected set; }
    }

    public abstract class AggregateRoot : BaseAggregateRoot<Guid>
    {
        public ulong Version
        {
            get => _version - (uint)_changes.Count;
            private set => _version = value;
        }

        protected AggregateRoot() => _changes = new List<IEvent>();

        protected void When(IEvent @event)
        {
            this.AsDynamic().WhenEvent((dynamic) @event);
            Version++;
        }

        protected virtual void WhenEvent(IEvent @event) => throw new InvalidOperationException($"Could not handle event of type {@event.GetType().Name}");

        private readonly List<IEvent> _changes;
        private ulong _version = ulong.MaxValue;

        protected void Apply(IEvent @event)
        {
            When(@event);
            _changes.Add(@event);
        }

        public IEnumerable<IEvent> GetChanges() => _changes.AsEnumerable();

        public void Load(IEnumerable<IEvent> history)
        {
            foreach (var @event in history)
            {
                When(@event);
            }
        }

        public async Task Load(IAsyncEnumerable<IEvent> history)
        {
            await foreach (var @event in history)
            {
                When(@event);
            }
        }

        public void ClearChanges()
        {
            _changes.Clear();
        }

        protected void ApplyToEntity(IInternalEventHandler entity, IEvent @event) => entity?.Handle(@event);
    }
}