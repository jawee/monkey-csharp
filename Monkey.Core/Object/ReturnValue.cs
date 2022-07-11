namespace Monkey.Core.Object;

public class ReturnValue : Object
{
    public Object Value { get; set; }
    public override string Type()
    {
        return ObjectType.RETURN_VALUE_OBJ;
    }

    public override string Inspect()
    {
        return Value.Inspect();
    }
}