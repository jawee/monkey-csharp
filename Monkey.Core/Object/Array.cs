using System.Text;

namespace Monkey.Core.Object;

public class Array : Object
{
    public List<Object> Elements { get; set; }
    public override string Type()
    {
        return ObjectType.ARRAY_OBJ;
    }

    public override string Inspect()
    {
        var builder = new StringBuilder();
        var elements = new List<string>();
        foreach (var el in Elements)
        {
            elements.Add(el.Inspect());
        }

        builder.Append('[');
        builder.Append(string.Join(", ", elements));
        builder.Append(']');

        return builder.ToString();
    }
}