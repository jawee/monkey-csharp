using System.Linq.Expressions;
using System.Reflection.Emit;
using Monkey.Core.AST;
using Monkey.Core.Code;
using Monkey.Core.Compiler;
using Monkey.Core.Object;
using Object = Monkey.Core.Object.Object;

namespace Monkey.Test;

public class CompilerTest
{
    [Test]
    public void TestConditionals()
    {
        var tests = new List<CompilerTestCase>
        {
            new()
            {
                Input = "if (true) { 10 }; 3333;",
                ExpectedConstants = new List<int> {10, 3333},
                ExpectedInstructions = new List<Instructions>
                {
                    // 0000
                    Code.Make(Opcode.OpTrue),
                    // 0001
                    Code.Make(Opcode.OpJumpNotTruthy, new List<int> {10}),
                    // 0004
                    Code.Make(Opcode.OpConstant, new List<int> {0}),
                    // 0007
                    Code.Make(Opcode.OpJump, new List<int> {11}),
                    // 0010
                    Code.Make(Opcode.OpNull),
                    // 0011
                    Code.Make(Opcode.OpPop),
                    // 0012
                    Code.Make(Opcode.OpConstant, new List<int> {1}),
                    // 0015
                    Code.Make(Opcode.OpPop)
                }
            },
            new()
            {
                Input = "if (true) { 10 } else { 20 }; 3333;",
                ExpectedConstants = new List<int> {10, 20, 3333},
                ExpectedInstructions = new List<Instructions>
                {
                    // 0000
                    Code.Make(Opcode.OpTrue),
                    // 0001
                    Code.Make(Opcode.OpJumpNotTruthy, new List<int> {10}),
                    // 0004
                    Code.Make(Opcode.OpConstant, new List<int> {0}),
                    // 0007
                    Code.Make(Opcode.OpJump, new List<int> {13}),
                    // 0010
                    Code.Make(Opcode.OpConstant, new List<int> {1}),
                    // 0013
                    Code.Make(Opcode.OpPop),
                    // 0014
                    Code.Make(Opcode.OpConstant, new List<int> {2}),
                    // 0017
                    Code.Make(Opcode.OpPop)
                }
            },
            new()
            {
                Input = "if (true) { 10 };",
                ExpectedConstants = new List<int> {10},
                ExpectedInstructions = new List<Instructions>
                {
                    // 0000
                    Code.Make(Opcode.OpTrue),
                    // 0001
                    Code.Make(Opcode.OpJumpNotTruthy, new List<int> {10}),
                    // 0004
                    Code.Make(Opcode.OpConstant, new List<int> {0}),
                    // 0007
                    Code.Make(Opcode.OpJump, new List<int> {11}),
                    // 0010
                    Code.Make(Opcode.OpNull),
                    // 0011
                    Code.Make(Opcode.OpPop),
                }
            },
        };
        
        RunCompilerTests(tests);
    }
    [Test]
    public void TestBooleanExpressions()
    {
        var tests = new List<CompilerTestCase> 
        {
            new()
            {
                Input = "true",
                ExpectedConstants = new List<int>(),
                ExpectedInstructions = new List<Instructions>
                {
                    Code.Make(Opcode.OpTrue),
                    Code.Make(Opcode.OpPop)
                }
            },
            new()
            {
                Input = "false",
                ExpectedConstants = new List<int>(),
                ExpectedInstructions = new List<Instructions>
                {
                    Code.Make(Opcode.OpFalse),
                    Code.Make(Opcode.OpPop)
                }
            },
            new()
            {
                Input = "1 > 2",
                ExpectedConstants = new List<int> {1, 2},
                ExpectedInstructions = new List<Instructions>
                {
                    Code.Make(Opcode.OpConstant, new List<int> {0}),
                    Code.Make(Opcode.OpConstant, new List<int> {1}),
                    Code.Make(Opcode.OpGreaterThan),
                    Code.Make(Opcode.OpPop)
                }
            },
            new()
            {
                Input = "1 < 2",
                ExpectedConstants = new List<int> {2, 1},
                ExpectedInstructions = new List<Instructions>
                {
                    Code.Make(Opcode.OpConstant, new List<int> {0}),
                    Code.Make(Opcode.OpConstant, new List<int> {1}),
                    Code.Make(Opcode.OpGreaterThan),
                    Code.Make(Opcode.OpPop)
                }
            },
            new()
            {
                Input = "1 == 2",
                ExpectedConstants = new List<int> {1, 2},
                ExpectedInstructions = new List<Instructions>
                {
                    Code.Make(Opcode.OpConstant, new List<int> {0}),
                    Code.Make(Opcode.OpConstant, new List<int> {1}),
                    Code.Make(Opcode.OpEqual),
                    Code.Make(Opcode.OpPop)
                }
            },
            new()
            {
                Input = "1 != 2",
                ExpectedConstants = new List<int> {1, 2},
                ExpectedInstructions = new List<Instructions>
                {
                    Code.Make(Opcode.OpConstant, new List<int> {0}),
                    Code.Make(Opcode.OpConstant, new List<int> {1}),
                    Code.Make(Opcode.OpNotEqual),
                    Code.Make(Opcode.OpPop)
                }
            },
            new()
            {
                Input = "true == false",
                ExpectedConstants = new List<int> {},
                ExpectedInstructions = new List<Instructions>
                {
                    Code.Make(Opcode.OpTrue),
                    Code.Make(Opcode.OpFalse),
                    Code.Make(Opcode.OpEqual),
                    Code.Make(Opcode.OpPop)
                }
            },
            new()
            {
                Input = "true != false",
                ExpectedConstants = new List<int> {},
                ExpectedInstructions = new List<Instructions>
                {
                    Code.Make(Opcode.OpTrue),
                    Code.Make(Opcode.OpFalse),
                    Code.Make(Opcode.OpNotEqual),
                    Code.Make(Opcode.OpPop)
                }
            },
            new()
            {
                Input = "!true",
                ExpectedConstants = new List<int>(),
                ExpectedInstructions = new List<Instructions>
                {
                    Code.Make(Opcode.OpTrue),
                    Code.Make(Opcode.OpBang),
                    Code.Make(Opcode.OpPop)
                }
            }
        };
        
        RunCompilerTests(tests);
    }
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
                ExpectedInstructions = new List<Instructions> {Code.Make(Opcode.OpConstant, new List<int> {0}), Code.Make(Opcode.OpConstant, new List<int> {1}), Code.Make(Opcode.OpAdd), Code.Make(Opcode.OpPop)}
            },
            new()
            {
                Input = "1; 2",
                ExpectedConstants = new List<int> {1, 2},
                ExpectedInstructions = new List<Instructions> {Code.Make(Opcode.OpConstant, new List<int> {0}), Code.Make(Opcode.OpPop), Code.Make(Opcode.OpConstant, new List<int> {1}), Code.Make(Opcode.OpPop)}
            },
            new()
            {
                Input = "1 - 2",
                ExpectedConstants = new List<int> {1, 2},
                ExpectedInstructions = new List<Instructions> {Code.Make(Opcode.OpConstant, new List<int> {0}), Code.Make(Opcode.OpConstant, new List<int> {1}), Code.Make(Opcode.OpSub), Code.Make(Opcode.OpPop)}
            },
            new()
            {
                Input = "1 * 2",
                ExpectedConstants = new List<int> {1, 2},
                ExpectedInstructions = new List<Instructions> {Code.Make(Opcode.OpConstant, new List<int> {0}), Code.Make(Opcode.OpConstant, new List<int> {1}), Code.Make(Opcode.OpMul), Code.Make(Opcode.OpPop)}
            },
            new()
            {
                Input = "1 / 2",
                ExpectedConstants = new List<int> {1, 2},
                ExpectedInstructions = new List<Instructions> {Code.Make(Opcode.OpConstant, new List<int> {0}), Code.Make(Opcode.OpConstant, new List<int> {1}), Code.Make(Opcode.OpDiv), Code.Make(Opcode.OpPop)}
            },
            new()
            {
                Input = "-1",
                ExpectedConstants = new List<int> {1},
                ExpectedInstructions = new List<Instructions> {Code.Make(Opcode.OpConstant, new List<int> {0}), Code.Make(Opcode.OpMinus), Code.Make(Opcode.OpPop)}
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

    private string? TestInstructions(List<Instructions> expected, Instructions actual)
    {
        var concatted = ConcatInstructions(expected);

        if (actual.Count != concatted.Count)
        {
            return $"wrong instructions length. Want \n'{concatted.String()}',\n Got \n'{actual.String()}'";
        }

        for (var i = 0; i < concatted.Count; i++)
        {
            var ins = concatted[i];
            if (actual[i] != ins)
            {
                return $"wrong instruction at {i}. Want '{concatted.String()}' Got '{actual.String()}'";
            }
        }

        return null;
    }

    private Instructions ConcatInstructions(List<Instructions> s)
    {
        var res = new Instructions();

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