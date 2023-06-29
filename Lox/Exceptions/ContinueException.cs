// Ignore Spelling: Stringify

#pragma warning disable CA1032

using System.Runtime.Serialization;

namespace Lox
{
    [Serializable]
    public class ContinueException : Exception
    {
        public ContinueException()
        {
        }

        protected ContinueException(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            throw new NotImplementedException();
        }
    }
}

#pragma warning restore CA1032
