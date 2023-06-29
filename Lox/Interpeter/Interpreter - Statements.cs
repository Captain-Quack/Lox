using System.Linq;
using Lox.Exceptions;
using Lox.Functions;
using Lox.Statements;
using Lox.Tokens;
using static Lox.Extras.Utils;
using static Lox.Tokens.TokenType;

#pragma warning disable CS8603 // Possible null reference return.

namespace Lox.Interpreting;

public partial class Interpreter
{
    public object Visit(Assert stmt) => IsTruthy(EvaluateExpression(stmt.Condition)) is bool b && !b
            ? throw new RuntimeException(stmt.Keyword, "Assertion failed: " + EvaluateExpression(stmt.Message))
            :
#if DEBUG
        "true";

#else
        true;

#endif

    public object Visit(Block stmt)
    {
        ExecuteBlock(stmt.Statements, new Environment(environment));

        return null;
    }

    public object Visit(Class stmt)
    {
        object? superclass = null;
        stmt.IfSuperclass(() =>
        {
            superclass = EvaluateExpression(stmt.Superclass!);
            if (superclass is not LoxClass)
            {
                throw new RuntimeException(stmt.Superclass!.Name, "Inherited classes must also be classes.");
            }
        });

        environment.Define(stmt.Name.Lexeme, null);
        stmt.IfSuperclass(() =>
        {
            environment = new(environment);
            environment.Define("super", superclass);
        });

        Dictionary<string, LoxFunction> meathods= new();
        foreach (var meathod in stmt.Meathods)
        {
            LoxFunction function = new(meathod, environment,
          meathod.Name.Lexeme=="init");
            meathods.Add(meathod.Name.Lexeme, function);
        }

        environment.Assign(stmt.Name, new LoxClass(stmt.Name.Lexeme, superclass as LoxClass, meathods));
        stmt.IfSuperclass(() => environment = environment.Enclosing!);
        return null;
    }

    public object Visit(Del stmt)
    {
        foreach (Token to in stmt.Variables)
        {
            environment.Delete(to.Lexeme);
        }
        return null;
    }

    public object Visit(Function stmt)
    {
        environment.Define(stmt.Name.Lexeme, new LoxFunction(stmt, environment, false));
        return null;
    }

    public object Visit(If stmt)
    {
        if (IsTruthy(EvaluateExpression(stmt.Condition)))
        {
            Execute(stmt.ThenBranch);
        }
        else if (stmt.ElseBranch is not null)
        {
            Execute(stmt.ElseBranch);
        }
        return null; // <-- this might have interesting syntax quirks... returning an if statement as 0?! Woah...
    }

    public object Visit(Print stmt)
    {
        Console.Out.WriteLine(string.Join(',', stmt.Expressions.Select(statement => Stringify(EvaluateExpression(statement)))));
        return NOTHING;
    }

    public object Visit(Return stmt)
    {
        object? value = null;
        if (stmt.Expression is not null)
        {
            value = EvaluateExpression(stmt.Expression);
        }
        throw new ReturnException(value);
    }

    public object Visit(StmtExpression stmt)
    {
        return EvaluateExpression(stmt.Expression);
    }

    public object Visit(Var stmt)
    {
        object? value = null;
        if (stmt.Initilizer is not null)
        {
            value = EvaluateExpression(stmt.Initilizer);
        }

        environment.Define(stmt.Name.Lexeme, value);
        return $"var {stmt.Name.Lexeme}: {value}";
    }

    public object Visit(Break stmt)
    {
        throw new BreakException();
    }

    public object Visit(Continue stmt) => throw new ContinueException();

    public object Visit(While stmt)
    {
#if DEBUG
        DateTime dt = DateTime.Now;
#endif
        while (IsTruthy(EvaluateExpression(stmt.Condition)))
        {
#if DEBUG
            if ((DateTime.Now - dt).TotalMilliseconds > 50000)
            {
                LoxRunner.Error(int.MaxValue, int.MaxValue, "Timeout Error", "A loop lasted longer then alloted time (12000 ms)", "while");

#pragma warning disable S1215 // "GC.Collect" should not be called
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true, true);
                throw new RuntimeException(new Token(EOF, "while", null, int.MaxValue, int.MaxValue), "Program halted due to a loop being too slow (infinite?!)");
            }
#endif
            try
            {
                Execute(stmt.Body);
            }
            catch (BreakException)
            {
                break;
            }
            catch (ContinueException)
            {
                // ...
            }
        }
        return null;
    }
}
