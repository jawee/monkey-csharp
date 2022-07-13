using Monkey.Core.AST;
using Boolean = Monkey.Core.AST.Boolean;
using Expression = Monkey.Core.AST.Expression;

namespace Monkey.Test;

public class ParserTest
{
    [Test]
    public void TestMacroLiteralParsing()
    {
        var input = "macro(x, y) { x + y; }";
        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        var program = parser.ParseProgram();
        CheckParserErrors(parser);

        if (program.Statements.Count != 1)
        {
            Assert.Fail($"program.Statements does not contain '1' statements. Got '{program.Statements.Count}'");
        }

        if (program.Statements[0] is not ExpressionStatement stmt)
        {
            Assert.Fail($"program.Statements[0] is not ExpressionStatement, got '{program.Statements[0]}'");
            return;
        }

        if (stmt.Expression is not MacroLiteral macro)
        {
            Assert.Fail($"stmt.Expression is not MacroLiteral, got '{stmt.Expression}'");
            return;
        }

        if (macro.Body.Statements.Count != 1)
        {
            Assert.Fail($"macro.Body.Statements has not 1 statements. Got '{macro.Body.Statements.Count}'");
        }

        if (macro.Body.Statements[0] is not ExpressionStatement bodyStmt)
        {
            Assert.Fail($"macro body stmt is not ExpressionStatement, got '{macro.Body.Statements[0]}'");
            return;
        }

        TestInfixExpression(bodyStmt.Expression, "x", "+", "y");
    }
    
    [Test]
    public void TestParsingHashLiteralsStringKeys()
    {
        var input = @"{""one"": 1, ""two"": 2, ""three"": 3}";

        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        var program = parser.ParseProgram();
        CheckParserErrors(parser);

        var stmt = program.Statements[0] as ExpressionStatement;
        if (stmt.Expression is not HashLiteral)
        {
            Assert.Fail($"exp is not HashLiteral. Got '{stmt.Expression}'");
        }

        var hash = stmt.Expression as HashLiteral;

        if (hash.Pairs.Count != 3)
        {
            Assert.Fail($"hash.Pairs has wrong length. Got '{hash.Pairs.Count}");
        }

        var expected = new Dictionary<string, int>()
        {
            {"one", 1},
            {"two", 2},
            {"three", 3}
        };

        foreach (var pair in hash.Pairs)
        {
            if (pair.Key is not StringLiteral)
            {
                Assert.Fail($"key is not StringLiteral, got '{pair.Key}'");
            }

            var literal = pair.Key as StringLiteral;

            var expectedValue = expected[literal.String()];

            TestIntegerLiteral(pair.Value, expectedValue);
        }
    }

    [Test]
    public void TestParsingEmptyHashLiteral()
    {
        var input = @"{}";
        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        var program = parser.ParseProgram();
        CheckParserErrors(parser);
        
        var stmt = program.Statements[0] as ExpressionStatement;
        if (stmt.Expression is not HashLiteral)
        {
            Assert.Fail($"exp is not HashLiteral. Got '{stmt.Expression}'");
        }

        var hash = stmt.Expression as HashLiteral;

        if (hash.Pairs.Count != 0)
        {
            Assert.Fail($"hash.Pairs has wrong length. Got '{hash.Pairs.Count}");
        }
    }

