using Monkey.Core.Object;
using Boolean = Monkey.Core.Object.Boolean;
using Environment = Monkey.Core.Object.Environment;
using Object = Monkey.Core.Object.Object;
using String = Monkey.Core.Object.String;

namespace Monkey.Test;

public class EvaluatorTest
{

    [Test]
    public void TestBuiltinFunctionsError()
    {
        var tests = new[]
        {
            new
            {
                Input = @"len(1)",
                Expected = "argument to 'len' not supported, got INTEGER"
            },
            new
            {
                Input = @"len(""one"", ""two"")",
                Expected = "wrong number of arguments. got=2, want=1"
            }
        };
        foreach (var test in tests)
        {
            var evaluated = TestEval(test.Input);

            if (evaluated is not Error)
            {
                Assert.Fail($"object is not Error. Got '{evaluated}'");
            }

            var errObj = evaluated as Error;
            if (!errObj.Message.Equals(test.Expected))
            {
                Assert.Fail($"wrong error message. Expected '{test.Expected}', got '{errObj.Message}'");
            } 
                
        }
    }
    [Test]
    public void TestBuiltinFunctionsSuccess()
    {
        var tests = new[]
        {
            new
            {
                Input = @"len("""")",
                Expected = 0
            },
            new
            {
                Input = @"len(""four"")",
                Expected = 4
            },
            new
            {
                Input = @"len(""hello world"")",
                Expected = 11
            }
        };

        foreach (var test in tests)
        {
            var evaluated = TestEval(test.Input);

            TestIntegerObject(evaluated, test.Expected);
        }
    }
    [Test]
    public void TestStringConcatenation()
    {
        var input = @"""Hello"" + "" "" + ""World!""";

        var evaluated = TestEval(input);

        if (evaluated is not String)
        {
            Assert.Fail($"Object is not String. Got '{evaluated}'");
        }

        var str = evaluated as String;

        if (!str.Value.Equals("Hello World!"))
        {
            Assert.Fail($"String has wrong value. Got '{str.Value}'");
        }
    }
    [Test]
    public void TestStringLiteral()
    {
        var input = @"""Hello World!""";

        var evaluated = TestEval(input);
        if (evaluated is not String)
        {
            Assert.Fail($"object is not String. Got '{evaluated}'");
        }

        var str = evaluated as String;
        if (!str.Value.Equals("Hello World!"))
        {
            Assert.Fail($"String has wrong value. Got '{str.Value}'");
        }
    }

    [Test]
    public void TestClosures()
    {
        var input = "let newAdder = fn(x) { fn(y) { x + y; }; }; let addTwo = newAdder(2); addTwo(2);";

        TestIntegerObject(TestEval(input), 4);
    }
    [Test]
    public void TestFunctionApplication()
    {
        var tests = new[]
        {
            new
            {
                Input = "let identity = fn(x) { x; }; identity(5);",
                Expected = 5
            },
            new
            {
                Input = "let identity = fn(x) { return x; }; identity(5);",
                Expected = 5
            },
            new
            {
                Input = "let double = fn(x) { x * 2; }; double(5);",
                Expected = 10
            },
            new
            {
                Input = "let add = fn(x, y) { x + y; }; add(5, 5);",
                Expected = 10
            },
            new
            {
                Input = "let add = fn(x, y) { x + y; }; add(5 + 5, add(5, 5));",
                Expected = 20
            },
            new
            {
                Input = "fn(x) { x; }(5)",
                Expected = 5
            }
        };

        foreach (var test in tests)
        {
            TestIntegerObject(TestEval(test.Input), test.Expected);
        }
    }
    [Test]
    public void TestFunctionObject()
    {
        var input = "fn(x) { x + 2; };";

        var evaluated = TestEval(input);
        if (evaluated is not Function)
        {
            Assert.Fail($"object is not Function. Got '{evaluated}'");
        }

        var fn = evaluated as Function;

        if (fn.Parameters.Count != 1)
        {
           Assert.Fail($"function has wrong parameters. Parameters: '{fn.Parameters}'"); 
        }

        if (!fn.Parameters[0].String().Equals("x"))
        {
            Assert.Fail($"parameter is not 'x', Got '{fn.Parameters[0]}'");
        }

        var expectedBody = "(x + 2)";

        if (!fn.Body.String().Equals(expectedBody))
        {
            Assert.Fail($"body is not '{expectedBody}'. Got '{fn.Body.String()}'");
        }
    }
    [Test]
    public void TestLetStatements()
    {
        var tests = new[]
        {
            new
            {
                Input = "let a = 5; a;",
                Expected = 5
            },
            new
            {
                Input = "let a = 5 * 5; a;",
                Expected = 25
            },
            new
            {
                Input = "let a = 5; let b = a; b;",
                Expected = 5
            },
            new
            {
                Input = "let a = 5; let b = a; let c = a + b + 5; c;",
                Expected = 15
            }
        };


        foreach (var test in tests)
        {
            TestIntegerObject(TestEval(test.Input), test.Expected);
        }
    }

    [Test]
    public void TestErrorHandling()
    {
        var tests = new[]
        {
            new
            {
                Input = "5 + true;",
                Expected = "type mismatch: INTEGER + BOOLEAN"
            },
            new
            {
                Input = "5 + true; 5;",
                Expected = "type mismatch: INTEGER + BOOLEAN"
            },
            new
            {
                Input = "-true",
                Expected = "unknown operator: -BOOLEAN"
            },
            new
            {
                Input = "true + false",
                Expected = "unknown operator: BOOLEAN + BOOLEAN"
            },
            new
            {
                Input = "5; true + false; 5",
                Expected = "unknown operator: BOOLEAN + BOOLEAN"
            },
            new
            {
                Input = "if (10 > 1) { true + false; }",
                Expected = "unknown operator: BOOLEAN + BOOLEAN"
            },
            new
            {
                Input = "if (10 > 1) { if (10 > 1) { return true + false } return 1; }",
                Expected = "unknown operator: BOOLEAN + BOOLEAN"
            },
            new
            {
                Input = "foobar",
                Expected = "identifier not found: foobar"
            },
            new
            {
                Input = @"""Hello"" - ""World""",
                Expected = "unknown operator: STRING - STRING"
            }
        };

        foreach (var test in tests)
        {
            var evaluated = TestEval(test.Input);

            if (evaluated is not Error)
            {
                Assert.Fail($"no error object returned. Got '{evaluated}'");
            }

            var errorObj = evaluated as Error;

            if (!errorObj.Message.Equals(test.Expected))
            {
                Assert.Fail($"wrong error message. expected '{test.Expected}'. Got '{errorObj.Message}'");
            }
        }
    }
    [Test]
    public void TestReturnStatements()
    {
        var tests = new[]
        {
            new
            {
                Input = "return 10;",
                Expected = 10
            },
            new
            {
                Input = "return 10; 9;",
                Expected = 10
            },
            new
            {
                Input = "return 2 * 5; 9;",
                Expected = 10
            },
            new
            {
                Input = "9; return 2 * 5; 9;",
                Expected = 10
            },
            new
            {
                Input = @"if (10 > 1) { if (10 > 1) { return 10; } return 1; }",
                Expected = 10
            }
        };

        foreach (var test in tests)
        {
            var evaluated = TestEval(test.Input);
            TestIntegerObject(evaluated, test.Expected);
        }
    }
    [Test]
    public void TestIfElseExpressions()
    {
        var tests = new[]
        {
            new
            {
                Input = "if (true) { 10 }",
                Expected = 10
            },
            new
            {
                Input = "if (1) { 10 }",
                Expected = 10
            },
            new
            {
                Input = "if (1 > 2) { 10 } else { 20 }",
                Expected = 20
            },
            new
            {
                Input = "if (1 < 2) { 10 } else { 20 }",
                Expected = 10
            }
        };

        foreach (var test in tests)
        {
            Console.WriteLine($"{test}");
            var evaluated = TestEval(test.Input);
            TestIntegerObject(evaluated, test.Expected);
        }
    }
    [Test]
    public void TestIfElseExpressionsNull()
    {
        var tests = new[]
        {
            new
            {
                Input = "if (false) { 10 }"
            },
            new
            {
                Input = "if (1 > 2) { 10 }"
            },
        };

        foreach (var test in tests)
        {
            Console.WriteLine($"TestCase: Input = '{test.Input}'.");
            var evaluated = TestEval(test.Input);
            TestNullObject(evaluated);
        }
    }

    private bool TestNullObject(Object obj)
    {
        if (obj != new Null())
        {
            Assert.Fail($"obj is not Null. Got '{obj}'");
            return false;
        }

        return true;
    }

    [Test]
    public void TestEvalIntegerExpression()
    {
        var tests = new[]
        {
            new
            {
                Input = "5",
                Expected = 5
            },
            new
            {
                Input = "10",
                Expected = 10
            },
            new
            {
                Input = "-5",
                Expected = -5
            },
            new
            {
                Input = "-10",
                Expected = -10
            },
            new
            {
                Input = "5 + 5 + 5 + 5 - 10",
                Expected = 10
            },
            new
            {
                Input = "2 * 2 * 2 * 2 * 2",
                Expected = 32
            },
            new
            {
                Input = "-50 + 100 + -50",
                Expected = 0
            },
            new
            {
                Input = "5 * 2 + 10",
                Expected = 20
            },
            new
            {
                Input = "5 + 2 * 10",
                Expected = 25
            },
            new
            {
                Input = "20 + 2 * -10",
                Expected = 0
            },
            new
            {
                Input = "50 / 2 * 2 + 10",
                Expected = 60
            },
            new
            {
                Input = "2 * (5 + 10)",
                Expected = 30
            },
            new
            {
                Input = "3 * 3 * 3 + 10",
                Expected = 37
            },
            new
            {
                Input = "3 * (3 * 3) + 10",
                Expected = 37
            },
            new
            {
                Input = "(5 + 10 * 2 + 15 / 3) * 2 + -10",
                Expected = 50
            }
        };
        
        foreach (var test in tests)
        {
            var evaluated = TestEval(test.Input);
            TestIntegerObject(evaluated, test.Expected);
        }
    }
    [Test]
    public void TestBangOperator()
    {
        var tests = new[]
        {
            new
            {
                Input = "!true",
                Expected = false
            },
            new
            {
                Input = "!false",
                Expected = true
            },
            new
            {
                Input = "!5",
                Expected = false
            },
            new
            {
                Input = "!!true",
                Expected = true
            },
            new
            {
                Input = "!!false",
                Expected = false
            },
            new
            {
                Input = "!!5",
                Expected = true
            }
        };

        foreach (var test in tests)
        {
            Console.WriteLine($"Testcase: Input = '{test.Input}', Expected = '{test.Expected}'");
            var evaluated = TestEval(test.Input);
            TestBooleanObject(evaluated, test.Expected);
        }
    }
    [Test]
    public void TestEvalBooleanExpression()
    {
        var tests = new[]
        {
            new
            {
                Input = "true",
                Expected = true
            },
            new
            {
                Input = "false",
                Expected = false
            },
            new
            {
                Input = "1 < 2",
                Expected = true
            },
            new
            {
                Input = "1 > 2",
                Expected = false
            },
            new
            {
                Input = "1 < 1",
                Expected = false
            },
            new
            {
                Input = "1 > 1",
                Expected = false
            },
            new
            {
                Input = "1 == 1",
                Expected = true
            },
            new
            {
                Input = "1 != 1",
                Expected = false
            },
            new
            {
                Input = "1 == 2",
                Expected = false
            },
            new
            {
                Input = "1 != 2",
                Expected = true
            },
            new
            {
                Input = "true == true",
                Expected = true
            },
            new
            {
                Input = "false == false",
                Expected = true
            },
            new
            {
                Input = "true == false",
                Expected = false
            },
            new
            {
                Input = "true != false",
                Expected = true
            },
            new
            {
                Input = "false != true",
                Expected = true
            },
            new
            {
                Input = "(1 < 2) == true",
                Expected = true
            },
            new
            {
                Input = "(1 < 2) == false",
                Expected = false
            },
            new
            {
                Input = "(1 > 2) == true",
                Expected = false
            },
            new
            {
                Input = "(1 > 2) == false",
                Expected = true
            }
        };

        foreach (var test in tests)
        {
            var evaluated = TestEval(test.Input);
            TestBooleanObject(evaluated, test.Expected);
        }
    }

    private bool TestBooleanObject(Object obj, bool expected)
    {
        if (obj is not Boolean)
        {
            Assert.Fail($"obj is not Boolean. Got '{obj}'");
            return false;
        }

        var result = obj as Boolean;

        if (result.Value != expected)
        {
            Assert.Fail($"obj has wrong value. Got '{result.Value}'. Want '{expected}'");
            return false;
        }

        return true;
    }
    private bool TestIntegerObject(Object obj, int expected)
    {
        if (obj is not Integer)
        {
            Assert.Fail($"obj is not Integer. Got '{obj}'");
            return false;
        }

        var result = obj as Integer;
        if (result.Value != expected)
        {
            Assert.Fail($"obj has wrong value. Got '{result.Value}'. Want '{expected}'");
            return false;
        }

        return true;
    }

    private Object TestEval(string input)
    {
        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        var program = parser.ParseProgram();
        var env = new Environment();

        return Evaluator.Eval(program, env);
    }
}