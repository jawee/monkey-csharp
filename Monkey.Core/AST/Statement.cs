namespace Monkey.Core.AST;

public interface Statement
{
    public Node Node { get; set; }
    public void StatementNode();
    public string TokenLiteral();
}

public class LetStatement : Statement
{
    public LetStatement(Token token)
    {
        Token = token;
    }

    public Node Node { get; set; }
    
    public Token Token { get; set; }
    public Identifier Name { get; set; }
    public Expression Value { get; set; }

    public void StatementNode()
    {
    }

    public string TokenLiteral()
    {
        return Token.Literal;
    }
}

public class Identifier 
{
    public Identifier(Token token, string value)
    {
        Token = token;
        Value = value;
    }

    public Token Token { get; set; }
    public string Value { get; set; }

    public void ExpressionNode()
    {
    }

    public string TokenLiteral()
    {
        return Token.Literal;
    }
}