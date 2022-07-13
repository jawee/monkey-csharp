using System.Text;
using Monkey.Core.AST;

namespace Monkey.Core.Object;

public class Macro : Object
{
    public List<Identifier> Parameters { get; set; }
    public BlockStatement Body { get; set; }
    public Environment Env { get; set; }
    public override string Type()
    {
        return ObjectType.MACRO_OBJ;
    }

    public override string Inspect()
    {
        var builder = new StringBuilder();

        var par = new List<string>();
        foreach (var p in Parameters)
        {
            par.Add(p.String());
        }

        builder.Append("macro");
        builder.Append('(');
        builder.Append(string.Join(", ", par));
        builder.Append(") {\n");
        builder.Append(Body.String());
        builder.Append("\n}");
        
        return builder.ToString();
    }
}