using Microsoft.VisualBasic;
using Monkey.Core.AST;
using Monkey.Core.Compiler;
using Monkey.Core.Object;
using Monkey.Core.Vm;
using Object = Monkey.Core.Object.Object;

namespace Monkey.Test;

public class VmTest
{
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
            }
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

            var stackElem = vm.StackTop();

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
            return $"objeect is not Integer. Got '{actual}'";
        }

        if (result.Value != expected)
        {
            return $"object has wrong value. Got '{result.Value}'. Want '{expected}'";
        }

        return null;
    }
}