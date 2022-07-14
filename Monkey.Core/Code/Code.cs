using System.Text;
using Monkey.Core.Object;

namespace Monkey.Core.Code;

public enum Opcode : byte
{
    OpConstant,
    OpAdd
}

public class Code
{
    private static Dictionary<Opcode, Definition> definitions = new()
    {
        {Opcode.OpConstant, new Definition { Name = "OpConstant", OperandWidths = new List<int> {2}}},
        {Opcode.OpAdd, new Definition { Name = "OpAdd", OperandWidths = new List<int> {}}}
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
            }

            offset += width;
        }

        return (operands, offset);
    }

    public static UInt16 ReadUint16(Instructions ins)
    {
        var val = BitConverter.ToUInt16(ins.ToArray(), 0);
        return val;
    }
}