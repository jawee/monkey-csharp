using System.Text;

namespace Monkey.Core.AST;

public class Program
{
    public List<Statement> Statements { get; set; }

    public string TokenLiteral()
    {
        if (Statements.Count > 0)
        {
            return Statements[0].TokenLiteral();
        }

        return "";
    }

    public string String()
    {
        var builder = new StringBuilder();

        foreach (var statement in Statements)
        {
            builder.Append(statement.String());
        }

        return builder.ToString();
    }
}