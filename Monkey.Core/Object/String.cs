using System.Security.Cryptography;
using System.Text;

namespace Monkey.Core.Object;

public class String : Object, Hashable
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
    
    public HashKey HashKey()
    {
        return new HashKey {Type = Type(), Value = Value.GetHashCode()};
    }
}