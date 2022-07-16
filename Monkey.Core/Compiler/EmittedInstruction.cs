using Monkey.Core.Code;

namespace Monkey.Core.Compiler;

public class EmittedInstruction
{
    public Opcode? Opcode { get; set; }
    public int? Position { get; set; }
}