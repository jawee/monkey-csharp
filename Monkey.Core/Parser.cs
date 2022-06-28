using Monkey.Core.AST;

namespace Monkey.Core;

public class Parser
{
    private Lexer _lexer;
    private List<string> _errors;

    private Token _curToken;
    private Token _peekToken;

    public Parser(Lexer lexer)
    {
        _lexer = lexer;
        _errors = new List<string>();
        
        NextToken();
        NextToken();
    }

    public List<string> Errors()
    {
        return _errors;
    }

    private void PeekError(string type)
    {
        var msg = $"Expected next token to be '{type}', got '{_peekToken.Type}' instead";
        _errors.Add(msg);
    }

    private void NextToken()
    {
        _curToken = _peekToken;
        _peekToken = _lexer.NextToken();
    }

    public Program ParseProgram()
    {
        var program = new Program();
        program.Statements = new List<Statement>();

        while (_curToken.Type != TokenType.EOF)
        {
            var stmt = ParseStatement();
            if (stmt is not null)
            {
                program.Statements.Add(stmt);
            }
            NextToken();
        }
        return program;
    }

    private Statement? ParseStatement()
    {
        switch (_curToken.Type)
        {
            case "LET":
                return ParseLetStatement();
            case "RETURN":
                return ParseReturnStatement();
            default:
                return null;
        }
    }

    private ReturnStatement ParseReturnStatement()
    {
        var statement = new ReturnStatement(_curToken);

        NextToken();

        while (!CurTokenIs(TokenType.SEMICOLON))
        {
            NextToken();
        }

        return statement;
    }

    private LetStatement ParseLetStatement()
    {
        var stmt = new LetStatement(_curToken);

        if (!ExpectPeek(TokenType.IDENT))
        {
            return null;
        }

        stmt.Name = new Identifier(_curToken, _curToken.Literal);

        if (!ExpectPeek(TokenType.ASSIGN))
        {
            return null;
        }
        
        //TODO: We're skipping the expressions until we encounter a semicolon
        while (!CurTokenIs(TokenType.SEMICOLON))
        {
            NextToken();
        }

        return stmt;
    }

    private bool ExpectPeek(string type)
    {
        if (PeekTokenIs(type))
        {
            NextToken();
            return true;
        }

        PeekError(type);
        return false;
    }

    private bool PeekTokenIs(string type)
    {
        return _peekToken.Type == type;
    }

    private bool CurTokenIs(string type)
    {
        return _curToken.Type == type;
    }
}