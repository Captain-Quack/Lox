using Lox.Expressions;
using Lox.Extras;
using Lox.Statements;
using Lox.Interpreting;

namespace Lox.Resolver;

internal partial class Resolver : Expressions.IVisitor<object?>, Statements.IVisitor<object>
{
    private readonly Stack<Dictionary<string, bool>> scopes = new();
    private readonly Interpreter interpreter;

    internal Resolver(Interpreter interpreter)
    {
        this.interpreter = interpreter;
    }

    public void Resolve(IList<Stmt> stmts)
    {
        foreach (var stmt in stmts)
        {
            Resolve(stmt);
        }
    }

    public void Resolve(IList<Expr> exprs)
    {
        foreach (var expr in exprs)
        {
            Resolve(expr);
        }
    }

    public void Resolve(Stmt stmt)
    {
        stmt.AcceptVisitor(this);
    }

    public void Resolve(Expr expr)
    {
        expr.AcceptVisitor(this);
    }
}
