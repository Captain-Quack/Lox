using System.Linq.Expressions;
using Lox.Expressions;
using Lox.Tokens;
using static Lox.Extras.Utils;

namespace Lox.Resolver;

internal partial class Resolver
{
    public object? Visit(Assign expr)
    {
        Resolve(expr.Value);
        ResolveLocal(expr, expr.Name);
        return null;
    }

    public object? Visit(Binary expr)
    {
        Resolve(expr.Left);
        Resolve(expr.Right);
        return null;
    }

    public object? Visit(Func expr)
    {
        Resolve(expr.Callee);
        foreach (var argument in expr.Args)
        {
            Resolve(argument);
        }
        return null;
    }

    public object? Visit(Getter expr)
    {
        Resolve(expr.GetFrom);
        return null;
    }

    public object? Visit(Grouping expr)
    {
        Resolve(expr.Expression);
        return null;
    }

    public object? Visit(Literal expr)
    {
        return null;
    }

    public object? Visit(List expr)
    {
        Resolve(expr.Elements);
        return null;
    }

    public object? Visit(Logical expr)
    {
        Resolve(expr.Left);
        Resolve(expr.Right);
        return null;
    }

    public object? Visit(Setter expr)
    {
        Resolve(expr.Value);
        Resolve(expr.Obj);
        return null;
    }

    public object? Visit(Super expr)
    {
        ResolveLocal(expr, expr.Keyword);
        if (currentClass == ClassType.None)
        {
            LoxRunner.Error(expr.Keyword.Line, expr.Keyword.Column, "Syntax Error", "Tried to use super out of a class", expr.Keyword.Lexeme);
        }
        else if (currentClass == ClassType.Class)
        {
            LoxRunner.Error(expr.Keyword.Line, expr.Keyword.Column, "Syntax Error", "Tried to use super in a class with no superclass.", expr.Keyword.Lexeme);
        }
        return null;
    }

    public object? Visit(This expr)
    {
        if (currentClass == ClassType.None)
        {
            LoxRunner.Error(expr.Keyword.Line, expr.Keyword.Column, "Syntax Error", "Cannot use 'this' outside of a class", "this");
            return null;
        }
        ResolveLocal(expr, expr.Keyword);
        return null;
    }

    public object? Visit(Ternary expr)
    {
        Resolve(expr.IfTrue); Resolve(expr.IfFalse); Resolve(expr.Conditional);
        return null;
    }

    public object? Visit(Unary expr)
    {
        Resolve(expr.Expression);
        return null;
    }

    public object? Visit(Variable expr)
    {
        var nameToken = expr.Name;
        if (scopes.Count != 0 && scopes.Peek().TryGetValue(expr.Name.Lexeme, out bool isDefined) && !isDefined)
        {
            LoxRunner.Error(nameToken.Line, nameToken.Column, "Syntax Error", "Cannot access local variable 'a' where it is not associated with a value", nameToken.Lexeme);
        }
        ResolveLocal(expr, expr.Name);
        return null;
    }

    private void ResolveLocal(Expr expr, Token name)
    {
        if (scopes.Count == 0) return;
        var stackList = scopes.Reverse().ToList();
        for (int i = stackList.Count - 1; i >= 0; i--)
        {
            if (stackList[i].ContainsKey(name.Lexeme))
            {
                interpreter.Resolve(expr, stackList.Count - 1 - i);
                return;
            }
        }
    }
}
