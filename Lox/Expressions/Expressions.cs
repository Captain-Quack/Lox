using Lox.Tokens;

namespace Lox.Expressions;

public record class Assign(Token Name, Expr Value) : Expr
{
    internal override T AcceptVisitor<T>(IVisitor<T> visitor) => visitor.Visit(this);
}

#pragma warning disable CA1724 // Type names should not match namespaces
public record class Binary(Expr Left, Token Op, Expr Right) : Expr
#pragma warning restore CA1724 // Type names should not match namespaces
{
    internal override T AcceptVisitor<T>(IVisitor<T> visitor) => visitor.Visit(this);
}

public record class Func(Expr Callee, Token RightParen, IList<Expr> Args) : Expr
{
    internal override T AcceptVisitor<T>(IVisitor<T> visitor) => visitor.Visit(this);
}

public record class Getter(Expr GetFrom, Token Name) : Expr
{
    internal override T AcceptVisitor<T>(IVisitor<T> visitor) => visitor.Visit(this);
}

public record class Grouping(Expr Expression) : Expr
{
    internal override T AcceptVisitor<T>(IVisitor<T> visitor) => visitor.Visit(this);
}

public record class Literal(object? Value) : Expr
{
    internal override T AcceptVisitor<T>(IVisitor<T> visitor) => visitor.Visit(this);
}

public record class List(IList<Expr> Elements) : Expr
{
    internal override T AcceptVisitor<T>(IVisitor<T> visitor) => visitor.Visit(this);
}

public record class Logical(Expr Left, Token Op, Expr Right) : Expr
{
    internal override T AcceptVisitor<T>(IVisitor<T> visitor) => visitor.Visit(this);
}

public record class Setter(Expr Obj, Token Name, Expr Value) : Expr
{
    internal override T AcceptVisitor<T>(IVisitor<T> visitor) => visitor.Visit(this);
}

public record class Super(Token Keyword, Token Obj) : Expr
{
    internal override T AcceptVisitor<T>(IVisitor<T> visitor) => visitor.Visit(this);
}

public record class Ternary(Expr Conditional, Expr IfTrue, Expr IfFalse) : Expr
{
    internal override T AcceptVisitor<T>(IVisitor<T> visitor) => visitor.Visit(this);
}

/// <param name="Keyword">"this"</param>
public record class This(Token Keyword) : Expr
{
    internal override T AcceptVisitor<T>(IVisitor<T> visitor) => visitor.Visit(this);
}

public record class Unary(Token Op, Expr Expression) : Expr
{
    internal override T AcceptVisitor<T>(IVisitor<T> visitor) => visitor.Visit(this);
}

public record class Variable(Token Name) : Expr
{
    internal override T AcceptVisitor<T>(IVisitor<T> visitor) => visitor.Visit(this);
}
