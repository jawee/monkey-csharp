using Monkey.Core.AST;
using Monkey.Core.Object;
using Array = Monkey.Core.Object.Array;
using Boolean = Monkey.Core.Object.Boolean;
using Environment = Monkey.Core.Object.Environment;
using Object = Monkey.Core.Object.Object;
using String = Monkey.Core.Object.String;

namespace Monkey.Test;

public class EvaluatorTest
{
    [Test]
    public void TestExpandMacros()
    {
        var tests = new[]
        {
            new
            {
                Input = "let infixExpression = macro() { quote(1 + 2); }; infixExpression();",
                Expected = "(1 + 2)"
            },
            new
            {
                Input = "let reverse = macro(a, b) { quote(unquote(b) - unquote(a)); }; reverse(2 + 2, 10 - 5);",
                Expected = "(10 - 5) - (2 + 2)"
            },
            new
            {
                Input = @"let unless = macro(condition, consequence, alternative) { quote(if (!(unquote(condition))) { unquote(consequence); } else { unquote(alternative); }); }; unless(10 > 5, puts(""not greater""), puts(""greater""));",
                Expected = @"if (!(10 > 5)) { puts(""not greater"") } else { puts(""greater"") }"
            }
        };

        foreach (var test in tests)
        {
            var expected = TestParseProgram(test.Expected);
            var program = TestParseProgram(test.Input);

            var env = new Environment();
            Evaluator.DefineMacros(program, env);
            var expanded = Evaluator.ExpandMacros(program, env);

            if (!expanded.String().Equals(expected.String()))
            {
                Assert.Fail($"not equal. Want '{expanded.String()}', got '{expanded.String()}'");
            }
        }
    }


    [Test]
    public void TestDefineMacros()
    {
        var input = @"let number = 1; let function = fn(x, y) {{ x + y }}; let mymacro = macro(x, y) { x + y; };";
        var env = new Environment();
        var program = TestParseProgram(input);
        Evaluator.DefineMacros(program, env);

        if (program.Statements.Count != 2)
        {
            Assert.Fail($"Wrong number of statements. Got '{program.Statements.Count}");
        }

        if (env.Get("number") is not null)
        {
            Assert.Fail($"number should not be defined");
        }

        if (env.Get("function") is not null)
        {
            Assert.Fail($"function should not be defined");
        }

        var obj = env.Get("mymacro");
        if (obj is null)
        {
            Assert.Fail($"macro not in environment");
        }

        if (obj is not Macro macro)
        {
            Assert.Fail($"object is not Macro. Got '{obj}'");
            return;
        }

        if (macro.Parameters.Count != 2)
        {
            Assert.Fail($"Wrong number of macro parameters. Got '{macro.Parameters.Count}");
        }

        if (!macro.Parameters[0].String().Equals("x"))
        {
            Assert.Fail($"parameter is not 'x', got '{macro.Parameters[0]}'");
        }

        if (!macro.Parameters[1].String().Equals("y"))
        {
            Assert.Fail($"parameter is not 'y', got '{macro.Parameters[1]}'");
        }

        var expectedBody = "(x + y)";

        if (!macro.Body.String().Equals(expectedBody))
        {
            Assert.Fail($"body is not '{expectedBody}', got '{macro.Body.String()}'");
        }
    }


    private Program TestParseProgram(string input)
    {
        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        return parser.ParseProgram();
    }

