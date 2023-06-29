namespace Lox.Statements;

public abstract partial class Stmt
{
    internal abstract T AcceptVisitor<T>(IVisitor<T> visitor);
}
