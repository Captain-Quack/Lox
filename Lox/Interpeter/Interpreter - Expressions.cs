using Lox.Expressions;
using Lox.Functions;
using static Lox.Extras.Utils;
using static Lox.Tokens.TokenType;
using Lox.Tokens;
using System.Data.SqlTypes;
using System.Linq.Expressions;
using Lox.Statements;
using System;

namespace Lox.Interpreting;

#pragma warning disable CS8603 // Possible null reference return.

public partial class Interpreter
{
    public object Visit(List expr)
    {
        var evaluated = new object[expr.Elements.Count];
        Parallel.For(0, expr.Elements.Count, i =>
        {
            evaluated[i] = expr.Elements[i];
        });
        return evaluated;
    }

    public object Visit(Assign expr)
    {
        object? value = EvaluateExpression(expr.Value);
        int? distance = null;

        if (locals.ContainsKey(expr))
        {
            distance = locals[expr];
        }

        if (distance is not null)
        {
            environment.AssignAt(distance.Value, expr.Name, value);
        }
        else
        {
            globals.Assign(expr.Name, value);
        }

        return value;
    }

    public object Visit(Binary expr)
    {
        var left = EvaluateExpression(expr.Left);
        var right = EvaluateExpression(expr.Right);

        double leftDouble = 0, rightDouble = 0;
        bool isLeftDouble = false, isRightDouble = false;

        if (left is double ld)
        {
            leftDouble = ld;
            isLeftDouble = true;
        }
        if (right is double rt)
        {
            rightDouble = rt;
            isRightDouble = true;
        }
        double Check(Func<double> math, string operation)
        {
            try
            {
                return checked(math.Invoke());
            }
            catch (OverflowException)
            {
                throw new RuntimeException(expr.Op, $"Overflow occurred when performing {operation}");
            }
            catch (InvalidOperationException exception)
            {
                throw new RuntimeException(expr.Op, exception.Message ?? $"Both operands for {expr.Op.Line} must be numbers (received {SoftType(expr.Left)} and {SoftType(expr.Right)}.)");
            }
            catch
            {
                throw new RuntimeException(expr.Op, $"An error occurred while trying to perform {operation}");
            }
        }

        return expr.Op.Type switch
        {
            EQUALEQUAL => Equals(left, right),
            BANGEQUAL => !Equals(left, right),

            // Comparison
            GREATEREQUAL => isLeftDouble && isRightDouble ? leftDouble >= rightDouble : Throw(expr.Op, "Both operands must be numbers."),
            LESSEQUAL => isLeftDouble && isRightDouble ? leftDouble <= rightDouble : Throw(expr.Op, "Both operands must be numbers."),
            GREATER => isLeftDouble && isRightDouble ? leftDouble > rightDouble : Throw(expr.Op, "Both operands must be numbers."),
            LESS => isLeftDouble && isRightDouble ? leftDouble < rightDouble : Throw(expr.Op, "Both operands must be numbers."),
            // BIT,
            LSHIFT => isLeftDouble && isRightDouble ? ( double )(( int )leftDouble << ( int )rightDouble) : Throw(expr.Op, "Both operands must be numbers."),
            RSHIFT => isLeftDouble && isRightDouble ? ( int )leftDouble >> ( int )rightDouble : Throw(expr.Op, "Both operands must be numbers."),

            // Math
            CARROT => isLeftDouble && isRightDouble ? Check(() => Pow(leftDouble, rightDouble), "exponentiation") : Throw(expr.Op, "Both operands must be numbers."),
            SLASH => isLeftDouble && isRightDouble ? Check(() => leftDouble / rightDouble, "division") : Throw(expr.Op, "Both operands must be numbers."),
            MINUS => isLeftDouble && isRightDouble ? Check(() => leftDouble - rightDouble, "subtraction") : Throw(expr.Op, "Both operands must be numbers."),
            PERCENT => isLeftDouble && isRightDouble ? Check(() => leftDouble % rightDouble, "modulus") : Throw(expr.Op, "Both operands must be numbers."),
            STAR => (left, right) switch
            {
                (double l, double r) => Check(() => l * r, "multiplication"),
                (string l, double r) => string.Concat(Enumerable.Repeat(l, ( int )r)),
                (object[] l, double r) => l.SelectMany(item => Enumerable.Repeat(item, checked(( int )r))).ToArray(),
                _ => throw new RuntimeException(expr.Op, "Invalid use of '*' operator")
            },
            PLUS => (left, right) switch
            {
                (double l, double r) => Check(() => l + r, "addition"),
                (string l, string r) => l + r,
                (string l, double r) => l + Stringify(r),
                (double l, string r) => Stringify(l) + r,
                (object[] l, object r) =>

               l.Concat(new[] { r }).ToArray(),
                _ => Throw(expr.Op, $"Invalid use of '+' operator:\nleft operand = {SoftType(left)}\nright operand = {SoftType(right)}")
            },
            _ => throw new NotImplementedException($"The operator '{expr.Op.Type}' is not implemented.")
        };
    }

