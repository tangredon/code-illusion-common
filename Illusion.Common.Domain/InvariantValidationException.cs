using System;

namespace Illusion.Common.Domain
{
    // todo: 5
    public class InvariantValidationException : Exception
    {
        public InvariantValidationException(string message) : base(message) {}
    }
}
