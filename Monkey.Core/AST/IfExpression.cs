using System.Text;

namespace Monkey.Core.AST;

public class IfExpression : Expression
{
    public Token Token { get; set; }
    public Expression Condition { get; set; }
    public BlockStatement Consequence { get; set; }
    public BlockStatement Alternative { get; set; }
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

        builder.Append("if");
        builder.Append(Condition.String());
        builder.Append(" ");
        builder.Append(Consequence.String());

        if (Alternative != null)
        {
            builder.Append("else ");
            builder.Append(Alternative.String());
        }

        return builder.ToString();
    }
}