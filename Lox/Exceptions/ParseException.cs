namespace Lox;
#pragma warning disable CA1032

/// <summary>
/// Thrown when something bad happens while parsing. For example, if you have this code:
/// <example>
/// var x = var(;)
/// </example>
/// </summary>
[Serializable]
public class ParseException : Exception
{
    public ParseException(int line, int column, string message, string lexeme, bool endquotemagic = false) : base(message)
    {
        Line = line;
        Column = column;
        Lexeme = endquotemagic ? lexeme + "\b" : lexeme;
    }

    /// <summary>
    /// Line of code the error happened on.
    /// </summary>
    public int Line { get; }

    /// <summary>
    /// This mostly don't work, but its supposed to show
    /// where in the line the error happened.
    /// </summary>
    public int Column { get; }

    /// <summary>
    /// Chunk of code thi
    /// </summary>
    public string Lexeme { get; }

    /// <summary>
    /// I implemented this to keep my code analyzer happy (it was bullying me.)
    /// </summary>
    /// <param name="serializationInfo"></param>
    /// <param name="streamingContext"></param>
    /// <exception cref="NotImplementedException"></exception>
    protected ParseException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
    {
        throw new NotImplementedException();
    }
}

#pragma warning restore CA1032