    public object Visit(Func expr)
    {
        var callee = EvaluateExpression(expr.Callee);

        var arguments = new List<object?>();
        foreach (var argument in expr.Args)
            arguments.Add(EvaluateExpression(argument));

        if (callee is not LoxCallable)
            throw new RuntimeException(expr.RightParen,
                "Can only call functions and classes.");

        var function = (LoxCallable)callee;
        if (arguments.Count != function.Arity)
            throw new RuntimeException(expr.RightParen,
                $"Expected {function.Arity} parameters but got {arguments.Count}");
        return function.FunctionCall(this, arguments);
    }

    public object Visit(Getter expr)
    {
        object? obj = EvaluateExpression(expr.GetFrom);
        if (obj is not LoxInstance instance)
        {
            throw new RuntimeException(expr.Name, "Tried to access an object without fields");
        }
        return instance[expr.Name];
    }

    public object Visit(Grouping expr) => EvaluateExpression(expr.Expression);

    public object Visit(Literal expr) => expr.Value;

    public object Visit(Logical expr)
    {
        var left = EvaluateExpression(expr.Left);

        if (expr.Op.Type == OR)
        {
            if (IsTruthy(left)) return left;
        }
        else if (expr.Op.Type == AND && !IsTruthy(left)) { return left; }
        return EvaluateExpression(expr.Right);
    }

    public object Visit(Setter expr)
    {
        object? obj = EvaluateExpression(expr.Obj);
        if (obj is not LoxInstance)
        {
            throw new RuntimeException(expr.Name, "Only instances have fields.");
        }

        object? value = EvaluateExpression(expr.Value);
        (( LoxInstance )obj)[expr.Name] = value;

        return value;
    }

    public object Visit(Super expr)
    {
        var distance = locals[expr];
        var superclass = (LoxClass?)environment.GetAt(distance!.Value!, "super");
        var instance = (LoxInstance?)environment.GetAt(distance!.Value - 1, "this");
        var method = superclass?.FindMethod(expr.Obj.Lexeme);
        if (method == null)
        {
            throw new RuntimeException(expr.Obj,
                $"Undefined property '{expr.Obj.Lexeme}'.");
        }
        return method.Bind(instance!);
    }

    public object Visit(Ternary expr)
    {
        var x =  EvaluateExpression(expr.Conditional);
        return IsTruthy(x) ? EvaluateExpression(expr.IfTrue) : EvaluateExpression(expr.IfFalse);
    }

    public object Visit(This expr)
    {
        return LookUpVariable(expr.Keyword, expr);
    }

    public object Visit(Variable expr)
    {
        return LookUpVariable(expr.Name, expr);
    }

    public object Visit(Unary expr)
    {
        var right = EvaluateExpression(expr.Expression);
        return expr.Op.Type switch
        {
            MINUS => right is double r ? -r : throw new RuntimeException(expr.Op, "Only numbers have negative values."),
            BANG => !IsTruthy(right),
#if DEBUG
            HASH => GetInfo(expr),
#endif
            _ => null
        };
    }

    private object LookUpVariable(Token name, Expr expression)
    {
        if (locals.TryGetValue(expression, out int? distance))
        {
            return environment.GetAt(distance!.Value, name.Lexeme);
        }
        else
        {
            return globals.Get(name);
        }
    }
}
