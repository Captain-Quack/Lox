using System.Diagnostics;
using Lox.Expressions;
using Lox.Interpreting;
using Lox.Tokens;
using static Lox.Tokens.TokenType;

namespace Lox.Parser;

internal sealed partial class Parser
{
    private Expr CreateBinaryExpression(Func<Expr> expression, params TokenType[] types)
    {
        Expr expr = expression();
        while (Match(types))
        {
            // I would think this consumes a lot of memory, but I am unexperienced with that
            // sort of thing, so... idk.
            expr = new Binary(expr, tokens[current - 1], expression());
        }

        return expr;
    }

    private Expr Expression() => Assignment();

    private Expr Assignment()
    {
        Expr expr = Ternary();
        if (Match(EQUAL))
        {
            Token equals = tokens[current - 1];
            Expr value = Expression();

            if (expr is Variable variable)
            {
                Token name = variable.Name;
                return new Assign(name, value);
            }
            if (expr is Getter getter)
            {
                return new Setter(getter.GetFrom, getter.Name, value);
            }
            LoxRunner.Error(equals.Line, equals.Column, "Runtime Error", "Invalid Assignment Target", equals.Lexeme);
        }

        return expr;
    }

    private Expr Ternary()
    {
        Expr expr = Or();
        if (Match(QUESTION))
        {
            Expr middle = Or();
            Consume(COLON, "Expected the ':' part of the ternary conditional expression");
            Expr left = Or();
            return new Ternary(expr, middle, left);
        }
        return expr;
    }

    private Expr Or()
    {
        Expr expr = And();
        while (Match(OR))
        {
            Token or = tokens[current -1];
            Expr left = And();
            expr = new Logical(expr, or, left);
        }
        return expr;
    }

    private Expr And()
    {
        Expr expr = Equality();
        while (Match(AND))
        {
            Token and = tokens[current - 1];
            Expr right = Equality();
            expr = new Logical(expr, and, right);
        }
        return expr;
    }

    private Expr Equality() => CreateBinaryExpression(Comparison, BANGEQUAL, EQUALEQUAL);

    private Expr Comparison() => CreateBinaryExpression(Bit, GREATER, GREATEREQUAL, LESS, LESSEQUAL);

    private Expr Bit() => CreateBinaryExpression(Term, LSHIFT, RSHIFT);

    private Expr Term() => CreateBinaryExpression(Factor, PLUS, MINUS);

    private Expr Factor() => CreateBinaryExpression(Unary, SLASH, STAR, CARROT, PERCENT);

    private Expr Unary()
    {
        var unaryTokens = new List<Token>();

        while (Match(BANG, MINUS, HASH))
        {
            unaryTokens.Add(tokens[current - 1]);
        }

        Expr expr = Call();

        // note for readers: this is skipped if count is 0.
        for (int i = unaryTokens.Count - 1; i >= 0; i--)
        {
            expr = new Unary(unaryTokens[i], expr);
        }

        return expr;
    }

    private Expr Call()
    {
        Expr expr = Primary();

        while (true)
        {
            if (Match(LEFTPAREN))
            {
                expr = FinishCall(ref expr);
            }
            else if (Match(DOT))
            {
                expr = new Getter(expr, Consume(IDENTIFIER, "Expected property name after '.'!"));
            }
            else
            {
                break;
            }
        }

        return expr;
    }

    private Func FinishCall(ref Expr callee)
    {
        var args = new List<Expr>();
        if (!Check(RIGHTPAREN))
        {
            do
            {
                if (args.Count >= 25)
                {
                    throw new ParseException(Peek.Line, Peek.Column, $"Cannot have more than 25 arguments, that's a bit ridiculous.", Peek.Lexeme);
                }
                args.Add(Expression());
            } while (Match(COMMA));
        }
        return new Func(callee, Consume(RIGHTPAREN, "Expected matching ')'"), args);
    }

    private List List()
    {
        var elements = new List<Expr>();

        if (!Check(RIGHTBRACKET))
        {
            do
            {
                elements.Add(Expression());
            } while (Match(COMMA));
        }

        Consume(RIGHTBRACKET, "Expected ']' after list elements.");
        return new List(elements.ToArray());
    }

    private Expr Primary()
    {
        if (Match(RIGHTPAREN)) throw Error(tokens[current - 1], "Unmatched ')'");
        if (Match(RIGHTBRACE)) throw Error(tokens[current - 1], "Unmatched '}'");
        if (Match(RIGHTBRACKET)) throw Error(tokens[current - 1], "Unmatched ']'");
        if (Match(FALSE)) return new Literal(false);
        if (Match(TRUE)) return new Literal(true);
        if (Match(NULL)) return new Literal(null);
        if (Match(NUMBER, CHARACTERS)) return new Literal(tokens[current - 1].Literal);

        if (Match(SUPER))
        {
            Token keyword = tokens[current - 1];
            Consume(DOT, "Expected '.' after 'super'.");
            Token method = Consume(IDENTIFIER, "a method was expected after superclass assessor.");
            return new Super(keyword, method);
        }
        if (Match(LEFTBRACKET))
        {
            return List();
        }
        if (Match(LEFTPAREN))
        {
            Expr expr = Expression();
            Consume(RIGHTPAREN, "Expected ')' after expression.");
            return new Grouping(expr);
        }
        if (Match(THIS)) return new This(tokens[current - 1]);
        if (Match(IDENTIFIER))
        {
            return new Variable(tokens[current - 1]);
        }
        throw Error(Peek, "Expected Expression");
    }
}
