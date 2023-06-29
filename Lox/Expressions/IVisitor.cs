namespace Lox.Expressions;

internal interface IVisitor<out T>
{
    T Visit(Assign expr);

    T Visit(Binary expr);

    T Visit(Func expr);

    T Visit(Getter expr);

    T Visit(Grouping expr);

    T Visit(Literal expr);

    T Visit(List expr);

    T Visit(Logical expr);

    T Visit(Setter expr);

    T Visit(Super expr);

    T Visit(This expr);

    T Visit(Ternary expr);

    T Visit(Unary expr);

    T Visit(Variable expr);
}
