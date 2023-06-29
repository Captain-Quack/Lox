// Ignore Spelling: Stringify

#pragma warning disable CA1032

using Lox.Tokens;

namespace Lox
{
    [Serializable]
    public class RuntimeException : Exception
    {
        public RuntimeException(Token token, string message) : base(message)
        {
            Token = token;
        }

        public Token Token { get; }

        /// <summary>
        /// Implemented to keep code analyzer happy.
        /// </summary>
        /// <param name="serializationInfo"></param>
        /// <param name="streamingContext"></param>
        /// <exception cref="NotImplementedException"></exception>
        protected RuntimeException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
        {
            throw new NotImplementedException();
        }
    }
}

#pragma warning restore CA1032
