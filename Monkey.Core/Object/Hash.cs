using System.Text;

namespace Monkey.Core.Object;

public class Hash : Object
{
    public Dictionary<HashKey, HashPair> Pairs { get; set; }
    public override string Type()
    {
        return ObjectType.HASH_OBJ;
    }

    public override string Inspect()
    {
        var builder = new StringBuilder();

        var pairs = new List<string>();
        foreach (var pair in Pairs)
        {
           pairs.Add($"{pair.Value.Key.Inspect()}: {pair.Value.Value.Inspect()}"); 
        }

        builder.Append('{');
        builder.Append(string.Join(", ", pairs));
        builder.Append('}');

        return builder.ToString();
    }
}