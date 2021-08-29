using System;
using System.Threading;
using System.Threading.Tasks;

namespace Illusion.Common.Domain
{
    public interface IAggregateStore
    {
        Task<bool> Exists<T>(Guid aggregateId, CancellationToken cancellationToken);

        Task Save<T>(T aggregate, CancellationToken cancellationToken) where T : AggregateRoot;

        Task<T> Load<T>(Guid aggregateId, CancellationToken cancellationToken) where T : AggregateRoot;
    }
}