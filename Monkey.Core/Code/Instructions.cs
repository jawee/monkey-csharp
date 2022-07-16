using System.Text;

namespace Monkey.Core.Code;
public class Instructions : List<byte>
{
    public string String()
    {
        var builder = new StringBuilder();

        var i = 0;
        while (i < Count)
        {
            var (def, err) = Code.Lookup((Opcode) this[i]);
            if (err is not null)
            {
                builder.Append($"ERROR: {err}");
                continue;
            }

            var bytes = GetRange(i + 1, Count - i - 1);

            var newInstr = new Instructions();
            newInstr.AddRange(bytes);

            var (operands, read) = Code.ReadOperands(def, newInstr);
            
            builder.Append($"{i:0000} {FmtInstruction(def, operands)} ");

            i += 1 + read;
        }

        builder.Remove(builder.Length - 1, 1);

        return builder.ToString();
    }

    private string FmtInstruction(Definition def, List<int> operands)
    {
        var operandCount = def.OperandWidths.Count;

        if (operands.Count != operandCount)
        {
            return $"ERROR: operand len {operands.Count} does not match defined {operandCount}";
        }

        switch (operandCount)
        {
            case 0:
                return def.Name;
            case 1:
                return $"{def.Name} {operands[0]}";
        }

        return $"ERROR: unhandled operandCount for {def.Name}";
    }
}