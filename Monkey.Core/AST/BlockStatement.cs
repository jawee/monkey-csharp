using System.Text;

namespace Monkey.Core.AST;

public class BlockStatement : Statement
{
    public Token Token { get; set; }
    public List<Statement> Statements { get; set; }
    public Node Node { get; set; }

    public BlockStatement()
    {
        Statements = new List<Statement>();
    }
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

        foreach (var statement in Statements)
        {
            builder.Append(statement.String());
        }

        return builder.ToString();
    }
}