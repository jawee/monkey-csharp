using System.Data;
using Monkey.Core.AST;
using NuGet.Frameworks;

namespace Monkey.Test;

public class ParserTest
{
    [Test]
    public void TestOperatorPrecedenceParsing()
    {
        var tests = new[]
        {
            new
            {
                Input = "-a * b",
                Expected = "((-a) * b)"
            },
            new
            {
                Input = "!-a",
                Expected = "(!(-a))"
            },
            new
            {
                Input = "a + b + c",
                Expected = "((a + b) + c)"
            },
            new
            {
                Input = "a + b - c",
                Expected = "((a + b) - c)"
            },
            new
            {
                Input = "a * b * c",
                Expected = "((a * b) * c)"
            },
            new
            {
                Input = "a * b / c",
                Expected = "((a * b) / c)"
            },
            new
            {
                Input = "a + b / c",
                Expected = "(a + (b / c))"
            },
            new
            {
                Input = "a + b * c + d / e - f",
                Expected = "(((a + (b * c)) + (d / e)) - f)"
            },
            new
            {
                Input = "3 + 4; -5 * 5",
                Expected = "(3 + 4)((-5) * 5)"
            },
            new
            {
                Input = "5 > 4 == 3 < 4",
                Expected = "((5 > 4) == (3 < 4))"
            },
            new
            {
                Input = "5 < 4 != 3 > 4",
                Expected = "((5 < 4) != (3 > 4))"
            },
            new
            {
                Input = "3 + 4 * 5 == 3 * 1 + 4 * 5",
                Expected = "((3 + (4 * 5)) == ((3 * 1) + (4 * 5)))"
            }
        };
        foreach (var test in tests)
        {
            var lexer = new Lexer(test.Input);
            var parser = new Parser(lexer);
            var program = parser.ParseProgram();
            CheckParserErrors(parser);

            var actual = program.String();
            if (!actual.Equals(test.Expected))
            {
                Assert.Fail($"expected '{test.Expected}'. Got '{actual}'");
            }
        }
    }
    
    [Test]
    public void TestParsingInfixExpressions()
    {
        var infixTests = new[]
        {
            new
            {
                Input = "5 + 5;", LeftValue = 5, Operator = "+", RightValue = 5
            },
            new
            {
                Input = "5 - 5;", LeftValue = 5, Operator = "-", RightValue = 5
            },
            new
            {
                Input = "5 * 5;", LeftValue = 5, Operator = "*", RightValue = 5
            },
            new
            {
              Input = "5 / 5;", LeftValue = 5, Operator = "/", RightValue = 5
            },
            new
            {
                Input = "5 > 5;", LeftValue = 5, Operator = ">", RightValue = 5
            },
            new
            {
                Input = "5 < 5;", LeftValue = 5, Operator = "<", RightValue = 5
            },
            new
            {
                Input = "5 == 5;", LeftValue = 5, Operator = "==", RightValue = 5
            },
            new
            {
                Input = "5 != 5;", LeftValue = 5, Operator = "!=", RightValue = 5
            },
        };

        foreach (var test in infixTests)
        {
            var lexer = new Lexer(test.Input);
            var parser = new Parser(lexer);
            var program = parser.ParseProgram();
            CheckParserErrors(parser);

            if (program.Statements.Count != 1)
            {
                Assert.Fail($"program.Statements does not contain '1' statements. Got '{program.Statements.Count}'");
            }

            if (program.Statements[0] is not ExpressionStatement)
            {
                Assert.Fail($"program.Statements[0] is not ExpressionStatement, got '{program.Statements[0]}'");
            }

            var statement = program.Statements[0] as ExpressionStatement;

            if (statement.Expression is not InfixExpression)
            {
                Assert.Fail($"statement.Expression is not PrefixExpression");
            }

            var expression = statement.Expression as InfixExpression;

            if (!TestIntegerLiteral(expression.Right, test.RightValue))
            {
                return;
            }
        }
    }

    [Test]
    public void TestParsingPrefixExpressions()
    {
        var prefixTests = new[]
        {
            new
            {
                Input = "!5;", Operator = "!", IntegerValue = 5
            },
            new
            {
                Input = "-15;", Operator = "-", IntegerValue = 15 
            }
        };

        foreach (var test in prefixTests)
        {
            var lexer = new Lexer(test.Input);
            var parser = new Parser(lexer);
            var program = parser.ParseProgram();
            CheckParserErrors(parser);

            if (program.Statements.Count != 1)
            {
                Assert.Fail($"program.Statements does not contain '1' statements. Got '{program.Statements.Count}'");
            }

            if (program.Statements[0] is not ExpressionStatement)
            {
                Assert.Fail($"program.Statements[0] is not ExpressionStatement, got '{program.Statements[0]}'");
            }

            var statement = program.Statements[0] as ExpressionStatement;

            if (statement.Expression is not PrefixExpression)
            {
                Assert.Fail($"statement.Expression is not PrefixExpression");
            }

            var expression = statement.Expression as PrefixExpression;

            if (!expression.Operator.Equals(test.Operator))
            {
                Assert.Fail($"expression.Operator is not '{test.Operator}'. Got '{expression.Operator}'");
            }

            if (!TestIntegerLiteral(expression.Right, test.IntegerValue))
            {
                return;
            }
        }
    }

