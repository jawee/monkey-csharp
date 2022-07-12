using System.Text;

namespace Monkey.Core.AST;

public class ArrayLiteral : Expression
{
    public Token Token { get; set; }
    public List<Expression> Elements { get; set; }
    
    public string TokenLiteral()
    {
        return Token.Literal;
    }

    public string String()
    {
        var builder = new StringBuilder();

        var elements = new List<string>();
        foreach (var expression in Elements)
        {
            elements.Add(expression.String());
        }

        builder.Append('[');
        builder.Append(string.Join(", ", elements));
        builder.Append(']');

        return builder.ToString();
    }
    
    public Node Node { get; set; }
    public void ExpressionNode()
    {
    }
}