namespace Monkey.Core.Object;

public class HashKey
{
    protected bool Equals(HashKey other)
    {
        return Type == other.Type && Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((HashKey) obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Type, Value);
    }

    public string Type { get; set; }
    public int Value { get; set; }
    
    public static bool operator ==(HashKey a, HashKey b)
    {
        return a.Type.Equals(b.Type) && a.Value == b.Value;
    }

    public static bool operator !=(HashKey a, HashKey b)
    {
        return !a.Type.Equals(b.Type) || a.Value != b.Value;
    }

    public override string ToString()
    {
        return $@"{{ Type: {Type}, Value: {Value} }}";
    }
    
}