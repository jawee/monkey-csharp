using System.Text;

namespace Monkey.Core.AST;

public interface Statement : Node
{
    public Node Node { get; set; }
    public void StatementNode();
    public string TokenLiteral();
    public string String();
}

public class ReturnStatement : Statement
{
    public ReturnStatement(Token token)
    {
        Token = token;
    }

    public Node Node { get; set; }
    public Token Token { get; set; }

    public Expression ReturnValue { get; set; }

    public void StatementNode()
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

        if (ReturnValue != null)
        {
            builder.Append(ReturnValue.String());
        }

        builder.Append(";");

        return builder.ToString();
    }
}

public class ExpressionStatement : Statement
{
    public Token Token { get; set; }
    public Expression Expression { get; set; }


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
        if (Expression != null)
        {
            return Expression.String();
        }

        return "";
    }
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

    public string String()
    {
        var builder = new StringBuilder();
        builder.Append(TokenLiteral());
        builder.Append(" ");
        builder.Append(Name?.String());
        builder.Append(" = ");

        if (Value != null)
        {
            builder.Append(Value.String());
        }

        builder.Append(";");

        return builder.ToString();
    }
}

public class Identifier : Expression
{
    public Identifier(Token token, string value)
    {
        Token = token;
        Value = value;
    }

    public Token Token { get; set; }
    public string Value { get; set; }

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
        return Value;
    }
}