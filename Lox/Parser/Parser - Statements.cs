using Lox.Expressions;
using Lox.Statements;
using Lox.Tokens;
using static Lox.Tokens.TokenType;

namespace Lox.Parser;

internal sealed partial class Parser
{
    /// <summary>
    /// Similar to python's assert keyword.
    /// For some reason, this is sometimes buggy.
    /// </summary>
    /// <returns></returns>
    private Assert AssertStatement()
    {
        // Remember, the assert statement is already swallowed!
        // The way we know a statement is an assert statement is by swallowing the first token
        // in it, and seeing it is "ASSERT".
        Expr condition = Expression();
        Literal message = new("Assertion failed");
        // assert bool [,] message
        if (Match(COMMA))
        {
            if (Expression() is Literal l)
            {
                message = l;
            }
            else
            {
                Token someToken = tokens[current - 1];
                throw new ParseException(someToken.Line, someToken.Column, "Messages must be runtime constants", someToken.Lexeme);
            }
        }
        _ = Consume(SEMICOLON, "Expected ';' after assertion.");

        // Return a new Assert object
        return new Assert(condition, tokens[current - 3], message);
    }

    private Block BlockStatement()
    {
        List<Stmt> statements = new();

        while (!Check(RIGHTBRACE) && !AtEnd)
        {
            statements.Add(Decleration());
        }

        _ = Consume(RIGHTBRACE, "Expected '}' after block.");
        return new Block(statements);
    }

    private Stmt ClassStatement()
    {
        Token name = Consume(IDENTIFIER, "Expected a class name");
        Variable? superclass = null;
        if (Match(COLON))
        {
            Consume(IDENTIFIER, "Expected inherited class name after ':' symbol.");
            superclass = new(tokens[current - 1]);
        }

        _ = Consume(LEFTBRACE, "Expected '{' after class name");
        List<Function> meathods = new();
        while (!Check(RIGHTBRACE) && !AtEnd)
        {
            meathods.Add(FunctionDeclaration("method"));
        }
        Consume(RIGHTBRACE, "Expect '}' after class body");
        return new Class(name, superclass, meathods);
    }

    private Del DelStatement()
    {
        var values = new List<Token>() { Consume(IDENTIFIER, "Tried to delete something that wasn't an identifier.") };

        if (Match(COMMA))
        {
            do
            {
                values.Add(Consume(IDENTIFIER, "Tried to delete something that wasn't an identifier."));
            } while (Match(COMMA));
        }

        _ = Consume(SEMICOLON, "Expected ';' after expression.");
        return new Del(values);
    }

    private If IfStatement()
    {
        Expr expr;
        if (Match(LEFTPAREN))
        {
            expr = Expression();
            _ = Consume(RIGHTPAREN, "Expected ')' after parenthesized conditional");
        }
        else
        {
            expr = Expression();
            if (!Check(LEFTBRACE))
            {
                throw new ParseException(Peek.Line, Peek.Column, "Expected '{' after conditional", tokens[current - 1].Lexeme);
            }
        }
        Stmt then = Statement();
        return new If(expr, then, Match(ELSE) ? Statement() : null);
    }

    private Function FunctionDeclaration(string kind)
    {
        Token name = Consume(IDENTIFIER, $"Expected valid {kind} name");
        Consume(LEFTPAREN, $"Expected '(' after {kind} name");
        List<Token> parameters = new();
        if (!Check(RIGHTPAREN))
        {
            do
            {
                if (parameters.Count >= 255)
                {
                    _ = Error(Peek, "Can't have more then 255 arguments");
                }

                parameters.Add(Consume(IDENTIFIER, "Expect parameter name."));
            } while (Match(COMMA));
        }
        _ = Consume(RIGHTPAREN, "Expected ')' to close " + kind + " parameters.");
        _ = Consume(LEFTBRACE, "'{' is required to start function body.");
        return new Function(name, parameters, BlockStatement());
    }

    private Print PrintStatement()
    {
        var values = new List<Expr>() { Expression() };

        if (Match(COMMA))
        {
            do
            {
                values.Add(Expression());
            } while (Match(COMMA));
        }

        _ = Consume(SEMICOLON, "Expected ';' after expression.");
        return new Print(values);
    }

    public Return ReturnStatement()
    {
        // already past the 'return' keyword
        Token keyword = tokens[current - 1];

        // Check if there is an expression after 'return'
        Expr? value = null;
        if (tokens[current].Type != SEMICOLON) // Check if the next token is not a semicolon
        {
            value = Expression();
        }

        if (tokens[current].Type != SEMICOLON) // Check if the current token (which should now be after the expression) is not a semicolon
        {
            throw Error(tokens[current], "Expected ';' after return statement.");
        }
        Advance(); // consume the semicolon

        return new Return(keyword, value);
    }

    public StmtExpression ExpressionStatement()
    {
        Expr expr = Expression();
        _ = Consume(SEMICOLON, "Expected ';' after expression.");
        return new StmtExpression(expr);
    }

    private Var VarDeclaration()
    {
        Token name = Consume(IDENTIFIER, "Expected variable name.");

        Expr? initializer = null;
        if (Match(EQUAL))
        {
            initializer = Expression();
        }

        _ = Consume(SEMICOLON, "Expected ';' after variable declaration.");
        return new Var(name, initializer);
    }

    private While WhileStatement()
    {
        Expr expr;
        if (Match(LEFTPAREN))
        {
            expr = Expression();
            _ = Consume(RIGHTPAREN, "Expected ')' after parenthesized conditional");
        }
        else
        {
            expr = Expression();
            if (!Check(LEFTBRACE))
            {
                throw new ParseException(Peek.Line, Peek.Column, "Expected '{' after conditional", tokens[current - 1].Lexeme);
            }
        }
        Stmt body = Statement();
        return new While(expr, body);
    }

    private Stmt ForStatement()
    {
        _ = Consume(LEFTPAREN, "Expected '(' after 'for'");
        Stmt? first;
        if (Match(VAR))
        {
            first = VarDeclaration();
        }
        else if (Match(SEMICOLON))
        {
            first = null;
        }
        else
        {
            first = ExpressionStatement();
        }
        Expr? condition = !Check(SEMICOLON) ? Expression() : null;
        _ = Consume(SEMICOLON, "Expected ';' after condition (second 'for' expression)");
        Expr? increment = !Check(SEMICOLON) ? Expression() : null;
        _ = Consume(RIGHTPAREN, "Expected ')' after for loop clauses");
        Stmt body = Statement();
        if (increment is not null)
        {
            body = new Block(new List<Stmt>() { body, new StmtExpression(increment) });
        }
        body = new While(condition ?? new Literal(true), body);
        if (first is not null)
        {
            body = new Block(new List<Stmt>() { first, body });
        }
        return body;
    }

    private Break BreakStatement() => new(tokens[current - 2]);

    private Continue ContinueStatement() => new();
}
