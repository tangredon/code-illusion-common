using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Illusion.Common.Framework.Events;

namespace Illusion.Common.Framework
{
    public abstract class BaseAggregateRoot<TId>
    {
        public TId Id { get; protected set; }
    }

    public abstract class AggregateRoot : BaseAggregateRoot<Guid>
    {
        public ulong Version { get; private set; } = ulong.MaxValue;

        protected abstract void When(IEvent @event);

        private readonly List<IEvent> _changes;

        protected AggregateRoot() => _changes = new List<IEvent>();

        protected void Apply(IEvent @event)
        {
            When(@event);
            EnsureValidState();
            _changes.Add(@event);
        }

        public IEnumerable<IEvent> GetChanges() => _changes.AsEnumerable();

        public void Load(IEnumerable<IEvent> history)
        {
            foreach (var @event in history)
            {
                When(@event);
                Version++;
            }
        }

        public async Task Load(IAsyncEnumerable<IEvent> history)
        {
            await foreach (var @event in history)
            {
                When(@event);
                Version++;
            }
        }

        public void ClearChanges() => _changes.Clear();

        protected abstract void EnsureValidState();

        protected void ApplyToEntity(IInternalEventHandler entity, IEvent @event) => entity?.Handle(@event);
    }
}