using System.Text;
using Microsoft.VisualBasic;
using Monkey.Core.AST;

namespace Monkey.Core.Object;

public class Function : Object
{
    public List<Identifier> Parameters { get; set; }
    public BlockStatement Body { get; set; }
    public Environment Env { get; set; }
    public override string Type()
    {
        return ObjectType.FUNCTION_OBJ;
    }

    public override string Inspect()
    {
        var builder = new StringBuilder();

        var list = new List<String>();
        foreach (var param in Parameters)
        {
            list.Add(param.String());
        }

        builder.Append("fn");
        builder.Append('(');
        builder.Append(string.Join(", ", list));
        builder.Append(") {\n");
        builder.Append(Body.String());
        builder.Append("\n}");

        return builder.ToString();
    }
}