using System.Text;

namespace Monkey.Core.AST;

public class CallExpression : Expression
{
    public Token Token { get; set; }
    public Expression Function { get; set; }
    public List<Expression> Arguments { get; set; }
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

        var args = Arguments.Select(a => a.String()).ToList();

        builder.Append(Function.String());
        builder.Append('(');
        builder.Append(string.Join(", ", args));
        builder.Append(')');

        return builder.ToString();
    }
}