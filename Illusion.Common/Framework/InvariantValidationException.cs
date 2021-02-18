using System;

namespace Illusion.Common.Framework
{
    public class InvariantValidationException : Exception
    {
        public InvariantValidationException(string message) : base(message) {}
    }
}
