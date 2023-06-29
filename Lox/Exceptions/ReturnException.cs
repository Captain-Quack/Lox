namespace Lox.Exceptions;

#pragma warning disable CA1032

public class ReturnException(object? value) : Exception
{
    public object? Value => value;
}
