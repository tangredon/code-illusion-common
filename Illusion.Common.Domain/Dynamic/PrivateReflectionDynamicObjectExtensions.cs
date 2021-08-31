namespace Illusion.Common.Domain.Dynamic
{
    /// <summary>
    /// Taken from https://github.com/gregoryyoung/m-r
    /// </summary>
    internal static class PrivateReflectionDynamicObjectExtensions
    {
        /// <summary>
        /// Returns this DynamicEventBasedAggregateRoot as dynamic type
        /// </summary>
        /// <param name="aggregateRoot">The DynamicEventBasedAggregateRoot</param>
        /// <returns>A dynamic representation of the DynamicEventBasedAggregateRoot</returns>
        public static dynamic AsDynamic(this AggregateRoot aggregateRoot)
        {
            return PrivateReflectionDynamicObject.WrapObjectIfNeeded(aggregateRoot);
        }
    }
}