    [Test]
    public void TestParsingHashLiteralsWithExpressions()
    {
        var input = @"{""one"": 0 + 1, ""two"": 10 - 8, ""three"": 15 / 5}";
        
        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        var program = parser.ParseProgram();
        CheckParserErrors(parser);
        
        var stmt = program.Statements[0] as ExpressionStatement;
        if (stmt.Expression is not HashLiteral)
        {
            Assert.Fail($"exp is not HashLiteral. Got '{stmt.Expression}'");
        }

        var hash = stmt.Expression as HashLiteral;

        if (hash.Pairs.Count != 3)
        {
            Assert.Fail($"hash.Pairs has wrong length. Got '{hash.Pairs.Count}");
        }

        var tests = new Dictionary<string, Action<Expression>>()
        {
            {"one", (e) => { TestInfixExpression(e, 0, "+", 1); }},
            {"two", (e) => { TestInfixExpression(e, 10, "-", 8); }},
            {"three", (e) => { TestInfixExpression(e, 15, "/", 5); }}
        };

        foreach (var pair in hash.Pairs)
        {
            if (pair.Key is not StringLiteral)
            {
                Assert.Fail($"key is not StringLiteral, got '{pair.Key}'");
            }

            var literal = pair.Key as StringLiteral;
            var testFunc = tests[literal.String()];

            testFunc(pair.Value);
        }
    }
    [Test]
    public void TestParsingIndexExpressions()
    {
        var input = "myArray[1 + 1]";
        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        var program = parser.ParseProgram();
        CheckParserErrors(parser);

        if (program.Statements[0] is not ExpressionStatement)
        {
            Assert.Fail($"program.Statements[0] is not ExpressionStatement. Got '{program.Statements[0]}'");
        }

        var stmt = program.Statements[0] as ExpressionStatement;

        if (stmt.Expression is not IndexExpression)
        {
            Assert.Fail($"stmt.Expression is not IndexExpression. Got '{stmt.Expression}'");
        }

        var indexExp = stmt.Expression as IndexExpression;
        if (!TestIdentifier(indexExp.Left, "myArray"))
        {
            Assert.Fail();
        }

        if (!TestInfixExpression(indexExp.Index, 1, "+", 1))
        {
            Assert.Fail();
        }
    }
    
    [Test]
    public void TestParrsingArrayLiterals()
    {
        var input = @"[1, 2 * 2, 3 + 3]";

        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        var program = parser.ParseProgram();
        CheckParserErrors(parser);

        if (program.Statements[0] is not ExpressionStatement)
        {
            Assert.Fail($"program.Statements[0] is not ExpressionStatement. Got '{program.Statements[0]}'");
        }

        var stmt = program.Statements[0] as ExpressionStatement;

        if (stmt.Expression is not ArrayLiteral)
        {
            Assert.Fail($"stmt.Expression is not ArrayLiteral. Got '{stmt.Expression}'");
        }

        var array = stmt.Expression as ArrayLiteral;

        if (array.Elements.Count != 3)
        {
            Assert.Fail($"array.Elements.Count not 3. Got '{array.Elements.Count}'");
        }

        TestIntegerLiteral(array.Elements[0], 1);
        TestInfixExpression(array.Elements[1], 2, "*", 2);
        TestInfixExpression(array.Elements[2], 3, "+", 3);
    }
    
