namespace Monkey.Core.AST;

public class StringLiteral : Expression
{
    public Node Node { get; set; }
    public Token Token { get; set; }
    public string Value { get; set; }
    public string TokenLiteral()
    {
        return Token.Literal;
    }

    public string String()
    {
        return Token.Literal;
    }

    public void ExpressionNode()
    {
    }
}