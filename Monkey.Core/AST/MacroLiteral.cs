using System.Text;

namespace Monkey.Core.AST;

public class MacroLiteral : Expression
{
    public Token Token { get; set; }
    public List<Identifier> Parameters { get; set; }
    public BlockStatement Body { get; set; }
    public string TokenLiteral()
    {
        return Token.Literal;
    }

    public Node Node { get; set; }
    
    public void ExpressionNode()
    {
    }

    public string String()
    {
        var builder = new StringBuilder();
        
        var par = new List<string>();
        foreach (var parameter in Parameters)
        {
            par.Add(parameter.String());
        }

        builder.Append(TokenLiteral());
        builder.Append('(');
        builder.Append(string.Join(", ", par));
        builder.Append(')');
        builder.Append(Body.String());

        return builder.ToString();
    }

}