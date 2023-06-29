using System.Diagnostics;
using System.Text;
using Lox.Interpreting;
using Lox.Statements;
using Lox.Tokens;

namespace Lox;

public class LoxRunner
{
    private static readonly Interpreter Interpreter = new();

    public static bool HadError { get; set; }

    private static string DashLine() => string.Concat(Enumerable.Repeat('-', Console.WindowWidth));

    public void Main(string[] args)
    {
        if (args is null)
        {
            System.Environment.Exit(666);
            return;
        }
        Console.CancelKeyPress += (sender, e) =>
        {
            System.Environment.Exit(0);
        };

        if (args.Length > 1)
        {
            Error(-1, 4, "IoError", "correct usage: lox [script file]", "lox...");
            return;
        }

        if (args.Length == 1)
        {
            RunScript(args[0]);
        }
        else
        {
            RunPrompt();
        }
    }

    public void RunScript(string filename)
    {
        if (string.IsNullOrEmpty(filename))
        {
            Error(0, 0, "IOError", "File name cannot be empty.", filename);
            return;
        }

        if (!File.Exists(filename))
        {
            Error(0, 0, "IOError", $"{filename} does not exist", filename);
            return;
        }

        try
        {
            string code = File.ReadAllText(filename);
            Debug.Assert(code is not null);
            Execute(code);
        }
        catch (UnauthorizedAccessException)
        {
            Error(0, 0, "IOError", $"You do not have permission to open {filename}", filename);
        }
        catch (IOException)
        {
            Error(0, 0, "IOError", $"An IO Error occurred when trying to open {filename}", filename);
        }
        catch
        {
            Error(0, 0, "IOError", $"An unknown error occurred while opening {filename}", filename);
            throw;
        }
    }

    /// <summary>
    /// Repl
    /// </summary>
    ///
    public void RunPrompt()
    {
        const string QuitCommand = "quit";
        const string ClearCommand = "clear";
        const int MaxBraceCount = byte.MaxValue;
        const string PrompterDefault = "> ";
        const string PrompterContinuation = "   ";
        StringBuilder command = new();
        int braceCount = 0;
        int line = 0;
        string prompter = PrompterDefault;

        Console.Title = "Lox Interactive";
        while (true)
        {
            HadError = false;
            Console.Write(prompter);
            string? voxCode = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(voxCode))
            {
                continue;
            }

            if (voxCode == QuitCommand)
            {
                break;
            }

            if (voxCode == ClearCommand)
            {
                (line, prompter) = (0, PrompterDefault);
                command.Clear();
                Interpreter.Clear();
                Console.Clear();
                continue;
            }

            braceCount += voxCode.Count(c => c == '{');
            braceCount -= voxCode.Count(c => c == '}');

            if (braceCount > MaxBraceCount)
            {
                Error(line, 0, "Thats-Dumb-Error", "Please organize your code better as to not have a brace depth of over 250.", "{");
                break;
            }

            if (braceCount < 0)
            {
                Error(line, 0, "Syntax Error", "Unmatched '}'", "}");
                (line, prompter) = (0, PrompterDefault);
                command.Clear();
                continue;
            }

            command.Append(voxCode);

            if (braceCount == 0)
            {
                (line, prompter) = (0, PrompterDefault);
                string commandStr = command.ToString().Trim();
                if (!string.IsNullOrEmpty(commandStr))
                {
                    switch (commandStr[^1])
                    {
                        case ';' or '}':
                            Execute(commandStr); break;
                        default:
                            Execute(commandStr + ";"); break;
                    }
                }
                command.Clear();
            }
            else
            {
                prompter = PrompterContinuation;
            }

            line++;
        }
    }

    internal object? Execute(string source, bool ret = false)
    {
        Debug.Assert(source is not null);
        Scanning.Scanner scanner = new(source);
        var tokens = scanner.ScanTokens();
        if (HadError) return null;

        var parser = new Parser.Parser(tokens.ToList());
        List<Stmt> statements = parser.Parse();
        if (HadError) return null;

        Resolver.Resolver resolver = new(Interpreter);
        resolver.Resolve(statements);
        if (HadError) return null;

        if (ret) return Interpreter.Interpret(statements).ToString();
        Console.Write(Interpreter.Interpret(statements).ToString());
        return null;
    }

    public static void Error(int line, int column, string label, string message, string lexeme, string? collated = "")
    {
        HadError = true;
        string formattedError = $@"
An Error Occurred.
{DashLine()}

{label}: {message}
 | {collated}
 | Line {line - 1}, Column {column}
 | : '{(lexeme)}'
{DashLine()}
";
        Console.Error.WriteLine(formattedError);
    }
}