    [Test]
    public void TestStringLiteralExpression()
    {
        var input = @"""hello world""";

        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        var program = parser.ParseProgram();
        CheckParserErrors(parser);

        var stmt = program.Statements[0] as ExpressionStatement;
        if (stmt.Expression is not StringLiteral)
        {
            Assert.Fail($"stmt.Expression is not StringLiteral. Got '{stmt.Expression}'");
        }

        var literal = stmt.Expression as StringLiteral;

        if (!literal.Value.Equals("hello world"))
        {
            Assert.Fail($"literal.Value not 'hello world', Got '{literal.Value}'");
        }
    }
    
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
            },
            new
            {
                Input = "true",
                Expected = "true"
            },
            new
            {
                Input = "false",
                Expected = "false"
            },
            new
            {
                Input = "3 > 5 == false",
                Expected = "((3 > 5) == false)"
            },
            new
            {
                Input = "3 < 5 == true",
                Expected = "((3 < 5) == true)"
            },
            new
            {
                Input = "1 + (2 + 3) +4",
                Expected = "((1 + (2 + 3)) + 4)"
            },
            new
            {
                Input = "(5 + 5) * 2",
                Expected = "((5 + 5) * 2)"
            },
            new
            {
                Input = "2 / (5 + 5)",
                Expected = "(2 / (5 + 5))"
            },
            new
            {
                Input = "-(5 + 5)",
                Expected = "(-(5 + 5))"
            },
            new
            {
                Input = "!(true == true)",
                Expected = "(!(true == true))"
            },
            new
            {
                Input = "a + add(b * c) + d",
                Expected = "((a + add((b * c))) + d)"
            },
            new
            {
                Input = "add(a, b, 1, 2 * 3, 4 + 5, add(6, 7 * 8))",
                Expected = "add(a, b, 1, (2 * 3), (4 + 5), add(6, (7 * 8)))"
            },
            new
            {
                Input = "add(a + b + c * d / f + g)",
                Expected = "add((((a + b) + ((c * d) / f)) + g))"
            },
            new
            {
                Input = "a * [1, 2, 3, 4][b * c] * d",
                Expected = "((a * ([1, 2, 3, 4][(b * c)])) * d)"
            },
            new
            {
                Input = "add(a * b[2], b[1], 2 * [1, 2][1])",
                Expected = "add((a * (b[2])), (b[1]), (2 * ([1, 2][1])))"
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
    public void TestParsingPrefixExpressionsBool()
    {
        var prefixTests = new[]
        {
            new
            {
                Input = "!true;", Operator = "!", Value = true
            },
            new
            {
                Input = "!false;", Operator = "!", Value = false 
            },
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

            if (!TestBooleanLiteral(expression.Right, test.Value))
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

    private struct ReturnTestCase
    {
        public string Input { get; set; }
        public object ExpectedValue { get; set; }
    }
    [Test]
    public void TestReturnStatementsNew()
    {
        var tests = new List<ReturnTestCase>
        {
            new()
            {
                Input = "return 5;",
                ExpectedValue = 5,
            },
            new()
            {
                Input = "return true;",
                ExpectedValue = true
            },
            new()
            {
                Input = "return foobar;",
                ExpectedValue = "foobar"
            }
        };

        foreach (var test in tests)
        {
            var lexer = new Lexer(test.Input);
            var parser = new Parser(lexer);
            var program = parser.ParseProgram();
            CheckParserErrors(parser);
            
            if (program.Statements.Count != 1)
            {
                Assert.Fail($"program.Statements does not contain '1' statements. Got '{program.Statements.Count}'");
            }

            if (program.Statements[0] is not ReturnStatement)
            {
                Assert.Fail($"program.Statements[0] is not ReturnStatement. Got '{program.Statements[0]}'");
            }

            var returnStmt = program.Statements[0] as ReturnStatement;

            if (!returnStmt.TokenLiteral().Equals("return"))
            {
                Assert.Fail($"returnStmt.TokenLiteral() not 'return'. Got '{returnStmt.TokenLiteral()}'");
            }

            if (!TestLiteralExpression(returnStmt.ReturnValue, test.ExpectedValue))
            {
                Assert.Fail();
            }
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

    private struct TestCase
    {
        public string Input { get; set; }
        public string ExpectedIdentifier { get; set; }
        public object ExpectedValue { get; set; }
    }
    [Test]
    public void TestLetStatementsNew()
    {

        var tests = new List<TestCase>
        {
            new()
            {
                Input = "let x = 5;",
                ExpectedIdentifier = "x",
                ExpectedValue = 5
            },
            new() 
            {
                Input = "let y = true;",
                ExpectedIdentifier = "y",
                ExpectedValue = true
            },
            new() 
            {
                Input = "let foobar = y;",
                ExpectedIdentifier = "foobar",
                ExpectedValue = "y"
            }
        };

        foreach (var test in tests)
        {
            var lexer = new Lexer(test.Input);
            var parser = new Parser(lexer);
            var program = parser.ParseProgram();
            CheckParserErrors(parser);

            if (program.Statements.Count != 1)
            {
                Assert.Fail($"program.Statements does not contain '1' statements. Got '{program.Statements.Count}'");
            }

            var stmt = program.Statements[0];
            if (!TestLetStatements(stmt, test.ExpectedIdentifier))
            {
                Assert.Fail();
            }

            var let = stmt as LetStatement;

            if (!TestLiteralExpression(let.Value, test.ExpectedValue))
            {
                Assert.Fail();
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

    private bool TestIdentifier(Expression exp, string? value)
    {
        if (exp is not Identifier)
        {
           Assert.Fail($"exp not 'Identifier'. Got '{exp}'");
           return false;
        }

        var ident = exp as Identifier;

        if (!ident.Value.Equals(value))
        {
            Assert.Fail($"ident.Value not '{value}'. Got '{ident.TokenLiteral()}'");
            return false;
        }

        if (!ident.TokenLiteral().Equals(value))
        {
            Assert.Fail($"ident.TokenLiteral not '{value}'. Got '{ident.TokenLiteral()}'");
            return false;
        }

        return true;
    }

    private bool TestLiteralExpression<T>(Expression exp, T expected) 
    {
        var type = expected?.GetType();
        if (type == typeof(int))
        {
            return TestIntegerLiteral(exp, (int)(object) expected!);
        }

        if (type == typeof(string))
        {
            return TestIdentifier(exp, (string)(object) expected!);
        }

        if (type == typeof(bool))
        {
            return TestBooleanLiteral(exp, (bool) (object) expected);
        }
        
        Assert.Fail($"Type of exp not handled. Got '{exp}'");
        return false;
    }

    private bool TestBooleanLiteral(Expression exp, bool value)
    {
        if (exp is not Boolean bo)
        {
            Assert.Fail($"exp is not Boolean. Got '{exp}'");
            return false;
        }

        if (bo.Value != value)
        {
            Assert.Fail($"bo.Value not '{value}'. Got '{bo.Value}'");
            return false;
        }

        if (!bo.TokenLiteral().Equals(value.ToString().ToLower()))
        {
            Assert.Fail($"bo.TokenLiteral not '{value.ToString()}'. Got '{bo.TokenLiteral()}'");
            return false;
        }

        return true;
    }

    private bool TestInfixExpression<T, TY>(Expression exp, T left, string op, TY right)
    {
        if (exp is not InfixExpression opExp)
        {
            Assert.Fail($"Exp is not 'InfixExpression'. Got '{exp}'");
            return false;
        }

        if (!TestLiteralExpression(opExp.Left, left))
        {
            return false;
        }

        if (opExp.Operator != op)
        {
            return false;
        }

        if (!TestLiteralExpression(opExp.Right, right))
        {
            return false;
        }

        return true;
    }

    
    [Test]
    public void TestParsingInfixExpressionsNew()
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

            if (!TestInfixExpression(expression, test.LeftValue, test.Operator, test.RightValue))
            {
                return;
            }
        }

    }
    [Test]
    public void TestParsingInfixExpressionsBool()
    {
        var infixTests = new[]
        {
            new
            {
                Input = "true == true", LeftValue = true, Operator = "==", RightValue = true
            },
            new
            {
                Input = "true != false", LeftValue = true, Operator = "!=", RightValue = false
            },
            new
            {
                Input = "false == false", LeftValue = false, Operator = "==", RightValue = false
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

            if (!TestInfixExpression(expression, test.LeftValue, test.Operator, test.RightValue))
            {
                return;
            }
        }

    }
    
    [Test]
    public void TestBooleanExpression()
    {
        var tests = new[] 
        {
            new { Input = "true;", ExpectedBoolean = true },
            new { Input = "false;", ExpectedBoolean = false },
        };

        foreach (var test in tests)
        {
            var lexer = new Lexer(test.Input);
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
            
            if (stmt?.Expression is not Boolean)
            {
                Assert.Fail("Expression is not Boolean");
            }

            var b = stmt?.Expression as Boolean;

            if (b != null && b.Value != test.ExpectedBoolean)
            {
                Assert.Fail($"b.Value not '{test.ExpectedBoolean}', got '{b.Value}'");
            }
        }
        
    }

    [Test]
    public void TestIfExpression()
    {
        var input = @"if (x < y) { x }";

        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        var program = parser.ParseProgram();
        CheckParserErrors(parser);

        if (program.Statements.Count != 1)
        {
            Assert.Fail($"program.Statements does not contain '{1}' statements. Got '{program.Statements.Count}'");
        }

        if (program.Statements[0] is not ExpressionStatement)
        {
            Assert.Fail($"program.Statements[0] is not ExpressionStatement. Got '{program.Statements[0]}'");
        }

        var stmt = program.Statements[0] as ExpressionStatement;

        if (stmt.Expression is not IfExpression)
        {
            Assert.Fail($"stmt.Expression is not IfExpression. Got '{stmt.Expression}'");
        }

        var exp = stmt.Expression as IfExpression;
        
        if (!TestInfixExpression(exp.Condition, "x", "<", "y"))
        {
            Assert.Fail();
        }

        if (exp.Consequence.Statements.Count != 1)
        {
            Assert.Fail($"consequence is not '1' statements. Got '{exp.Consequence.Statements.Count}'");
        }

        if (exp.Consequence.Statements[0] is not ExpressionStatement)
        {
            Assert.Fail($"Statements[0] is not ExpressionStatement. Got '{exp.Consequence.Statements[0]}'");
        }

        var consequence = exp.Consequence.Statements[0] as ExpressionStatement;

        if (!TestIdentifier(consequence.Expression, "x"))
        {
            Assert.Fail();
        }

        if (exp.Alternative != null)
        {
            Assert.Fail($"exp.Alternative.Statements was not null. Got '{exp.Alternative}'");
        }
    }
    
    [Test]
    public void TestIfElseExpression()
    {
        var input = @"if (x < y) { x } else { y }";

        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        var program = parser.ParseProgram();
        CheckParserErrors(parser);

        if (program.Statements.Count != 1)
        {
            Assert.Fail($"program.Statements does not contain '{1}' statements. Got '{program.Statements.Count}'");
        }

        if (program.Statements[0] is not ExpressionStatement)
        {
            Assert.Fail($"program.Statements[0] is not ExpressionStatement. Got '{program.Statements[0]}'");
        }

        var stmt = program.Statements[0] as ExpressionStatement;

        if (stmt.Expression is not IfExpression)
        {
            Assert.Fail($"stmt.Expression is not IfExpression. Got '{stmt.Expression}'");
        }

        var exp = stmt.Expression as IfExpression;
        
        if (!TestInfixExpression(exp.Condition, "x", "<", "y"))
        {
            Assert.Fail();
        }

        if (exp.Consequence.Statements.Count != 1)
        {
            Assert.Fail($"consequence is not '1' statements. Got '{exp.Consequence.Statements.Count}'");
        }

        if (exp.Consequence.Statements[0] is not ExpressionStatement)
        {
            Assert.Fail($"Statements[0] is not ExpressionStatement. Got '{exp.Consequence.Statements[0]}'");
        }

        var consequence = exp.Consequence.Statements[0] as ExpressionStatement;

        if (!TestIdentifier(consequence.Expression, "x"))
        {
            Assert.Fail();
        }

        if (exp.Alternative.Statements.Count != 1)
        {
            Assert.Fail($"exp.Alternative.Statements does not contain '1' statements. Got '{exp.Alternative.Statements.Count}'");
        }

        if (exp.Alternative.Statements[0] is not ExpressionStatement)
        {
            Assert.Fail($"Statements[0] is not ExpressionStatement. Got '{exp.Alternative.Statements[0]}");
        }

        var alternative = exp.Alternative.Statements[0] as ExpressionStatement;

        if (!TestIdentifier(alternative.Expression, "y"))
        {
            Assert.Fail();
        }
    }

    [Test]
    public void TestFunctionLiteralParsing()
    {
        var input = @"fn(x, y) { x + y; }";
        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        var program = parser.ParseProgram();
        CheckParserErrors(parser);
        
        if (program.Statements.Count != 1)
        {
           Assert.Fail($"program.Statements does not contain '1' statements. Got '{program.Statements.Count}'"); 
        }

        if (program.Statements[0] is not ExpressionStatement)
        {
            Assert.Fail($"program.Statements[0] is not ExpressionStatement. Got '{program.Statements[0]}'");
        }

        var stmt = program.Statements[0] as ExpressionStatement;

        if (stmt.Expression is not FunctionLiteral)
        {
            Assert.Fail($"stmt.Expression is not FunctionLiteral. Got '{stmt.Expression}'");
        }

        var function = stmt.Expression as FunctionLiteral;
        
        if (function.Parameters.Count != 2)
        {
            Assert.Fail($"function literal parameters wrong. want '2', got '{function.Parameters.Count}'");
        }

        TestLiteralExpression(function.Parameters[0], "x");
        TestLiteralExpression(function.Parameters[1], "y");

        if (function.Body.Statements.Count != 1)
        {
            Assert.Fail($"function.Body.Statements has not 1 statements. Got '{function.Body.Statements.Count}'");
        }

        if (function.Body.Statements[0] is not ExpressionStatement)
        {
            Assert.Fail($"function body stmt is not ExpressionStatement. Got '{function.Body.Statements[0]}'");
        }

        var bodyStmt = function.Body.Statements[0] as ExpressionStatement;

        TestInfixExpression(bodyStmt.Expression, "x", "+", "y");
    }

    [Test]
    public void TestFunctionParameterParsing()
    {
        var tests = new[]
        {
            new
            {
                Input = "fn() {};",
                ExpectedParams = new List<String> { }
            },
            new
            {
                Input = "fn(x) {};",
                ExpectedParams = new List<String> {"x"}
            },
            new
            {
                Input = "fn(x, y, z) {};",
                ExpectedParams = new List<String> {"x", "y", "z"}
            }
        };

        foreach (var test in tests)
        {
            var lexer = new Lexer(test.Input);
            var parser = new Parser(lexer);
            var program = parser.ParseProgram();

            var stmt = program.Statements[0] as ExpressionStatement;
            var function = stmt.Expression as FunctionLiteral;

            if (function.Parameters.Count != test.ExpectedParams.Count)
            {
                Assert.Fail($"Count parameters wrong. Want '{test.ExpectedParams.Count}', Got '{function.Parameters.Count}'");
            }

            for (var i = 0; i < test.ExpectedParams.Count; i++)
            {
                TestLiteralExpression(function.Parameters[i], test.ExpectedParams[i]);
            }

        }
    }

    [Test]
    public void TestCallExpressionParsing()
    {
        var input = @"add(1, 2 * 3, 4 + 5);";

        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        var program = parser.ParseProgram();
        CheckParserErrors(parser);

        if (program.Statements.Count != 1)
        {
            Assert.Fail($"program.Statements does not contain '1' statments. Got '{program.Statements.Count}'");
        }

        if (program.Statements[0] is not ExpressionStatement)
        {
            Assert.Fail($"stmt is not ExpressionStatement. Got '{program.Statements[0]}'");
        }

        var stmt = program.Statements[0] as ExpressionStatement;

        if (stmt.Expression is not CallExpression)
        {
            Assert.Fail($"stmt.Expression is not CallExpression. Got '{stmt.Expression}'");
        }

        var exp = stmt.Expression as CallExpression;

        if (!TestIdentifier(exp.Function, "add"))
        {
            Assert.Fail();
        }

        if (exp.Arguments.Count != 3)
        {
            Assert.Fail($"wrong length of arguments. Got '{exp.Arguments.Count}'");
        }

        TestLiteralExpression(exp.Arguments[0], 1);
        TestInfixExpression(exp.Arguments[1], 2, "*", 3);
        TestInfixExpression(exp.Arguments[2], 4, "+", 5);
    }
}