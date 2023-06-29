using System.Threading;
using System;
using Lox.Functions;
using Lox.Statements;
using Lox.Scanning;

namespace Lox.Interpreting;

public partial class Interpreter
{
    static Interpreter()
    {
        globals.Define("clock", new NaitiveFunction(0, (_, _) =>
        {
            return DateTimeOffset.Now.ToUnixTimeMilliseconds() / 1000.0;
        }));

        globals.Define("random", new NaitiveFunction(0, (_, _) =>
        {
            return new Random().NextDouble();
        }));
        globals.Define("str", new NaitiveFunction(1, (_, args) =>
        {
            return args[0]?.ToString() ?? "null";
        }));
        globals.Define("defined", new NaitiveFunction(1, (interpreter, args) =>
        {
            return interpreter.environment.PeekValues().ContainsKey(args[0]?.ToString() ?? "");
        }));
        globals.Define("exec", new NaitiveFunction(1, (_, args) =>
        {
            try
            {
                return new LoxRunner().Execute(args[0]?.ToString() ?? "", true) ?? "null";
            }
            catch (Exception ex) when (ex is ParseException or RuntimeException)
            {
                Console.WriteLine($"== Previous error occurred in exec statement ==");
            }
            return "null";
        }));
    }
}
