using System.Text.Json;
using Monkey.Core.AST;
using Monkey.Core.Compiler;
using Monkey.Core.Object;
using Monkey.Core.Vm;
using Array = Monkey.Core.Object.Array;
using Boolean = Monkey.Core.Object.Boolean;
using Object = Monkey.Core.Object.Object;
using String = Monkey.Core.Object.String;

namespace Monkey.Test;

public class VmTest
{
    [Test]
    public void TestBuiltinFunctions()
    {
        var tests = new VmTestCase[]
        {
            new()
            {
                Input = @"len("""")",
                Expected = 0
            },
            new()
            {
                Input = @"len(""four"")",
                Expected = 4
            },
            new()
            {
                Input = @"len(""hello world"")",
                Expected = 11
            },
            new()
            {
                Input = @"len(1)",
                Expected = new Error {Message = @"argument to 'len' not supported, got INTEGER"}
            },
            new()
            {
                Input = @"len(""one"", ""two"")",
                Expected = new Error {Message = @"wrong number of arguments. got=2, want=1"}
            },
            new()
            {
                Input = @"len([1, 2, 3])",
                Expected = 3
            },
            new()
            {
                Input = @"len([])",
                Expected = 0
            },
            new()
            {
                Input = @"puts(""hello"", ""world"")",
                Expected = Vm.NULL
            },
            new()
            {
                Input = @"first([1, 2, 3])",
                Expected = 1
            },
            new()
            {
                Input = @"first([])",
                Expected = Vm.NULL
            },
            new()
            {
                Input = @"first(1)",
                Expected = new Error {Message = "argument to 'first' must be ARRAY, got INTEGER"}
            },
            new()
            {
                Input = @"last([1, 2, 3])",
                Expected = 3
            },
            new()
            {
                Input = @"last([])",
                Expected = Vm.NULL
            },
            new()
            {
                Input = @"last(1)",
                Expected = new Error {Message = "argument to 'last' must be ARRAY, got INTEGER"}
            },
            new()
            {
                Input = @"rest([1, 2, 3])",
                Expected = new[] {2, 3},
            },
            new()
            {
                Input = @"rest([])",
                Expected = Vm.NULL
            },
            new()
            {
                Input = @"push([], 1)",
                Expected = new[] {1}
            },
            new()
            {
                Input = @"push(1, 1)",
                Expected = new Error {Message = "argument to 'push' must be ARRAY, got INTEGER"}
            }
        };
        
        RunVmTests(tests);
    }
    [Test]
    public void TestCallingFunctionsWithWrongArguments()
    {
        var tests = new VmTestCase[]
        {
            new()
            {
                Input = "fn() { 1; }(1);",
                Expected = "wrong number of arguments: want=0, got=1"
            },
            new()
            {
                Input = "fn(a) { a; }();",
                Expected = "wrong number of arguments: want=1, got=0"
            },
            new()
            {
                Input = "fn(a, b) { a + b; }(1);",
                Expected = "wrong number of arguments: want=2, got=1"
            }
        };

        foreach (var test in tests)
        {
            var program = Parse(test.Input);

            var comp = new Compiler();
            var err = comp.Compile(program);
            if (err is not null)
            {
                Assert.Fail($"compiler error: {err}");
            }

            var vm = new Vm(comp.Bytecode());
            err = vm.Run();
            if (err is null)
            {
                Assert.Fail($"expected VM error but resulted in none.");
            }

            if (!err.Equals(test.Expected))
            {
                Assert.Fail($"wrong VM error: want={test.Expected} got={err}");
            }
            
        }
    }
    [Test]
    public void TestCallingFunctionsWithArgumentsAndBindings()
    {
        var tests = new VmTestCase[]
        {
            new()
            {
                Input = @"
                        let identity = fn(a) { a; };
                        identity(4);",
                Expected = 4,
            },
            new()
            {
                Input = @"
                        let sum = fn(a, b) { a + b; };
                        sum(1, 2);",
                Expected = 3

            },
            new()
            {
                Input = @"
                        let sum = fn(a, b) {
                            let c = a + b;
                            c;
                        };
                        sum(1, 2);",
                Expected = 3
            },
            new()
            {
                Input = @"let sum = fn(a, b) { 
                    let c = a + b;
                    c;
                };
                sum(1, 2) + sum(3, 4);",
                Expected = 10
            },
            new()
            {
                Input = @"let sum = fn(a, b) { 
                    let c = a + b;
                    c;
                };
                let outer = fn() { 
                    sum(1, 2) + sum(3, 4);
                };
                outer();",
                Expected = 10
            },
            new()
            {
                Input = @"let globalNum = 10;
                        let sum = fn(a, b) { 
                            let c = a + b;
                            c + globalNum;
                        };
                        let outer = fn() {
                            sum(1, 2) + sum(3, 4) + globalNum;
                        };
                        outer() + globalNum;",
                Expected = 50
            }
            
        };
        
        RunVmTests(tests);
    }
    [Test]
    public void TestCallingFunctionsWithBindings()
    {
        var tests = new VmTestCase[]
        {
            new()
            {
                Input = "let one = fn() { let one = 1; one }; one();",
                Expected = 1
            },
            new()
            {
                Input = "let oneAndTwo = fn() { let one = 1; let two = 2; one + two; }; oneAndTwo();",
                Expected = 3
            },
            new()
            {
                Input = "let oneAndTwo = fn() { let one = 1; let two = 2; one + two; }; let threeAndFour = fn() { let three = 3; let four = 4; three + four; }; oneAndTwo() + threeAndFour();",
                Expected = 10
            },
            new()
            {
                Input = "let firstFoobar = fn() { let foobar = 50; foobar; }; let secondFoobar = fn() { let foobar = 100; foobar; }; firstFoobar() + secondFoobar();",
                Expected = 150
            },
            new()
            {
                Input = "let globalSeed = 50; let minusOne = fn() { let num = 1; globalSeed - num; }; let minusTwo = fn() { let num = 2; globalSeed - num; }; minusOne() + minusTwo();",
                Expected = 97
            }
        };
        
        RunVmTests(tests);
    }
    
    [Test]
    public void TestFirstClassFunctions()
    {
        var tests = new VmTestCase[]
        {
            new()
            {
                Input =
                    @"let returnsOne = fn() { 1; }; let returnsOneReturner = fn() { returnsOne; }; returnsOneReturner()();",
                Expected = 1
            }
        };
        
        RunVmTests(tests);
    }
    [Test]
    public void TestFunctionsWithoutReturnValue()
    {
        var tests = new VmTestCase[]
        {
            new()
            {
                Input = @"let noReturn = fn() { }; noReturn();",
                Expected = Vm.NULL
            },
            new()
            {
                Input = @"let noReturn = fn() { }; let noReturnTwo = fn() { noReturn(); }; noReturn(); noReturnTwo();",
                Expected = Vm.NULL
            }
        };

        RunVmTests(tests);
    }
    [Test]
    public void TestCallingFunctionsWithoutArguments()
    {
        var tests = new VmTestCase[]
        {
            new()
            {
                Input = @"let fivePlusTen = fn() { 5 + 10; }; fivePlusTen();",
                Expected = 15
            },
            new()
            {
                Input = @"let earlyExit = fn() { return 99; 100; }; earlyExit();",
                Expected = 99
            },
            new()
            {
                Input = @"let earlyExit = fn() {return 99; return 100; }; earlyExit();",
                Expected = 99
            }
        };
        RunVmTests(tests);
    }
    [Test]
    public void TestIndexExpressions()
    {
        var tests = new VmTestCase[]
        {
            new()
            {
                Input = "[1, 2, 3][1]", Expected = 2
            },
            new()
            {
                Input = "[1, 2, 3][0 + 2]", Expected = 3
            },
            new()
            {
                Input = "[[1, 1, 1]][0][0]", Expected = 1
            },
            new()
            {
                Input = "[][0]", Expected = Vm.NULL
            },
            new()
            {
                Input = "[1, 2, 3][99]", Expected = Vm.NULL
            },
            new()
            {
                Input = "[1][-1]", Expected = Vm.NULL
            },
            new()
            {
                Input = "{1: 1, 2: 2}[1]", Expected = 1
            },
            new()
            {
                Input = "{1: 1, 2: 2}[2]", Expected = 2
            },
            new()
            {
                Input = "{1: 1}[0]", Expected = Vm.NULL
            },
            new()
            {
                Input = "{}[0]", Expected = Vm.NULL
            }
        };
        
        RunVmTests(tests);
    }
    
    [Test]
    public void TestHashLiterals()
    {

        var tests = new VmTestCase[]
        {
            new()
            {
                Input = "{}", Expected = new Dictionary<HashKey, int> { }
            },
            new()
            {
                Input = "{1: 2, 2: 3}",
                Expected = new Dictionary<HashKey, int>()
                {
                    {(new Integer {Value = 1}).HashKey(), 2},
                    {(new Integer {Value = 2}).HashKey(), 3}
                }
            },
            new()
            {
                Input = "{1 + 1: 2 * 2, 3 + 3: 4 * 4}",
                Expected = new Dictionary<HashKey, int>
                {
                    {(new Integer {Value = 2}).HashKey(), 4},
                    {(new Integer {Value = 6}).HashKey(), 16}
                }
            }
        };
        
        RunVmTests(tests);
    }
    [Test]
    public void TestArrayLiterals()
    {
        var tests = new VmTestCase[]
        {
            new()
            {
                Input = "[]",
                Expected = new int[] { }
            },
            new()
            {
                Input = "[1, 2, 3]",
                Expected = new int[] {1, 2, 3}
            },
            new()
            {
                Input = "[1 + 2, 3 * 4, 5 + 6]",
                Expected = new[] {3, 12, 11}
            }
        };
        
        RunVmTests(tests);
    }
    [Test]
    public void TestStringExpressions()
    {
        var tests = new VmTestCase[]
        {
            new()
            {
                Input = @"""monkey""", Expected = $"monkey"
            },
            new()
            {
                Input = @"""mon"" + ""key""", Expected = "monkey"
            },
            new()
            {
                Input = @"""mon"" + ""key"" + ""banana""", Expected = "monkeybanana"
            }
        };
        
        RunVmTests(tests);
    }
    [Test]
    public void TestGlobalLetStatements()
    {
        var tests = new VmTestCase[]
        {
            new()
            {
                Input = "let one = 1; one", Expected = 1
            },
            new()
            {
                Input = "let one = 1; let two = 2; one + two", Expected = 3
            },
            new()
            {
                Input = "let one = 1; let two = one + one; one + two", Expected = 3
            },
            new()
            {
                Input = "let one = 1; let two = 2; let three = one + two; three", Expected = 3
            }
        };
        
        RunVmTests(tests);
    }
    [Test]
    public void TestConditionals()
    {
        var tests = new VmTestCase[] 
        {
            new()
            {
                Input = "if (true) { 10 }", Expected = 10
            },
            new()
            {
                Input = "if (true) { 10 } else { 20 }", Expected = 10
            },
            new()
            {
                Input = "if (false) { 10 } else { 20 } ", Expected = 20
            },
            new()
            {
                Input = "if (1) { 10 }", Expected = 10
            },
            new()
            {
                Input = "if (1 < 2) { 10 }", Expected = 10
            },
            new()
            {
                Input = "if (1 < 2) { 10 } else { 20 }", Expected = 10
            },
            new()
            {
                Input = "if (1 > 2) { 10 } else { 20}", Expected = 20
            },
            new()
            {
                Input = "if (1 > 2) { 10 }", Expected = Vm.NULL,
            },
            new()
            {
                Input = "if (false) { 10 }", Expected = Vm.NULL
            },
            new()
            {
                Input = "if ((if (false) { 10 })) { 10 } else { 20 }", Expected = 20
            }
        };
        
        RunVmTests(tests);
    }
    [Test]
    public void TestBooleanExpressions()
    {
        var tests = new VmTestCase[]
        {
            new() 
            {
                Input = "true",
                Expected = true
            },
            new() 
            {
                Input = "false",
                Expected = false
            },
            new() 
            {
                Input = "1 < 2",
                Expected = true
            },
            new() 
            {
                Input = "1 > 2",
                Expected = false
            },
            new() 
            {
                Input = "1 < 1",
                Expected = false
            },
            new() 
            {
                Input = "1 > 1",
                Expected = false
            },
            new() 
            {
                Input = "1 == 1",
                Expected = true
            },
            new() 
            {
                Input = "1 != 1",
                Expected = false
            },
            new() 
            {
                Input = "1 == 2",
                Expected = false
            },
            new() 
            {
                Input = "1 != 2",
                Expected = true
            },
            new() 
            {
                Input = "true == true",
                Expected = true
            },
            new() 
            {
                Input = "false == false",
                Expected = true
            },
            new() 
            {
                Input = "true == false",
                Expected = false
            },
            new() 
            {
                Input = "true != false",
                Expected = true
            },
            new() 
            {
                Input = "(1 < 2) == true",
                Expected = true
            },
            new() 
            {
                Input = "(1 < 2) == false",
                Expected = false
            },
            new() 
            {
                Input = "(1 > 2) == true",
                Expected = false
            },
            new() 
            {
                Input = "(1 > 2) == false",
                Expected = true
            },
            new()
            {
                Input = "!true",
                Expected = false
            },
            new()
            {
                Input = "!false",
                Expected = true
            },
            new()
            {
                Input = "!5",
                Expected = false
            },
            new()
            {
                Input = "!!true",
                Expected = true
            },
            new()
            {
                Input = "!!false",
                Expected = false
            },
            new()
            {
                Input = "!!5",
                Expected = true
            },
            new()
            {
                Input = "!(if (false) { 5; })", Expected = true
            },
        };
        
        RunVmTests(tests);
    }
    [Test]
    public void TestIntegerArithmetic()
    {
        var tests = new[]
        {
            new VmTestCase
            {
                Input = "1",
                Expected = 1
            },
            new VmTestCase
            {
                Input = "2",
                Expected = 2
            },
            new VmTestCase
            {
                Input = "1 + 2",
                Expected = 3
            },
            new VmTestCase
            {
                Input = "1 - 2",
                Expected = -1
            },
            new VmTestCase
            {
                Input = "1 * 2",
                Expected = 2
            },
            new()
            {
                Input = "4 / 2",
                Expected = 2
            },
            new()
            {
                Input = "50 / 2 * 2 + 10 - 5",
                Expected = 55
            },
            new()
            {
                Input = "5 + 5 + 5 + 5 -10",
                Expected = 10
            },
            new()
            {
                Input = "2 * 2 * 2 * 2 * 2",
                Expected = 32
            },
            new()
            {
                Input = "5 * 2 + 10",
                Expected = 20
            },
            new()
            {
                Input = "5 + 2 * 10", Expected = 25
            },
            new()
            {
                Input = "5 * (2 + 10)", Expected = 60
            },
            new()
            {
                Input = "-5", Expected = -5
            },
            new()
            {
                Input = "-10", Expected = -10
            },
            new()
            {
                Input = "-50 + 100 + -50", Expected = 0
            },
            new()
            {
                Input = "(5 + 10 * 2 + 15 / 3) * 2 + -10", Expected = 50
            },
        };
        
        RunVmTests(tests);
    }
    private class VmTestCase
    {
        public string Input { get; set; }
        public object Expected { get; set; }
    }

    private void RunVmTests(VmTestCase[] tests)
    {
        foreach (var tt in tests)
        {
            // Console.WriteLine($"{JsonSerializer.Serialize(tt)}");
            var program = Parse(tt.Input);

            var comp = new Compiler();
            var err = comp.Compile(program);

            if (err is not null)
            {
                Assert.Fail($"compiler error: {err}");
            }

            var vm = new Vm(comp.Bytecode());
            err = vm.Run();
            if (err is not null)
            {
                Assert.Fail($"vm error: {err}");
            }

            var stackElem = vm.LastPoppedStackElem();

            TestExpectedObject(tt.Expected, stackElem);
        }
    }

    private void TestExpectedObject(object expected, Object actual)
    {
        if (expected is int integer)
        {
            var err = TestIntegerObject(integer, actual);
            if (err is not null)
            {
                Assert.Fail($"TestIntegerObject failed: {err}");
            }
        }

        if (expected is bool b)
        {
            var err = TestBooleanObject(b, actual);
            if (err is not null)
            {
                Assert.Fail($"TestBooleanObject failed: {err}");
            }
        }

        if (expected is Null n)
        {
            if (actual is not Null)
            {
                Assert.Fail($"object is not Null: {actual.Type()}");
            }
        }

        if (expected is string s)
        {
            var err = TestStringObject(s, actual);
            if (err is not null)
            {
                Assert.Fail($"TestStringObject failed: {err}");
            }
        }

        if (expected is int[] arr)
        {
            if (actual is not Array arrObj)
            {
                Assert.Fail($"object not Array: {actual}");
                return;
            }

            if (arrObj.Elements.Count != arr.Length)
            {
                Assert.Fail($"wrong num of elements. want={arr.Length}, got={arrObj.Elements.Count}");
            }

            for (var i = 0; i < arr.Length; i++)
            {
                var exepctedElem = arr[i];
                var err = TestIntegerObject(exepctedElem, arrObj.Elements[i]);
                if (err is not null)
                {
                    Assert.Fail($"TestIntegerObject failed: {err}");
                }
            }
        }

        if (expected is Dictionary<HashKey, int> dict)
        {
            if (actual is not Hash hash)
            {
                Assert.Fail($"object is not Hash. Got {actual}");
                return;
            }

            if (hash.Pairs.Count != dict.Count)
            {
                Assert.Fail($"hash has wrong number of Pairs. want={dict.Count}, got={hash.Pairs.Count}");
            }

            foreach (var (expectedKey, expectedValue) in dict)
            {
                if (!hash.Pairs.ContainsKey(expectedKey))
                {
                    Assert.Fail($"no pair for given key in Pairs");
                }

                var err = TestIntegerObject(expectedValue, hash.Pairs[expectedKey].Value);
                if (err is not null)
                {
                    Assert.Fail($"TestIntegerObject failed: {err}");
                }
            }
        }

        if (expected is Error error)
        {
            if (actual is not Error actualErr)
            {
                Assert.Fail($"object is not Error: {actual}");
                return;
            }

            if (!error.Message.Equals(actualErr.Message))
            {
                Assert.Fail($"wrong error message. expected={error.Message}, got={actualErr.Message}");
            }
        }
    }

    private string? TestStringObject(string expected, Object actual)
    {
        if (actual is not String str)
        {
            return $"object is not String. Got={actual}";
        }

        if (!str.Value.Equals(expected))
        {
            return $"object has wrong value. got={str.Value}, want={expected}";
        }

        return null;
    }

    private string? TestBooleanObject(bool expected, Object actual)
    {
        if (actual is not Boolean b)
        {
            return $"object is not Boolean. Got '{actual}'";
        }

        if (b.Value != expected)
        {
            return $"object has wrong value, got '{b.Value}', want '{expected}'";
        }

        return null;
    }

    private static Program Parse(string input)
    {
        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        return parser.ParseProgram();
    }

    private static string? TestIntegerObject(int expected, Object actual)
    {
        if (actual is not Integer result)
        {
            return $"object is not Integer. Got '{actual}'";
        }

        if (result.Value != expected)
        {
            return $"object has wrong value. Got '{result.Value}'. Want '{expected}'";
        }

        return null;
    }
}