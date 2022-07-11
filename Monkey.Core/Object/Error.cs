using Monkey.Core.AST;

namespace Monkey.Core.Object;

public class Error : Object
{
    public string Message { get; set; }
    public override string Type()
    {
        return ObjectType.ERROR_OBJ;
    }

    public override string Inspect()
    {
        return $"ERROR: {Message}";
    }
}