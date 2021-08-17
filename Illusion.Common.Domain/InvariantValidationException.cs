using System;

namespace Illusion.Common.Domain
{
    public class InvariantValidationException : Exception
    {
        public InvariantValidationException(string message) : base(message) {}
    }
}
