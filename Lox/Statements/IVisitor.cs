namespace Lox.Statements;

internal interface IVisitor<out T>
{
    T Visit(Assert stmt);

    T Visit(Break stmt);

    T Visit(Block stmt);

    T Visit(Class stmt);

    T Visit(Continue stmt);

    T Visit(Del stmt);

    T Visit(Function stmt);

    T Visit(If stmt);

    T Visit(Print stmt);

    T Visit(Return stmt);

    T Visit(Var stmt);

    T Visit(While stmt);

    T Visit(StmtExpression stmt);
}
