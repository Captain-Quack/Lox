using Lox.Tokens;

namespace Lox.Scanning;

using System.Globalization;
using System.Runtime.CompilerServices;
using Lox.Extras;
using static TokenType;

using TO = TokenType;

/// <summary>
/// code goes in, tokens go out.
/// </summary>
///
internal sealed class Scanner
{
    private const string EmptySource = "IHOPENOBODYENTERSTHISEXACTSEQUENCEOFCHARACTERSTHATWOULDBEBAD!";
    private string source;
    private Token[] tokens; // <-- Changed from List<Token> to Token[]
    private int start;
    private int current;
    private int line;
    private int column;
    private int tokenCount; // Added counter for tokens added

    internal static readonly Dictionary<string, TO> keywords = new(StringComparer.OrdinalIgnoreCase)
    {
        { "and", AND },
        { "assert", ASSERT },
        { "class", CLASS },
        { "del",  DEL },
        { "else", ELSE },
        { "false", FALSE},
        { "true", TRUE },
        { "fun", FUN },
        { "for", FOR},
        { "if", IF },
        { "null", NULL},
        { "nil", NULL},
        { "not", BANG },  // This will need to be fixed if we add "is" keyword (is not)
        { "or", OR },
        { "print", PRINT },
        { "return", RETURN },
        { "this", THIS },
        { "super", SUPER },
        { "var", VAR },
        { "while", WHILE},
        { "break", BREAK },
        { "continue", CONTINUE }
    };

    internal Scanner(string source = EmptySource)
    {
        tokens = new Token[source?.Length + 1 ?? 0];
        this.source = source!;
    }

    internal Token[] ScanTokens()
    {
        line++;
        column++;

        if (source == EmptySource)
        {
            throw new InvalidDataException("Check the Executor, you built the scanner wrong.");
        }
        while (!IsAtEnd)
        {
            start = current;
            ScanToken();
            column++;
        }

        tokens[tokenCount++] = new Token(EOF, "", null, line, column + 1);

        return tokens;
    }

    public IList<Token> ScanTokens(string code)
    {
        ArgumentNullException.ThrowIfNull(code);
        tokens = new Token[code.Length];
        source = code;
        return ScanTokens();
    }

    private void ScanToken()
    {
        char c = Advance();

        switch (c)
        {
            case '#':
                AddToken(HASH);
                break;

            case '(':
                AddToken(LEFTPAREN);
                break;

            case ')':
                AddToken(RIGHTPAREN);
                break;

            case '{':
                AddToken(LEFTBRACE);
                break;

            case '}':
                AddToken(RIGHTBRACE);
                break;

            case '[':
                AddToken(LEFTBRACKET);
                break;

            case ']':
                AddToken(RIGHTBRACKET);
                break;

            case ',':
                AddToken(COMMA);
                break;

            case '.':
                AddToken(DOT);
                break;

            case ';':
                AddToken(SEMICOLON);
                break;

            case '*':
                AddToken(STAR);
                break;

            case '^':
                AddToken(CARROT);
                break;

            case '-':
                AddToken(MINUS);
                break;

            case '+':
                AddToken(PLUS);
                break;

            case '/':
                HandleSlash();
                break;

            case '%':
                AddToken(PERCENT);
                break;

            case '=':
                AddToken(Match('=') ? EQUALEQUAL : EQUAL);
                break;

            case '>':
#pragma warning disable S3358 // Ternary operators should not be nested
                AddToken(Match('=') ? GREATEREQUAL : Match('>') ? RSHIFT : GREATER);
                break;

            case '<':
                AddToken(Match('=') ? LESSEQUAL : Match('<') ? LSHIFT : LESS);
                break;
#pragma warning restore S3358 // Ternary operators should not be nested

            case '!':
                AddToken(Match('=') ? BANGEQUAL : BANG);
                break;

            case '|':
                AddToken(Match('|') ? OR : BAR);
                break;

            case '&':
                AddToken(Match('&') ? AND : AMP);
                break;

            case ':':
                AddToken(COLON);
                break;

            case '?':
                AddToken(QUESTION);
                break;

            case '"':
            case '\'':
                HandleStringLiteral(ref c);
                break;

            default:
                HandleDefault(ref c);
                break;
        }
    }

    private void HandleSlash()
    {
        if (Match('/'))
        {
            while (Peek != '\n' && !IsAtEnd) Advance();
        }
        else
        {
            AddToken(SLASH);
        }
    }

    private void HandleStringLiteral(ref char terminator)
    {
        char nextChar = Peek;
        while (nextChar != terminator && !IsAtEnd)
        {
            if (nextChar == '\n')
            {
                column = 1;
                line++;
            }
            Advance();
            nextChar = Peek;
        }

        if (IsAtEnd)
        {
            LoxRunner.Error(line, column, "Syntax Error", "Unterminated string.", $"{Peek}");
            return;
        }

        Advance();

        var value = source.Substring(start + 1, current - start - 2);
        AddToken(CHARACTERS, value);
    }

    private void HandleDefault(ref char c)
    {
        if ('0' <= c && c <= '9')
        {
            HandleNumberLiteral();
        }
        else if (('a' <= c && c <= 'z') || ('A' <= c && c <= 'Z') || c == '_')
        {
            HandleIdentifier();
        }
        else
        {
            if (!char.IsWhiteSpace(c))
            {
                LoxRunner.Error(line, column, "SyntaxError", $"{c} was unexpected", c.ToString());
            }
        }
    }

    private void HandleNumberLiteral()
    {
        while ('0' <= Peek && Peek <= '9') Advance();

        if (Peek == '.' && '0' <= PeekNext() && PeekNext() <= '9')
        {
            Advance();
            while ('0' <= Peek && Peek <= '9') Advance();
        }

        AddToken(NUMBER, double.Parse(source[start..current]));
    }

    private void HandleIdentifier()
    {
        while (char.IsAsciiLetterOrDigit(Peek) || Peek == '_') Advance();

        string text = source[start..current];
        if (keywords.TryGetValue(text, out TO type))
        {
            AddToken(type);
        }
        else
        {
            AddToken(IDENTIFIER);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private char Advance()
    {
        char c = source[current++];
        ++column;
        if (c == '\n')
        {
            column = 1;
            ++line;
        }

        return c;
    }

    /// <summary>
    /// Conditional move
    /// </summary>
    /// <param name="expected">What to check for</param>
    /// <returns>if it matched or not</returns>
    private bool Match(char expected)
    {
        if (IsAtEnd || source[current] != expected) return false;
        ++current;
        return true;
    }

    private void AddToken(TO token, object? literal = null)
    {
        tokens[tokenCount++] = new Token(token, source[start..current], literal, line, column);
    }

    private bool IsAtEnd => current >= source.Length;

    /// <summary>
    /// What your index is currently on (not read yet.)
    /// </summary>
    ///
    private char Peek => IsAtEnd ? '\0' : source[current];

    /// <summary>
    /// See a token ahead of the current index
    /// </summary>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private char PeekNext()
    {
        if (current + 1 >= source.Length) return '\0';
        return source[current + 1];
    }
}
