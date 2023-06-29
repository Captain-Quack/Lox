using Lox.Tokens;
using static Lox.Extras.Utils;

namespace Lox.Interpreting;

internal sealed class Environment
{
    internal readonly Environment? Enclosing;
    private readonly Dictionary<string, object?> values = new();

    internal Environment()
    {
        Enclosing = null;
    }

    internal Environment(Environment enclosing)
    {
        Enclosing = enclosing;
    }

    internal void Define(string name, object? value)
    {
        values[name] = value;
    }

    internal void Delete(string name)
    {
        values.Remove(name);
    }

    internal void Clear()
    {
        values.Clear();
    }

    internal Dictionary<string, object?> PeekValues()
    {
        return values;
    }

    internal object? GetAt(int distance, string name)
    {
#if DEBUG
#pragma warning disable CA1303 // Do not pass literals as localized parameters
        Console.WriteLine("Debug:");
#pragma warning restore CA1303 // Do not pass literals as localized parameters
        PrintDictionary(Ancestor(distance)!.values);

#endif

        return Ancestor(distance)!.values[name];
    }

    internal void AssignAt(int distance, Token name, object? value)
    {
        Ancestor(distance)!.values[name.Lexeme] = value;
    }

    public Environment? Ancestor(int distance)
    {
        var environment = this;
        for (int i = 0; i < distance; i++)
        {
            environment = environment?.Enclosing;
        }
        return environment;
    }

    public object? Get(Token name)
    {
        if (values.ContainsKey(name.Lexeme) && values.TryGetValue(name.Lexeme, out object? value))
            return value;
        if (Enclosing is not null)
            return Enclosing.Get(name);

        throw new RuntimeException(name, $"Undefined variable {name.Lexeme}.");
    }

    public void Assign(Token name, object? value)
    {
        if (values.ContainsKey(name.Lexeme))
        {
            values[name.Lexeme] = value;
            return;
        }

        if (Enclosing != null)
        {
            Enclosing.Assign(name, value);
            return;
        }

        // If the variable is not defined in the current or any enclosing environment, define it in the current environment
        throw new RuntimeException(name, "Undefined variable '" + name.Lexeme + "'.");
    }

    public static void PrintDictionary(Dictionary<string, object?> dictionary)
    {
#pragma warning disable CA1303 // Do not pass literals as localized parameters
        Console.WriteLine("------------------------");
        Console.WriteLine("|   Key   |   Value    |");
        Console.WriteLine("------------------------");

        foreach (KeyValuePair<string, object?> pair in dictionary)
        {
            Console.WriteLine($"| {pair.Key,-7} | {Stringify(pair.Value),-9} |");
        }

        Console.WriteLine("------------------------");
#pragma warning restore CA1303 // Do not pass literals as localized parameters
    }
}
