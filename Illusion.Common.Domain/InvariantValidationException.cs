using System;

namespace Illusion.Common.Domain
{
    // todo: 4
    public class InvariantValidationException : Exception
    {
        public InvariantValidationException(string message) : base(message) {}
    }
}
