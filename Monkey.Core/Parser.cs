using System.Globalization;
using Monkey.Core.AST;

namespace Monkey.Core;

enum Precedence
{
    _ = 0,
    LOWEST = 1,
    EQUALS = 2,
    LESSGREATER = 3,
    SUM = 4,
    PRODUCT = 5,
    PREFIX = 6,
    CALL = 7
}

public class Parser
{
    private Lexer _lexer;
    private List<string> _errors;

    private Token _curToken;
    private Token _peekToken;

    private Dictionary<string, Func<Expression>> _prefixParseFns;
    private Dictionary<string, Func<Expression, Expression>> _infixParseFns;

    private Dictionary<string, Precedence> _precedences = new Dictionary<string, Precedence>
    {
        {TokenType.EQ, Precedence.EQUALS},
        {TokenType.NOT_EQ, Precedence.EQUALS},
        {TokenType.LT, Precedence.LESSGREATER},
        {TokenType.GT, Precedence.LESSGREATER},
        {TokenType.PLUS, Precedence.SUM},
        {TokenType.MINUS, Precedence.SUM},
        {TokenType.SLASH, Precedence.PRODUCT},
        {TokenType.ASTERISK, Precedence.PRODUCT}
    };

    public Parser(Lexer lexer)
    {
        _lexer = lexer;
        _errors = new List<string>();
        _prefixParseFns = new Dictionary<string, Func<Expression>>();
        _infixParseFns = new Dictionary<string, Func<Expression, Expression>>();
        
        RegisterPrefix(TokenType.IDENT, ParseIdentifier);
        RegisterPrefix(TokenType.INT, ParseIntegerLiteral);
        RegisterPrefix(TokenType.BANG, ParsePrefixExpression);
        RegisterPrefix(TokenType.MINUS, ParsePrefixExpression);
        
        RegisterInfix(TokenType.PLUS, ParseInfixExpression);
        RegisterInfix(TokenType.MINUS, ParseInfixExpression);
        RegisterInfix(TokenType.SLASH, ParseInfixExpression);
        RegisterInfix(TokenType.ASTERISK, ParseInfixExpression);
        RegisterInfix(TokenType.EQ, ParseInfixExpression);
        RegisterInfix(TokenType.NOT_EQ, ParseInfixExpression);
        RegisterInfix(TokenType.LT, ParseInfixExpression);
        RegisterInfix(TokenType.GT, ParseInfixExpression);
        
        NextToken();
        NextToken();
    }

    private Expression ParseInfixExpression(Expression left)
    {
        var expression = new InfixExpression
        {
            Token = _curToken,
            Operator = _curToken.Literal,
            Left = left
        };

        var precedence = CurPrecedence();
        NextToken();
        expression.Right = ParseExpression(precedence);

        return expression;
    }

    private Precedence PeekPrecedence()
    {
        if (_precedences.ContainsKey(_peekToken.Type))
        {
            return _precedences[_peekToken.Type];
        }

        return Precedence.LOWEST;
    }

    private Precedence CurPrecedence()
    {
        if (_precedences.ContainsKey(_curToken.Type))
        {
            return _precedences[_curToken.Type];
        }

        return Precedence.LOWEST;
    }
    
    private Expression ParsePrefixExpression()
    {
        var expression = new PrefixExpression
        {
            Token = _curToken,
            Operator = _curToken.Literal
        };
        
        NextToken();

        expression.Right = ParseExpression(Precedence.PREFIX);

        return expression;
    }


    public List<string> Errors()
    {
        return _errors;
    }

    private void NoPrefixParseFnError(string tokenType)
    {
        var msg = $"no prefix parse function for '{tokenType}' found";
        _errors.Add(msg);
    }


    private Expression ParseIdentifier()
    {
        return new Identifier(_curToken, _curToken.Literal);
    }

    private Expression ParseIntegerLiteral()
    {
        var literal = new IntegerLiteral {Token = _curToken};
        var ok = int.TryParse(_curToken.Literal, out var value);
        if (!ok)
        {
            var msg = $"could not parse '{_curToken.Literal}' as integer";
            _errors.Add(msg);
            return null;
        }

        literal.Value = value;

        return literal;
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
                return ParseExpressionStatement();
        }
    }

    private ExpressionStatement ParseExpressionStatement()
    {
        var statement = new ExpressionStatement
        {
            Token = _curToken,
            Expression = ParseExpression(Precedence.LOWEST)
        };

        if (PeekTokenIs(TokenType.SEMICOLON))
        {
            NextToken();
        }

        return statement;
    }

    private Expression ParseExpression(Precedence precedence)
    {
        if (!_prefixParseFns.ContainsKey(_curToken.Type))
        {
            NoPrefixParseFnError(_curToken.Type);
        }
        var prefix = _prefixParseFns[_curToken.Type];

        if (prefix is null)
        {
            NoPrefixParseFnError(_curToken.Type);
            return null;
        }

        var leftExp = prefix();

        while (!PeekTokenIs(TokenType.SEMICOLON) && precedence < PeekPrecedence())
        {
            if (!_infixParseFns.ContainsKey(_peekToken.Type))
            {
                return leftExp;
            }
            var infix = _infixParseFns[_peekToken.Type];

            if (infix is null)
            {
                return leftExp;
            }
            
            NextToken();

            leftExp = infix(leftExp);
        }
        
        return leftExp;
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

    private void RegisterPrefix(string tokenType, Func<Expression> function)
    {
        _prefixParseFns[tokenType] = function;
    }

    private void RegisterInfix(string tokenType, Func<Expression, Expression> function)
    {
        _infixParseFns[tokenType] = function;
    }
}