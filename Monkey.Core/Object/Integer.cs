namespace Monkey.Core.Object;

public class Integer : Object, Hashable
{
    public int Value { get; set; }
    public override string Type()
    {
        return ObjectType.INTEGER_OBJ;
    }

    public override string Inspect()
    {
        return Value.ToString();
    }
    
    public override bool Equals(object? obj)
    {
        if (obj == null)
        {
            return false;
        }
        
        if (this.GetType() != obj.GetType())
        {
            return false;
        }
        return Value == ((obj as Integer)!).Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
    
    public static bool operator ==(Integer a, Integer b)
    {
        if (ReferenceEquals(a, b))
        {
            return true;
        }

        if (((object)a == null) || ((object)b == null))
        {
            return false;
        }

        return a.Value == b.Value;
    }

    public static bool operator !=(Integer a, Integer b)
    {
        if (ReferenceEquals(a, b))
        {
            return false;
        }
        
        if(((object)a == null) || ((object)b == null))
        {
            return true;
        }

        return a.Value != b.Value;
    }

    public HashKey HashKey()
    {
        return new HashKey {Type = Type(), Value = Value};
    }
}