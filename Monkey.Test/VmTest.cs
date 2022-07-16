using System.Text.Json;
using System.Text.Json.Serialization;
using Monkey.Core.AST;
using Monkey.Core.Compiler;
using Monkey.Core.Object;
using Monkey.Core.Vm;
using Boolean = Monkey.Core.Object.Boolean;
using Object = Monkey.Core.Object.Object;

namespace Monkey.Test;

public class VmTest
{

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
            Console.WriteLine($"{JsonSerializer.Serialize(tt)}");
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
        if (expected is int i)
        {
            var err = TestIntegerObject(i, actual);
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