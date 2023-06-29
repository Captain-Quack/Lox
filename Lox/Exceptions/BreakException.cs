// Ignore Spelling: Stringify

using System.Runtime.Serialization;

#pragma warning disable CA1032

namespace Lox
{
    [Serializable]
    public class BreakException : Exception
    {
        public BreakException()
        {
        }

        protected BreakException(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            throw new NotImplementedException();
        }
    }
}

#pragma warning restore CA1032
