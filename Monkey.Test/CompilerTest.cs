using System.Linq.Expressions;
using Monkey.Core.AST;
using Monkey.Core.Code;
using Monkey.Core.Compiler;
using Monkey.Core.Object;
using Object = Monkey.Core.Object.Object;

namespace Monkey.Test;

public class CompilerTest
{
    private struct CompilerTestCase
    {
        public string Input { get; set; }
        public List<int> ExpectedConstants { get; set; }
        public List<Instructions> ExpectedInstructions { get; set; }
    }
    [Test]
    public void TestIntegerArithmetic()
    {
        var tests = new List<CompilerTestCase>
        {
            new()
            {
                Input = "1 + 2",
                ExpectedConstants = new List<int> {1, 2},
                ExpectedInstructions = new List<Instructions> {Code.Make(Opcode.OpConstant, new List<int> {0}), Code.Make(Opcode.OpConstant, new List<int> {1}), Code.Make(Opcode.OpAdd)}
            }
        };

        RunCompilerTests(tests);
    }

    private void RunCompilerTests(List<CompilerTestCase> tests)
    {
        foreach (var tt in tests)
        {
            var program = Parse(tt.Input);

            var compiler = new Compiler();
            var err = compiler.Compile(program);
            if (err != null)
            {
                Assert.Fail($"compiler error: {err}");
            }

            var bytecode = compiler.Bytecode();

            err = TestInstructions(tt.ExpectedInstructions, bytecode.Instructions);
            if (err != null)
            {
                Assert.Fail($"TestInstructions failed: {err}");
            }

            err = TestConstants(tt.ExpectedConstants, bytecode.Constants);
            if (err != null)
            {
                Assert.Fail($"TestConstants failed: {err}");
            }

        }
    }

    private string? TestConstants(List<int> expected, List<Object> actual)
    {
        if (expected.Count != actual.Count)
        {
            return $"wrong number of constants. Got '{actual.Count}'. Want '{expected.Count}'";
        }

        for (var i = 0; i < expected.Count; i++)
        {
            var constant = expected[i];
            if (constant is int)
            {
                var err = TestIntegerObject(constant, actual[i]);
                if (err is not null)
                {
                    return $"constant {i} - TestIntegerObject failed: {err}";
                }
            }
        }

        return null;
    }

    private string? TestIntegerObject(int expected, Object actual)
    {
        if (actual is not Integer result)
        {
            return $"object is not Integer. Got '{actual}'";
        }

        if (result.Value != expected)
        {
            return $"object has wrong value. Got '{result.Value}' Want '{expected}'";
        }

        return null;
    }

    private string? TestInstructions(List<Instructions> expected, List<byte> actual)
    {
        var concatted = ConcatInstructions(expected);

        if (actual.Count != concatted.Count)
        {
            return $"wrong instructions length. Want '{concatted.Count}', Got '{actual.Count}'";
        }

        for (var i = 0; i < concatted.Count; i++)
        {
            var ins = concatted[i];
            if (actual[i] != ins)
            {
                return $"wrong instruction at {i}. Want '{concatted}' Got '{actual}'";
            }
        }

        return null;
    }

    private List<byte> ConcatInstructions(List<Instructions> s)
    {
        var res = new List<byte>();

        foreach (var ins in s)
        {
            res.AddRange(ins);
        }

        return res;
    }

    private Program Parse(string input)
    {
        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        return parser.ParseProgram();
    }
}