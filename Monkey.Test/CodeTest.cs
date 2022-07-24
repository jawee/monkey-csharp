using Monkey.Core.Code;

namespace Monkey.Test;

public class CodeTest
{
    [Test]
    public void TestReadOperands()
    {
        var tests = new[]
        {
            new
            {
                Op = Opcode.OpConstant,
                Operands = new List<int> {65535},
                BytesRead = 2
            },
            new
            {
                Op = Opcode.OpGetLocal,
                Operands = new List<int> {255},
                BytesRead = 1
            },
            new
            {
                Op = Opcode.OpClosure,
                Operands = new List<int> {65535, 255},
                BytesRead = 3
            }
        };

        foreach (var tt in tests)
        {
            var instructions = Code.Make(tt.Op, tt.Operands);

            var (def, err) = Code.Lookup(tt.Op);
            if (err is not null)
            {
                Assert.Fail($"definition not found: {err}");
            }

            var ins = instructions.GetRange(1, instructions.Count - 1);
            var newIns = new Instructions();
            newIns.AddRange(ins);

            var (operandsRead, n) = Code.ReadOperands(def, newIns);

            if (n != tt.BytesRead)
            {
                Assert.Fail($"n wrong. Want '{tt.BytesRead}' Got '{n}'");
            }

            for (var i = 0; i < tt.Operands.Count; i++)
            {
                var want = tt.Operands[i];
                if (operandsRead[i] != want)
                {
                    Assert.Fail($"operand wrong. want '{want}', got '{operandsRead[i]}'");
                }
            }
        }
    }
    [Test]
    public void TestInstructionsString()
    {
        var instructions = new List<Instructions>
        {
            Code.Make(Opcode.OpAdd, new List<int>()),
            Code.Make(Opcode.OpGetLocal, new List<int> {1}),
            Code.Make(Opcode.OpConstant, new List<int>(){ 2 }),
            Code.Make(Opcode.OpConstant, new List<int>(){ 65535 }),
            Code.Make(Opcode.OpClosure, new() {65535, 255})
        };

        var expected = @"0000 OpAdd 0001 OpGetLocal 1 0003 OpConstant 2 0006 OpConstant 65535 0009 OpClosure 65535 255";

        var concatted = new Instructions();
        foreach (var ins in instructions)
        {
            concatted.AddRange(ins);
        }

        if (!concatted.String().Equals(expected))
        {
            Assert.Fail($"instructions wrongly formatted. Want '{expected}', Got '{concatted.String()}'");
        }
    }
    
    [Test]
    public void TestMake()
    {
        var tests = new[]
        {
            new
            {
                Op = Opcode.OpConstant,
                Operands = new List<int> {65534},
                Expected = new List<byte> {(byte) Opcode.OpConstant, 254, 255}
            },
            new
            {
                Op = Opcode.OpAdd,
                Operands = new List<int>(),
                Expected = new List<byte> {(byte) Opcode.OpAdd}
            },
            new
            {
                Op = Opcode.OpGetLocal,
                Operands = new List<int> {255},
                Expected = new List<byte> {(byte) Opcode.OpGetLocal, 255}
            },
            new
            {
                Op = Opcode.OpClosure,
                Operands = new List<int> {65534, 255},
                Expected = new List<byte> {(byte) Opcode.OpClosure, 254, 255, 255}
            }
        };

        foreach (var test in tests)
        {
            var instruction = Code.Make(test.Op, test.Operands);

            if (instruction.Count != test.Expected.Count)
            {
                Assert.Fail($"instruction has wrong length. want '{test.Expected.Count}', got '{instruction.Count}'");
            }

            for (var i = 0; i < test.Expected.Count; i++)
            {
                var b = test.Expected[i];
                if (instruction[i] != b)
                {
                    Assert.Fail($"wrong byte at pos {i}. Want '{b}' Got '{instruction[i]}'");
                }
            }
        }
    }
}