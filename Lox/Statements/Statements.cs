#pragma warning disable CA1716 // Identifiers should not match keywords

using Lox.Expressions;
using Lox.Tokens;

namespace Lox.Statements;

public class Assert(Expr condition, Token keyword, Expr message) : Stmt
{
    public Expr Condition => condition;
    public Token Keyword => keyword;
    public Expr Message => message;

    internal override T AcceptVisitor<T>(IVisitor<T> visitor) => visitor.Visit(this);
}

public class Block(IList<Stmt> statements) : Stmt
{
    public IList<Stmt> Statements => statements;

    internal override T AcceptVisitor<T>(IVisitor<T> visitor) => visitor.Visit(this);
}

public class Class(Token name, Variable? superclass, IList<Function> meathods) : Stmt
{
    public Token Name => name;
    public Variable? Superclass => superclass;
    public IList<Function> Meathods => meathods;

    internal override T AcceptVisitor<T>(IVisitor<T> visitor) => visitor.Visit(this);
}

public class Del(IList<Token> variables) : Stmt
{
    public IList<Token> Variables => variables;

    internal override T AcceptVisitor<T>(IVisitor<T> visitor) => visitor.Visit(this);
}

public class Function(Token name, IList<Token> parameters, Block body) : Stmt
{
    public Token Name => name;
    public IList<Token> Parameters => parameters;
    public Block Body => body;

    internal override T AcceptVisitor<T>(IVisitor<T> visitor) => visitor.Visit(this);
}

public class If(Expr condition, Stmt thenBranch, Stmt? elseBranch) : Stmt
{
    public Expr Condition => condition;
    public Stmt ThenBranch => thenBranch;
    public Stmt? ElseBranch => elseBranch;

    internal override T AcceptVisitor<T>(IVisitor<T> visitor) => visitor.Visit(this);
}

public class While(Expr condition, Stmt body) : Stmt
{
    public Expr Condition => condition;
    public Stmt Body => body;

    internal override T AcceptVisitor<T>(IVisitor<T> visitor) => visitor.Visit(this);
}

public class Print(IList<Expr> expressions) : Stmt
{
    public IList<Expr> Expressions => expressions;

    internal override T AcceptVisitor<T>(IVisitor<T> visitor) => visitor.Visit(this);
}

public class Return(Token keyword, Expr? expression) : Stmt
{
    public Token Keyword => keyword;
    public Expr? Expression => expression;

    internal override T AcceptVisitor<T>(IVisitor<T> visitor) => visitor.Visit(this);
}

public class StmtExpression(Expr expression) : Stmt
{
    public Expr Expression => expression;

    internal override T AcceptVisitor<T>(IVisitor<T> visitor) => visitor.Visit(this);
}

public class Var(Token name, Expr? initilizer) : Stmt
{
    public Token Name => name;
    public Expr? Initilizer => initilizer;

    internal override T AcceptVisitor<T>(IVisitor<T> visitor) => visitor.Visit(this);
}

public class Break(Token keyword) : Stmt
{
    public Token Keyword => keyword;

    internal override T AcceptVisitor<T>(IVisitor<T> visitor) => visitor.Visit(this);
}

public class Continue : Stmt
{
    internal override T AcceptVisitor<T>(IVisitor<T> visitor) => visitor.Visit(this);
}

#pragma warning restore CA1716 // Identifiers should not match keywords
