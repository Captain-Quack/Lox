using Lox.Interpreting;

namespace Lox.Functions;

internal abstract record LoxCallable
{
    public abstract int Arity { get; init; }
    public abstract object? FunctionCall(Interpreter interpreter, IList<object?> arguments);
    public override string ToString()
    {
        return "<Function>";
    }
}

internal sealed record NaitiveFunction : LoxCallable
{
    public override int Arity { get; init; }
    private readonly Func<Interpreter, IList<object?>, object> _function;
    public override object FunctionCall(Interpreter interpreter, IList<object?> arguments)
    {
        return _function(interpreter, arguments);
    }
    internal NaitiveFunction(int arity, Func<Interpreter, IList<object?>, object> funcCall)
    {
        Arity = arity;
        _function = funcCall;
    }
    public override string ToString()
    {
        return "<Naitive Function>";
    }
}
