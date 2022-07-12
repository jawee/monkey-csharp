namespace Monkey.Core.Object;

public class Builtin : Object
{
    public Func<List<Object>, Object>? Fn { get; set; }

    public override string Type()
    {
        return ObjectType.BUILTIN_OBJ;
    }

    public override string Inspect()
    {
        return "builtin function";
    }
}