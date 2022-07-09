using System.Text;

namespace Monkey.Core.AST;

public class FunctionLiteral : Expression
{
    public Token Token { get; set; }
    public List<Identifier> Parameters { get; set; }
    public BlockStatement Body { get; set; }
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
        builder.Append(TokenLiteral());
        builder.Append('(');

        var parameters = new List<string>();
        foreach (var parameter in Parameters)
        {
            parameters.Add(parameter.String());
        }

        builder.Append(string.Join(", ", parameters));
        builder.Append(')');
        builder.Append(Body.String());
        return builder.ToString();
    }
}