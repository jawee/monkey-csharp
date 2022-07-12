namespace Monkey.Core.Object;

public class String : Object
{
    public string Value { get; set; }
    public override string Type()
    {
        return ObjectType.STRING_OBJ;
    }

    public override string Inspect()
    {
        return Value;
    }
}