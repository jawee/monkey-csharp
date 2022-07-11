namespace Monkey.Core.Object;

public class Null : Object
{
    public override string Type()
    {
        return ObjectType.NULL_OBJ;
    }

    public override string Inspect()
    {
        return "null";
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Null)
        {
            return false;
        }

        return true;
    }
    
}