namespace Lox.Tokens;

/// <summary>
/// This object is created by the scanner, read by the parser, and manipulated in the interpreter.
/// </summary>
/// <param name="Type">The TokenType (name corresponds to matching character(s))</param>
/// <param name="Lexeme">Chunk of code the token came from</param>
/// <param name="Literal">This value is fed to the Interpreter</param>
/// <param name="Line">Line of code the token is on (for debugging)</param>
/// <seealso cref="Scanner.ScanTokens()"/>
/// <seealso cref="TokenType"/>
public record Token(TokenType Type, string Lexeme, object? Literal, int Line, int Column);
