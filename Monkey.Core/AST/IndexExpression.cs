using System.Text;

namespace Monkey.Core.AST;

public class IndexExpression : Expression
{
    public Token Token { get; set; }
    public Expression Left { get; set; }
    public Expression Index { get; set; }

    public string TokenLiteral()
    {
        return Token.Literal;
    }

    public string String()
    {
        var builder = new StringBuilder();

        builder.Append('(');
        builder.Append(Left.String());
        builder.Append('[');
        builder.Append(Index.String());
        builder.Append("])");

        return builder.ToString();
    }

    public Node Node { get; set; }
    public void ExpressionNode()
    {
    }
}