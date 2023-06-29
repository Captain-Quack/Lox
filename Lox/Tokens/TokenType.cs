namespace Lox.Tokens;

public enum TokenType
{
    // Special Characters
    COMMA,

    DOT,
    SEMICOLON,
    HASH,   // #
    COLON,
    QUESTION,

    // Braces and Brackets
    /// <summary>
    /// (
    /// </summary>
    LEFTPAREN,

    /// <summary>
    /// )
    /// </summary>
    RIGHTPAREN,

    /// <summary>
    /// {
    /// </summary>
    LEFTBRACE,

    /// <summary>
    /// }
    /// </summary>
    RIGHTBRACE,

    /// <summary>
    /// [
    /// </summary>
    LEFTBRACKET,

    /// <summary>
    /// ]
    /// </summary>
    RIGHTBRACKET,

    // Binary Operators
    SLASH,

    STAR,
    PERCENT,
    CARROT,
    BAR,   // |
    AMP,   // &

    // Comparison Tokens
    BANG, BANGEQUAL,   // !, !=

    EQUAL, EQUALEQUAL,
    GREATER, GREATEREQUAL, RSHIFT,
    LESS, LESSEQUAL, LSHIFT,

    // Arithmetic Operators
    PLUS,

    MINUS,

    // Literals
    IDENTIFIER,

    CHARACTERS,
    NUMBER,
    BOOL,

    // Keywords
    AND,

    CLASS,
    DEL,
    ELSE,
    FALSE,
    FUN,
    FOR,
    IF,
    NULL,
    OR,
    PRINT,
    RETURN,
    SUPER,
    THIS,
    TRUE,
    VAR,
    WHILE,
    ASSERT,
    BREAK,
    CONTINUE,

    // Special Tokens
    NEWLINE,

    EOF
}
