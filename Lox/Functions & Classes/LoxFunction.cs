using Lox.Exceptions;
using Lox.Interpreting;
using Lox.Statements;
using Environment = Lox.Interpreting.Environment;

namespace Lox.Functions;

internal sealed record class LoxFunction : LoxCallable
{
    private Function Decl { get; init; }
    private Environment Closure { get; init; }
    private bool IsInit { get; init; }
    public override int Arity { get; init; }

    internal LoxFunction(Function declaration, Environment closure, bool isinit)
    {
        Decl = declaration;
        Closure = closure;
        IsInit = isinit;
        Arity = Decl.Parameters.Count;
    }

    public override object FunctionCall(Interpreter interpreter, IList<object?> arguments)
    {
        Environment environment = new(Closure);

        for (int i = 0; i < Decl.Parameters.Count; i++)
        {
            environment.Define(Decl.Parameters[i].Lexeme, arguments[i]);
        }

        try
        {
            interpreter.ExecuteBlock(Decl.Body.Statements, environment);
        }
        catch (ReturnException returnValue)
        {
            if (IsInit) return Closure.GetAt(0, "this")!;

            return returnValue.Value!;
        }

        // For a constructor, return the instance 'this'
        if (IsInit) return Closure.GetAt(0, "this")!;

#pragma warning disable CS8603 // Possible null reference return.
        return null;
#pragma warning restore CS8603 // Possible null reference return.
    }

    public LoxFunction Bind(LoxInstance instance)
    {
        Environment environment = new(Closure);
        environment.Define("this", instance);
        return new LoxFunction(Decl, environment, IsInit);
    }
    public override string ToString()
    {
        return $"<function {Decl.Name.Lexeme}>";
    }
}
