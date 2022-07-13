namespace Monkey.Core.AST;

public class IntegerLiteral : Expression
{
    public Token Token { get; set; }
    public int Value { get; set; }

    public void ExpressionNode()
    {
    }

    public Node Node { get; set; }

    public void StatementNode()
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

    public override string ToString()
    {
        return $"{Token.Literal}";
    }
}