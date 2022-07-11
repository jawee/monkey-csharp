namespace Monkey.Core.Object;

public class Boolean : Object
{
    public bool Value { get; set; }


    public override string Type()
    {
       return ObjectType.BOOLEAN_OBJ; 
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
        return Value == (obj as Boolean).Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
    
    public static bool operator ==(Boolean a, Boolean b)
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

    public static bool operator !=(Boolean a, Boolean b)
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

}