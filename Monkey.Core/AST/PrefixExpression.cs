using System.Text;

namespace Monkey.Core.AST;

public class PrefixExpression : Expression
{
    public Node Node { get; set; }
    public Token Token { get; set; }
    public string Operator { get; set; }
    public Expression Right { get; set; }
    
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
        builder.Append('(');
        builder.Append(Operator);
        builder.Append(Right.String());
        builder.Append(')');

        return builder.ToString();
    }
}