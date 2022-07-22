using System.Text;
using Monkey.Core.Object;

namespace Monkey.Core.Code;

public enum Opcode : byte
{
    OpConstant,
    OpAdd,
    OpSub,
    OpMul,
    OpDiv,
    OpPop,
    OpTrue,
    OpFalse,
    OpEqual,
    OpNotEqual,
    OpGreaterThan,
    OpMinus,
    OpBang,
    OpJumpNotTruthy,
    OpJump,
    OpNull,
    OpGetGlobal,
    OpSetGlobal,
    OpArray,
    OpHash,
    OpIndex,
    OpCall,
    OpReturnValue,
    OpReturn,
    OpGetLocal,
    OpSetLocal
}

public class Code
{
    private static Dictionary<Opcode, Definition> definitions = new()
    {
        {Opcode.OpConstant, new Definition { Name = "OpConstant", OperandWidths = new List<int> {2}}},
        {Opcode.OpAdd, new Definition { Name = "OpAdd", OperandWidths = new List<int> {}}},
        {Opcode.OpPop, new Definition { Name = "OpPop", OperandWidths = new List<int> {}}},
        {Opcode.OpSub, new Definition { Name = "OpPop", OperandWidths = new List<int> {}}},
        {Opcode.OpMul, new Definition { Name = "OpPop", OperandWidths = new List<int> {}}},
        {Opcode.OpDiv, new Definition { Name = "OpPop", OperandWidths = new List<int> {}}},
        {Opcode.OpTrue, new Definition { Name = "OpTrue", OperandWidths = new List<int> {}}},
        {Opcode.OpFalse, new Definition { Name = "OpFalse", OperandWidths = new List<int> {}}},
        {Opcode.OpEqual, new Definition { Name = "OpEqual", OperandWidths = new List<int> {}}},
        {Opcode.OpNotEqual, new Definition { Name = "OpNotEqual", OperandWidths = new List<int> {}}},
        {Opcode.OpGreaterThan, new Definition { Name = "OpGreaterThan", OperandWidths = new List<int> {}}},
        {Opcode.OpMinus, new Definition { Name = "OpMinus", OperandWidths = new List<int> {}}},
        {Opcode.OpBang, new Definition { Name = "OpBang", OperandWidths = new List<int> {}}},
        {Opcode.OpJumpNotTruthy, new Definition { Name = "OpJumpNotTruthy", OperandWidths = new List<int> {2}}},
        {Opcode.OpJump, new Definition { Name = "OpJump", OperandWidths = new List<int> {2}}},
        {Opcode.OpNull, new Definition { Name = "OpNull", OperandWidths = new List<int>{}}},
        {Opcode.OpGetGlobal, new Definition { Name = "OpGetGlobal", OperandWidths = new List<int> {2}}},
        {Opcode.OpSetGlobal, new Definition { Name = "OpSetGlobal", OperandWidths = new List<int> {2}}},
        {Opcode.OpArray, new Definition { Name = "OpArray", OperandWidths = new List<int> {2}}},
        {Opcode.OpHash, new Definition { Name = "OpHash", OperandWidths = new List<int> {2}}},
        {Opcode.OpIndex, new Definition { Name = "OpIndex", OperandWidths = new List<int> {}}},
        {Opcode.OpCall, new Definition { Name = "OpCall", OperandWidths = new List<int> {1}}},
        {Opcode.OpReturnValue, new Definition { Name = "OpReturnValue", OperandWidths = new List<int> {}}},
        {Opcode.OpReturn, new Definition { Name = "OpReturn", OperandWidths = new List<int> {}}},
        {Opcode.OpGetLocal, new Definition { Name = "OpGetLocal", OperandWidths = new List<int> {1}}},
        {Opcode.OpSetLocal, new Definition { Name = "OpSetLocal", OperandWidths = new List<int> {1}}},
    };

    public static (Definition?, string?) Lookup(Opcode op)
    {
        if (definitions.ContainsKey(op))
        {
            return (definitions[op], null);
        }
        
        
        return (null, $"opcode {op} undefined");
    }

    public static Instructions Make(Opcode op, List<int>? operands = null)
    {
        if (operands is null)
        {
            operands = new List<int>();
        }
        if (!definitions.ContainsKey(op))
        {
            return new Instructions();
        }

        var def = definitions[op];

        var instructionLen = 1;

        foreach (var w in def.OperandWidths)
        {
            instructionLen += w;
        }

        var instruction = new Instructions();
        instruction.Add((byte) op);

        var offset = 1;
        for (var i = 0; i < operands.Count; i++)
        {
            var o = operands[i];
            var width = def.OperandWidths[i];
            switch (width)
            {
                case 2:
                    // var uo = Convert.ToUInt16(o);
                    var source = new [] {o};
                    var target = new byte[source.Length * 2]; 
                    Buffer.BlockCopy(source, 0, target, 0, source.Length * 2);
                    instruction.AddRange(target);
                    break; 
                case 1:
                    if (offset >= instruction.Count)
                    {
                        instruction.Add((byte)o);
                    }
                    else
                    {
                        instruction[offset] = (byte) o;
                    }
                    break;
            }

            offset += width;
        }

        return instruction;
    }

    public static (List<int>, int) ReadOperands(Definition def, Instructions ins)
    {
        var operands = new List<int>(def.OperandWidths);
        var offset = 0;

        for (var i = 0; i < def.OperandWidths.Count; i++)
        {
            var width = def.OperandWidths[i];
            switch (width)
            {
                case 2:
                    var bytes = ins.GetRange(offset, ins.Count - offset);
                    var newInstr = new Instructions();
                    newInstr.AddRange(bytes);
                    operands[i] = ReadUint16(newInstr);
                    break;
                case 1:
                    bytes = ins.GetRange(offset, ins.Count - offset);
                    newInstr = new Instructions();
                    newInstr.AddRange(bytes);
                    operands[i] = ReadUint16(newInstr);
                    break;
            }

            offset += width;
        }

        return (operands, offset);
    }

    public static UInt16 ReadUint16(Instructions ins)
    {
        if (ins.Count == 1)
        {
            ins.Add(0);
        }
        var val = BitConverter.ToUInt16(ins.ToArray(), 0);
        return val;
    }
}