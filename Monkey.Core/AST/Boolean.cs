namespace Monkey.Core.AST;

public class Boolean : Expression
{
    public Token Token { get; set; }
    public bool Value { get; set; }
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
        return Token.Literal;
    }
}