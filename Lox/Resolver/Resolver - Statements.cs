using System.Xml.Linq;
using Lox.Statements;
using Lox.Tokens;
using static Lox.Extras.Utils;

namespace Lox.Resolver;

internal sealed partial class Resolver
{
    private enum FunctionType
    { FUNCTION, MEATHOD, INIT, NOPE };

    private enum ClassType
    {
        None,
        Class,
        SUBCLASS
    }

    private FunctionType currentFunction = FunctionType.NOPE;
    private ClassType currentClass = ClassType.None;

    public object Visit(Assert stmt)
    {
        Resolve(stmt.Condition);
        return NOTHING;
    }

    public object Visit(Break stmt)
    {
        return NOTHING;
    }

    public object Visit(Block stmt)
    {
        scopes.Push(new Dictionary<string, bool>());
        Resolve(stmt.Statements);
        scopes.Pop();
        return NOTHING;
    }

    public object Visit(Class stmt)
    {
        var enclosingClass = currentClass;
        currentClass = ClassType.Class;

        Declare(stmt.Name);
        Define(stmt.Name);

        // if bug, split the resolve and if statement.
        stmt.IfSuperclass(() =>
        {
            currentClass = ClassType.SUBCLASS;
            Resolve(stmt.Superclass!);
            scopes.Push(new Dictionary<string, bool>());
            scopes.Peek()["super"] = true;
            if (stmt.Name.Lexeme == stmt.Superclass!.Name.Lexeme)
            {
                LoxRunner.Error(stmt.Name.Line, stmt.Name.Column, "Syntax Error", "A class cannot inherit itself.", stmt.Superclass.Name.Lexeme);
            }
        });
        scopes.Push(new Dictionary<string, bool>());
        scopes.Peek()["this"] = true;
        // gee, maybe that ResolveFunction was a good idea...
        foreach (var method in stmt.Meathods)
        {
            var decl = FunctionType.MEATHOD;
            if (method.Name.Lexeme == "init")
            {
                decl = FunctionType.INIT;
            }
            var expression = new Function(method.Name, method.Parameters, method.Body);
            //---
            FunctionType enclosingFunction = currentFunction;
            currentFunction = decl;
            scopes.Push(new Dictionary<string, bool>());
            foreach (Token param in expression.Parameters)
            {
                Declare(param);
                Define(param);
            }
            Resolve(expression.Body.Statements);
            scopes.Pop();
            stmt.IfSuperclass(() => scopes.Push(new Dictionary<string, bool>()));
            currentFunction = enclosingFunction;
            //---
        }

        scopes.Pop();
        currentClass = enclosingClass;
        return NOTHING;
    }

    public object Visit(Continue stmt)
    {
        return NOTHING;
    }

    public object Visit(Del stmt)
    {
        foreach (var variable in stmt.Variables)
        {
            Undefine(variable);
        }
        return NOTHING;
    }

    public object Visit(Function stmt)
    {
        Declare(stmt.Name);
        Define(stmt.Name);
        FunctionType enclosingFunction = currentFunction;
        currentFunction = FunctionType.FUNCTION;
        scopes.Push(new Dictionary<string, bool>());

        foreach (var item in stmt.Parameters)
        {
            Declare(item);
            Define(item);
        }
        Resolve(stmt.Body.Statements);
        scopes.Pop();
        currentFunction = enclosingFunction;
        return NOTHING;
    }

    public object Visit(If stmt)
    {
        Resolve(stmt.Condition);
        Resolve(stmt.ThenBranch);
        if (stmt.ElseBranch is not null) Resolve(stmt.ElseBranch);
        return NOTHING;
    }

    public object Visit(Print stmt)
    {
        Resolve(stmt.Expressions);
        return NOTHING;
    }

    public object Visit(Return stmt)
    {
        if (currentFunction == FunctionType.NOPE)
        {
            LoxRunner.Error(stmt.Keyword.Line, stmt.Keyword.Column, "Syntax Error", "'return' outside of function.", stmt.Keyword.Lexeme);
        }
        if (stmt.Expression is not null)
        {
            if (currentFunction == FunctionType.INIT)
            {
                LoxRunner.Error(stmt.Keyword.Line, stmt.Keyword.Column, "Syntax Error", "Cannot return a value from init()", stmt.Keyword.Lexeme);
            }
            Resolve(stmt.Expression);
        }
        return NOTHING;
    }

    public object Visit(Var stmt)
    {
        Declare(stmt.Name);
        if (stmt.Initilizer is not null)
        {
            Resolve(stmt.Initilizer);
        }
        Define(stmt.Name);
        return NOTHING;
    }

    public object Visit(While stmt)
    {
        Resolve(stmt.Condition);
        Resolve(stmt.Body);
        return NOTHING;
    }

    public object Visit(StmtExpression stmt)
    {
        Resolve(stmt.Expression);
        return NOTHING;
    }

    private void Declare(Token name)
    {
        if (scopes.Count == 0)
        {
            return;
        }
        Dictionary<string, bool> scope = scopes.Peek();
        if (scope.ContainsKey(name.Lexeme))
        {
            LoxRunner.Error(name.Line, name.Column, "Name Error", $"Already named a variable or function \"{name.Lexeme}\" in the same scope", name.Lexeme);
        }
        scope.Add(name.Lexeme, false);
    }

    private void Define(Token name)
    {
        if (scopes.Count != 0)
        {
            scopes.Peek()[name.Lexeme] = true;
        }
    }

    private void Undefine(Token name)
    {
        if (scopes.Count != 0) scopes.Peek().Remove(name.Lexeme);
    }
}