    [Test]
    public void TestQuoteUnquote()
    {
        var tests = new[]
        {
            new
            {
                Input = "quote(unquote(4))",
                Expected = "4"
            },
            new
            {
                Input = "quote(unquote(4 + 4))",
                Expected = "8"
            },
            new
            {
                Input = "quote(8 + unquote(4 + 4))",
                Expected = "(8 + 8)"
            },
            new
            {
                Input = "quote(unquote(4 + 4) + 8)",
                Expected = "(8 + 8)"
            },
            new
            {
                Input = "let foobar = 8; quote(foobar)",
                Expected = "foobar"
            },
            new
            {
                Input = "let foobar = 8; quote(unquote(foobar))",
                Expected = "8"
            },
            new
            {
                Input = "quote(unquote(true))",
                Expected = "true"
            },
            new
            {
                Input = "quote(unquote(true == false))",
                Expected = "false"
            },
            new
            {
                Input = "quote(unquote(quote(4 + 4)))",
                Expected = "(4 + 4)"
            },
            new
            {
                Input = "let quotedInfixExpression = quote(4 + 4); quote(unquote(4 + 4) + unquote(quotedInfixExpression))",
                Expected = "(8 + (4 + 4))"
            }
        };

        foreach (var test in tests)
        {
            var evaluated = TestEval(test.Input);
            if (evaluated is not Quote)
            {
                Assert.Fail($"expected Quote. Got '{evaluated}'");
            }

            var quote = evaluated as Quote;

            if (quote.Node == null)
            {
                Assert.Fail("quote.Node is null");
            }

            if (!quote.Node.String().Equals(test.Expected))
            {
                Assert.Fail($"Not equal. Got '{quote.Node.String()}'. Want '{test.Expected}'");
            }
        }
    }
    [Test]
    public void TestQuote()
    {
        var tests = new[]
        {
            new
            {
                Input = "quote(5)",
                Expected = "5"
            },
            new
            {
                Input = "quote(5 + 8)",
                Expected = "(5 + 8)"
            }, 
            new
            {
                Input = "quote(foobar)",
                Expected = "foobar"
            },
            new
            {
                Input = "quote(foobar + barfoo)",
                Expected = "(foobar + barfoo)"
            }
        };

        foreach (var test in tests)
        {
            var evaluated = TestEval(test.Input);
            if (evaluated is not Quote)
            {
                Assert.Fail($"expected Quote. Got '{evaluated}'");
            }

            var quote = evaluated as Quote;

            if (quote.Node == null)
            {
                Assert.Fail($"quote.Node is null");
            }

            if (!quote.Node.String().Equals(test.Expected))
            {
                Assert.Fail($"not equal. Got '{quote.Node.String()}'. Want '{test.Expected}'");
            }
        }
    }
    [Test]
    public void TestHashIndexExpressionsInt()
    {
        var tests = new[]
        {
            new
            {
                Input = @"{""foo"": 5}[""foo""]",
                Expected = 5
            },
            new
            {
                Input = @"let key = ""foo""; {""foo"": 5}[key]",
                Expected = 5
            },
            new
            {
                Input = @"{5: 5}[5]",
                Expected = 5
            },
            new
            {
                Input = @"{true: 5}[true]",
                Expected = 5
            },
            new 
            {
                Input = @"{false: 5}[false]",
                Expected = 5
            }
        };

        foreach (var test in tests)
        {
            var evaluated = TestEval(test.Input);
            TestIntegerObject(evaluated, test.Expected);
        }
    }

    [Test]
    public void TestHashIndexExpressionsNull()
    {
        var tests = new[]
        {
            new
            {
                Input = @"{}[""foo""]",
            },
            new
            {
                Input = @"{""foo"": 5}[""bar""]",
            },
        };
        foreach (var test in tests)
        {
            var evaluated = TestEval(test.Input);
            TestNullObject(evaluated);
        }
    }
    [Test]
    public void TestHashLiterals()
    {
        var input = @"let two = ""two"";
        {
            ""one"": 10-9,
            two: 1 + 1,
            ""thr"" + ""ee"": 6 / 2,
            4: 4,
            true: 5,
            false: 6
        }";

        var evaluated = TestEval(input);
        if (evaluated is not Hash)
        {
            Assert.Fail($"Eval didn't return Hash. Got '{evaluated}'");
        }

        var result = evaluated as Hash;

        var expected = new Dictionary<HashKey, int>
        {
            {new String {Value = "one"}.HashKey(), 1},
            {new String {Value = "two"}.HashKey(), 2},
            {new String {Value = "three"}.HashKey(), 3},
            {new Integer {Value = 4}.HashKey(), 4},
            {new Boolean {Value = true}.HashKey(), 5},
            {new Boolean {Value = false}.HashKey(), 6}
        };

        if (result.Pairs.Count != expected.Count)
        {
            Assert.Fail($"Hash has wrong num of pairs. Got '{result.Pairs}'");
        }

        foreach (var (e, v) in expected)
        {
            if (!result.Pairs.ContainsKey(e))
            {
                Assert.Fail($"result.Pairs does not contain key '{e}'");
            }
            var pair = result.Pairs[e];

            TestIntegerObject(pair.Value, v);
        }

    }
    [Test]
    public void TestArrayPush()
    {
        var tests = new[]
        {
            new
            {
                Input = "let myArray = []; push(myArray, 1);",
                Expected = "[1]",
            },
            new
            {
                Input = "let myArray = [1]; push(myArray, 2);",
                Expected = "[1, 2]"
            },
            new
            {
                Input = "let myArray = [1, 2, 3]; push(myArray, 4);",
                Expected = "[1, 2, 3, 4]"
            }
        };

        foreach (var test in tests)
        {
            var evaluated = TestEval(test.Input) as Array;
            var evaluatedExpected = TestEval(test.Expected) as Array;

            if (!ArrayEquals(evaluated, evaluatedExpected))
            {
                Assert.Fail($"evaluated is not equal to expected. Got '{evaluated.Inspect()}', Want '{evaluatedExpected.Inspect()}'"); 
            }
        }
    }
    [Test]
    public void TestArrayFirst()
    {
        var tests = new[]
        {
            new
            {
                Input = "let myArray = [1, 2, 3]; first(myArray);",
                Expected = 1
            }
        };

        foreach (var test in tests)
        {
            var evaluated = TestEval(test.Input);
            TestIntegerObject(evaluated, test.Expected);
        }
    }