    private bool TestIntegerLiteral(Expression il, int value)
    {
        if (il is not IntegerLiteral)
        {
            Assert.Fail($"il is not IntegerLiteral");
            return false;
        }
        var integer = il as IntegerLiteral;

        if (integer.Value != value)
        {
            Assert.Fail($"integer.Value not '{value}'. Got '{integer.Value}'");
            return false;
        }

        if (integer.TokenLiteral() != value.ToString())
        {
            Assert.Fail($"integer.TokenLiteral not '{value}'. Got '{integer.TokenLiteral()}'");
            return false;
        }

        return true;
    }

    [Test]
    public void TestIdentifierExpression()
    {
        var input = @"foobar;";
        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        var program = parser.ParseProgram();
        CheckParserErrors(parser);

        if (program.Statements.Count != 1)
        {
            Assert.Fail($"Program has not enough statements. Got '{program.Statements.Count}'");
        }

        var statement = program.Statements[0];
        if (statement is not ExpressionStatement)
        {
            Assert.Fail($"program.Statements[0] is not ExpressionStatement");
        }

        var stmt = statement as ExpressionStatement;



        if (stmt?.Expression is not Identifier)
        {
            Assert.Fail("Expression is not Identifier");
        }

        var ident = stmt?.Expression as Identifier;

        if (ident != null && !ident.Value.Equals("foobar"))
        {
            Assert.Fail($"ident.Value not 'foobar', got '{ident.Value}'");
        }

        if (!ident.TokenLiteral().Equals("foobar"))
        {
            Assert.Fail($"ident.TokenLiteral() not 'foobar', got '{ident.TokenLiteral()}'");
        }
    }

    [Test]
    public void TestIntegerLiteralExpression()
    {
        var input = $"5;";
        
        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        var program = parser.ParseProgram();
        CheckParserErrors(parser);

        if (program.Statements.Count != 1)
        {
            Assert.Fail($"Program has not enough statements. Got '{program.Statements.Count}'");
        }

        var statement = program.Statements[0];
        if (statement is not ExpressionStatement)
        {
            Assert.Fail($"program.Statements[0] is not ExpressionStatement");
        }

        var stmt = statement as ExpressionStatement;
        
        if (stmt?.Expression is not IntegerLiteral)
        {
            Assert.Fail("Expression is not IntegerLiteral");
        }

        var ident = stmt?.Expression as IntegerLiteral;

        if (ident != null && ident.Value != 5)
        {
            Assert.Fail($"ident.Value not '5', got '{ident.Value}'");
        }

        if (!ident.TokenLiteral().Equals("5"))
        {
            Assert.Fail($"ident.TokenLiteral() not '5', got '{ident.TokenLiteral()}'");
        }
    }
    
    [Test]
    public void TestReturnStatements()
    {
        var input = @"
        return 5;
        return 10;
        return 993322;";

        var lexer = new Lexer(input);
        var parser = new Parser(lexer);

        var program = parser.ParseProgram();
        CheckParserErrors(parser);

        if (program.Statements.Count != 3)
        {
            Assert.Fail($"program.Statements does not contain 3 statements. Got '{program.Statements.Count}'");
        }

        foreach (var statement in program.Statements)
        {

            if (statement is not ReturnStatement returnStmt)
            {
                Assert.Fail($"s is not a ReturnStatement.");
                continue;
            }
            if (returnStmt.TokenLiteral() != "return")
            {
                Assert.Fail($"returnStmt.TokenLiteral not 'return', got '{returnStmt.TokenLiteral()}'");
            }
        }
    }

    [Test]
    public void TestLetStatements()
    {
        var input = @"
        let x = 5;
        let y = 10;
        let foobar = 838383;
        ";

        var lexer = new Lexer(input);
        var parser = new Parser(lexer);

        var program = parser.ParseProgram();
        CheckParserErrors(parser);
        if (program == null)
        {
            Assert.Fail("ParseProgram() returned null");
        }

        if (program.Statements.Count != 3)
        {
           Assert.Fail($"program.Statements does not contain 3 statements. Got '{program.Statements.Count}"); 
        }

        var tests = new[]
        {
            new {ExpectedIdentifier = "x"},
            new {ExpectedIdentifier = "y"},
            new {ExpectedIdentifier = "foobar"}
        };

        for (var i = 0; i < tests.Length; i++)
        {
            var test = tests[i];
            var stmt = program.Statements[i];

            if (!TestLetStatements(stmt, test.ExpectedIdentifier))
            {
                return;
            }
        }
    }

    private void CheckParserErrors(Parser parser)
    {
        var errors = parser.Errors();

        if (errors.Count == 0)
        {
            return;
        }
        
        Console.WriteLine($"Parser has {errors.Count} errors");
        foreach (var error in errors)
        {
           Console.WriteLine($"Parser error: '{error}'"); 
        }
        Assert.Fail();
    }

    private bool TestLetStatements(Statement s, string name)
    {
        if (s.TokenLiteral() != "let")
        {
            Assert.Fail($"s.TokenLiteral not 'let'. Got '{s.TokenLiteral()}'");
            return false;
        }

        if (s is not LetStatement letStmt)
        {
            Assert.Fail($"s is not a LetStatement.");
            return false;
        }
        
        if (letStmt.Name.Value != name)
        {
           Assert.Fail($"letStmt.Name.Value is not '{name}', got '{letStmt.Name.Value}'");
           return false;
        }

        if (letStmt.Name.TokenLiteral() != name)
        {
            Assert.Fail($"letStmt.Name.TokenLiteral() not '{name}', got '{letStmt.Name.TokenLiteral()}'");
            return false;
        }

        return true;
    }
}