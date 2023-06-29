using System.Text;
using Lox.Expressions;
using Lox.Statements;
using Lox.Tokens;

namespace Lox.Interpreting;

public partial class Interpreter : Expressions.IVisitor<object>, Statements.IVisitor<object>
{
    private static readonly Environment globals = new();
    private Environment environment = globals;
    private static readonly Dictionary<Expr, int?> locals = new();

    private static bool IsTruthy(object? obj)
    {
        if (obj is null) return false;
        if (obj is bool b) return b;
        if (obj is double d && (d == 0)) return false;
        return true;
    }

    public string Interpret(IList<Stmt> statements)
    {
        StringBuilder sb = new();
        foreach (Stmt statement in statements)
        {
            object? value = Execute(statement);

            if (value is bool b && b)
            {
                break;     // something terrible happened!
            }
            else
            {
                sb.AppendLine(value?.ToString());
            }
        }
        return sb.ToString();
    }

    public object? EvaluateExpression(Expr expr) => expr?.AcceptVisitor(this) ?? null;

    private (string, int) lastError = ("", 0);

    public object? Execute(Stmt stmt)
    {
        try
        {
            return stmt.AcceptVisitor(this);
        }
        //overkill?
        catch (RuntimeException error)
        {
            if (10000 < lastError.Item2)
            {
                LoxRunner.Error(error.Token.Line, error.Token.Column, "No-Good-Very-Bad Error",
                    lastError.Item1, $"[{lastError.Item2} similar errors have been collated]\nThis is presumably an infinite or very large loop, so Interpreting has halted.");
                return true;
            }
            if (lastError.Item1 == error.Message)
            {
                lastError = (error.Message, ++lastError.Item2);

                if (lastError.Item2 % CollationMod() == 0)
                {
                    LoxRunner.Error(error.Token.Line, error.Token.Column, "Runtime Error",
                    lastError.Item1, $"[{lastError.Item2} similar errors have been collated]");
                }
            }
            else
            {
                lastError = (error.Message, 1);
                LoxRunner.Error(error.Token.Line, error.Token.Column, "Runtime Error",
                error.Message, error.Token.Lexeme);
            }
        }

        return null;
    }

    private int CollationMod() => lastError.Item2 switch
    {
        < 5 => 1,
        < 25 => 5,
        < 100 => 10,
        < 500 => 100,
        < 10000 => 1000,
        _ => int.MaxValue
    };

    internal void ExecuteBlock(IList<Stmt> statements, Environment environment)
    {
        Environment previous = this.environment;
        try
        {
            this.environment = environment;

            foreach (var statement in statements)
            {
                Execute(statement);
            }
        }
        finally
        {
            this.environment = previous;
        }
    }

    internal void Resolve(Expr expr, int depth)
    {
        locals[expr] = depth;
    }

    internal static object Throw(Token token, string message) => throw new RuntimeException(token, message);

    private string BuildArrayRepr(IList<object> objects)
    {
        System.Text.StringBuilder sb = new();
        sb.Append('[');
        bool isFirst = true;

        for (int i = 0; i < objects.Count; i++)
        {
            object? obj = objects[i];
            if (isFirst)
            {
                isFirst = false;
            }
            else
            {
                sb.Append(", ");
            }

            switch (obj)
            {
                case Literal l:
                    sb.Append(l.Value?.ToString() ?? "null"); break;
                case List list:
                    sb.Append(BuildArrayRepr(list.Elements.ToArray()));
                    break;

                case Expr expr:
                    sb.Append(EvaluateExpression(expr)); break;
                default:
                    sb.Append(obj.ToString()); break;
            }
        }
        sb.Append(']');
        return sb.ToString();
    }

    public void Clear()
    {
        environment.Clear();
    }

#if DEBUG

    private bool GetInfo(Expr? expression)
    {
        if (expression is null)
        {
            return false;
        }

        string str = $@"
     |------------------------------------
     |{expression}: {expression.GetType()}
     |{expression} as string: {expression}
     |{expression} evaluated: {EvaluateExpression(expression)}
     |------------------------------------
     Internal Properties:
     ";
        Console.WriteLine(str);
        EvaluateExpression(expression)?.GetType().GetProperties()?.ToList().ForEach(Console.WriteLine);
        return true;
    }

#endif
}
