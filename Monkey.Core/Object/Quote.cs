using Monkey.Core.AST;

namespace Monkey.Core.Object;

public class Quote : Object
{
    public Node Node { get; set; }
    public override string Type()
    {
        return ObjectType.QUOTE_OBJ;
    }

    public override string Inspect()
    {
        return $"QUOTE({Node.String()})";
    }
}