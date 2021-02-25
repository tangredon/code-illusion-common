using System;
using System.Threading.Tasks;

namespace Illusion.Common.Framework
{
    public interface IAggregateStore
    {
        Task<bool> Exists<T>(Guid aggregateId);

        Task Save<T>(T aggregate) where T : AggregateRoot;

        Task<T> Load<T>(Guid aggregateId) where T : AggregateRoot;
    }
}