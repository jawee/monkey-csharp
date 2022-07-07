using System.Text;

namespace Monkey.Core.AST;

public class InfixExpression : Expression
{
    public Token Token { get; set; }
    public Expression Left { get; set; }
    public string Operator { get; set; }
    public Expression Right { get; set; }
    
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

        builder.Append('(');
        builder.Append(Left.String());
        builder.Append(' ');
        builder.Append(Operator);
        builder.Append(' ');
        builder.Append(Right.String());
        builder.Append(')');

        return builder.ToString();
    }
}