using Monkey.Core.Code;

namespace Monkey.Core.Compiler;

public class CompilationScope
{
    public Instructions Instructions { get; set; }
    public EmittedInstruction LastInstruction { get; set; }
    public EmittedInstruction PreviousInstruction { get; set; }
}