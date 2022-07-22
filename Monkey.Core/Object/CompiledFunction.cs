using Monkey.Core.Code;

namespace Monkey.Core.Object;

public class CompiledFunction : Object
{
    public Instructions Instructions { get; set; }
    public int NumLocals { get; set; }
    public int NumParameters { get; set; }

    public override string Type()
    {
        return ObjectType.COMPILED_FUNCTION_OBJ;
    }

    public override string Inspect()
    {
        return $"CompiledFunction[{this}]";
    }
}