using Lox.Tokens;

namespace Lox.Parser;

using Lox.Statements;
using static TokenType;

internal sealed partial class Parser
{
    /// <summary>
    /// This is the tokens, which is feed from the Scanner
    /// </summary>
    private readonly List<Token> tokens;

    /// <summary>
    /// Used  for indexing
    /// </summary>
    private int current;

    internal Parser(List<Token> tokens) => this.tokens = tokens;

    /// <summary>
    /// Stuff starts here
    /// </summary>
    public List<Stmt> Parse()
    {
        if (tokens is null)
        {
            throw new InvalidDataException();
        }
        var statements = new List<Stmt>();
        while (!AtEnd)
        {
            try
            {
                statements.Add(Decleration()!);
            }
            catch (ParseException ex)
            {
                LoxRunner.Error(ex.Line, ex.Column, "Parsing Exception", ex.Message, ex.Lexeme);
                Synchronize();
            }
        }
        return statements;
    }

    private Stmt Decleration()
    {
        // [var] x = 5
        if (Match(VAR)) return VarDeclaration();
        if (Match(FUN)) return FunctionDeclaration("function");
        return Statement();
    }

    private Stmt Statement() => Peek.Type switch
    {
        ASSERT => AdvanceAndReturn(AssertStatement),
        DEL => AdvanceAndReturn(DelStatement),
        CLASS => AdvanceAndReturn(ClassStatement),
        IF => AdvanceAndReturn(IfStatement),
        LEFTBRACE => AdvanceAndReturn(BlockStatement),
        PRINT => AdvanceAndReturn(PrintStatement),
        RETURN => AdvanceAndReturn(ReturnStatement, true),
        WHILE => AdvanceAndReturn(WhileStatement),
        FOR => AdvanceAndReturn(ForStatement),
        CONTINUE => AdvanceAndReturn(ContinueStatement, true),
        BREAK => AdvanceAndReturn(BreakStatement, true),
        // unpaired "else"
        ELSE => throw new ParseException(Peek.Line, Peek.Column, "'else' cannot start a statement.", Peek.Lexeme),
        // [1 + 1]
        _ => ExpressionStatement()
    };

    private Stmt AdvanceAndReturn(Func<Stmt> func, bool allowSemicolon = false)
    {
        Advance();
        if (Peek.Type == SEMICOLON)
        {
            if (allowSemicolon)
            {
                Advance();  // Consume the semicolon
            }
            else
            {
                throw new ParseException(Peek.Line, Peek.Column, "Expected an expression before end of statement (received ';')", tokens[current - 1].Lexeme);
            }
        }
        return func();
    }

    /*
     * Helper Methods
     */

    /// <summary>
    /// Expect a token to show up with Peek, otherwise throw an error.
    /// </summary>
    /// <param name="type">Expected</param>
    /// <param name="message">Error Message</param>
    private Token Consume(TokenType type, string message) => Check(type) ? Advance() : throw Error(Peek, message);

    private bool Match(params TokenType[] types)
    {
        if (types.Contains(Peek.Type))
        {
            Advance();
            return true;
        }

        return false;
    }

    private bool Check(TokenType type) => Peek.Type == type;

    private bool AtEnd => tokens[current].Type == EOF;

    private Token Peek => tokens[current];

    private Token Advance()
    {
        if (!AtEnd)
        {
            current++;
        }
        return tokens[current - 1];
    }

    /// <summary>
    /// Skip a statement - Don't use  unless something bad happens.
    /// (used for recovery)
    /// </summary>
    private void Synchronize()
    {
        Advance();

        while (Peek.Type != EOF)
        {
            if (tokens[current - 1].Type == SEMICOLON)
                return;

            switch (Peek.Type)
            {
                case CLASS:
                case FUN:
                case VAR:
                case FOR:
                case IF:
                case WHILE:
                case PRINT:
                case RETURN:
                    return;

                default:
                    Advance();
                    break;
            }
        }
    }

    private static ParseException Error(Token token, string message)
    {
        return new ParseException(token.Line + 1, token.Column, message, token.Lexeme);
    }
}