    [Test]
    public void TestArrayRest()
    {
        var tests = new[]
        {
            new
            {
                Input = "let myArray = [1, 2, 3]; rest(myArray);",
                Expected = "[2, 3]"
            }
        };

        foreach (var test in tests)
        {
            var evaluated = TestEval(test.Input) as Array;
            var evaluatedExpected = TestEval(test.Expected) as Array;

            if (!ArrayEquals(evaluated, evaluatedExpected))
            {
               Assert.Fail($"evaluated is not equal to expected. Got '{evaluated.Inspect()}', Want '{evaluatedExpected.Inspect()}'"); 
            }
        }
    }

    private bool ArrayEquals(Array a1, Array a2)
    {
        return a1.Inspect().Equals(a2.Inspect());
    }
    [Test]
    public void TestArrayIndexExpressionsNull()
    {
        var tests = new[]
        {
            new
            {
                Input = "[1, 2, 3][3]",
            },
            new
            {
                Input = "[1, 2, 3][-1]"
            }
        };
        foreach (var test in tests)
        {
            var evaluated = TestEval(test.Input);
            TestNullObject(evaluated);
        }
    }
    [Test]
    public void TestArrayIndexExpressions()
    {
        var tests = new[]
        {
            new
            {
                Input = "[1, 2, 3][0]",
                Expected = 1
            },
            new
            {
                Input = "[1, 2, 3][1]",
                Expected = 2
            },
            new
            {
                Input = "[1, 2, 3][2]",
                Expected = 3
            },
            new
            {
                Input = "let i = 0; [1][i]",
                Expected = 1
            },
            new
            {
                Input = "[1, 2, 3][1 + 1]",
                Expected = 3
            },
            new
            {
                Input = "let myArray = [1, 2, 3]; myArray[2]",
                Expected = 3
            },
            new
            {
                Input = "let myArray = [1, 2, 3]; myArray[0] + myArray[1] + myArray[2]",
                Expected = 6
            },
            new
            {
                Input = "let myArray = [1, 2, 3]; let i = myArray[0]; myArray[i]",
                Expected = 2
            },
        };

        foreach (var test in tests)
        {
            var evaluated = TestEval(test.Input);
            TestIntegerObject(evaluated, test.Expected);
        }
    }
    [Test]
    public void TestArrayLiterals()
    {
        var input = "[1, 2 * 2, 3 + 3]";
        var evaluated = TestEval(input);
        if (evaluated is not Array)
        {
            Assert.Fail($"evaluated is not Array, Got '{evaluated}'");
        }

        var result = evaluated as Array;
        if (result.Elements.Count != 3)
        {
            Assert.Fail($"array has wrong num of elements. Got '{result.Elements.Count}'");
        }

        TestIntegerObject(result.Elements[0], 1);
        TestIntegerObject(result.Elements[1], 4);
        TestIntegerObject(result.Elements[2], 6);
    }

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
            },
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
            },
            new
            {
                Input = @"len([1, 2, 3])",
                Expected = 3
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
            },
            new
            {
                Input = @"{""name"": ""Monkey""}[fn(x) { x }];",
                Expected = "unusable as hash key: FUNCTION"
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