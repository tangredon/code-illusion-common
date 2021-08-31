using System;

namespace Illusion.Common.Domain
{
    // todo: 3
    public class InvariantValidationException : Exception
    {
        public InvariantValidationException(string message) : base(message) {}
    }
}
