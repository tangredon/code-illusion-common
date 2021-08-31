using System;

namespace Illusion.Common.Domain
{
    // todo: 2
    public class InvariantValidationException : Exception
    {
        public InvariantValidationException(string message) : base(message) {}
    }
}
