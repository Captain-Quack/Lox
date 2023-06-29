open NUnit.Framework
open Lox

type AstPrinterTests() =

    [<Test>]
    member _.TestPrintBinaryExpression() =
        let expression = 
            Expr.Binary(
                Expr.Unary(
                    Token(TokenType.MINUS, "-", null, 1),
                    Expr.Literal(123)),
                Token(TokenType.STAR, "*", null, 1),
                Expr.Grouping(
                    Expr.Literal(45.67)))

        let result = AstPrinter().Print(expression)

        Assert.AreEqual("(* (- 123) (group 45.67))", result)

    [<Test>]
    member _.TestPrintUnaryExpression() =
        let expression = 
            Expr.Unary(
                Token(TokenType.MINUS, "-", null, 1),
                Expr.Literal(123))

        let result = AstPrinter().Print(expression)

        Assert.AreEqual("(- 123)", result)

    [<Test>]
    member _.TestPrintGroupingExpression() =
        let expression = 
            Expr.Grouping(
                Expr.Literal(45.67))

        let result = AstPrinter().Print(expression)

        Assert.AreEqual("(group 45.67)", result)

    [<Test>]
    member _.TestPrintLiteralExpression() =
        let expression = Expr.Literal(123)

        let result = AstPrinter().Print(expression)

        Assert.AreEqual("123", result)
