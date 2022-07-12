using System.Text;

namespace Monkey.Core.AST;

public class HashLiteral : Expression
{
    public Token Token { get; set; }
    public Dictionary<Expression, Expression> Pairs { get; set; }
    public Node Node { get; set; }

    public void ExpressionNode()
    {
    }

    public string TokenLiteral()
    {
        return Token.Literal;
    }

    public string String()
    {
        var builder = new StringBuilder();

        var pairs = new List<string>();
        foreach (var pair in Pairs)
        {
            pairs.Add($"{pair.Key.String()}:{pair.Value.String()}");
        }

        builder.Append('{');
        builder.Append(string.Join(", ", pairs));
        builder.Append('}');

        return builder.ToString();
    }
    
}