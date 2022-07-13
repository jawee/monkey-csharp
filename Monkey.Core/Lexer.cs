namespace Monkey.Core;

public class Lexer
{
    private string _input;
    private int _position;
    private int _readPosition;
    // private char _ch;
    private byte _ch;
    public Lexer(string input)
    {
        _input = input;
        ReadChar();
    }

    public Token NextToken()
    {
        Token tok = null;

        SkipWhitespace();
        
        switch(_ch)
        {
            case (byte) '=':
                if (PeekChar() == Convert.ToByte('='))
                {
                    var ch = _ch;
                    ReadChar();
                    var literal = Convert.ToChar(ch) + Convert.ToChar(_ch).ToString();
                    tok = new Token {Type = TokenType.EQ, Literal = literal};
                }
                else
                {
                    tok = new Token {Type = TokenType.ASSIGN, Literal = Convert.ToChar(_ch).ToString()};
                }
                break;
            case (byte) ';':
                tok = new Token {Type = TokenType.SEMICOLON, Literal = Convert.ToChar(_ch).ToString()};
                break;
            case (byte) '(':
                tok = new Token {Type = TokenType.LPAREN, Literal = Convert.ToChar(_ch).ToString()};
                break;
            case (byte) ')':
                tok = new Token {Type = TokenType.RPAREN, Literal = Convert.ToChar(_ch).ToString()};
                break;
            case (byte) ',':
                tok = new Token {Type = TokenType.COMMA, Literal = Convert.ToChar(_ch).ToString()};
                break;
            case (byte) '+':
                tok = new Token {Type = TokenType.PLUS, Literal = Convert.ToChar(_ch).ToString()};
                break;
            case (byte) '-':
                tok = new Token {Type = TokenType.MINUS, Literal = Convert.ToChar(_ch).ToString()};
                break;
            case (byte) '!':
                if (PeekChar() == Convert.ToByte('='))
                {
                    var ch = _ch;
                    ReadChar();
                    var literal = Convert.ToChar(ch) + Convert.ToChar(_ch).ToString();
                    tok = new Token {Type = TokenType.NOT_EQ, Literal = literal};
                }
                else
                {
                    tok = new Token {Type = TokenType.BANG, Literal = Convert.ToChar(_ch).ToString()};
                }
                break;
            case (byte) '/':
                tok = new Token {Type = TokenType.SLASH, Literal = Convert.ToChar(_ch).ToString()};
                break;
            case (byte) '*':
                tok = new Token {Type = TokenType.ASTERISK, Literal = Convert.ToChar(_ch).ToString()};
                break;
            case (byte) '<':
                tok = new Token {Type = TokenType.LT, Literal = Convert.ToChar(_ch).ToString()};
                break;
            case (byte) '>':
                tok = new Token {Type = TokenType.GT, Literal = Convert.ToChar(_ch).ToString()};
                break;
            case (byte) '{':
                tok = new Token {Type = TokenType.LBRACE, Literal = Convert.ToChar(_ch).ToString()};
                break;
            case (byte) '}':
                tok = new Token {Type = TokenType.RBRACE, Literal = Convert.ToChar(_ch).ToString()};
                break;
            case 0:
                tok = new Token {Type = TokenType.EOF, Literal = ""};
                break;
            case (byte) '"':
                tok = new Token {Type = TokenType.STRING, Literal = ReadString()};
                break;
            case (byte) '[':
                tok = new Token {Type = TokenType.LBRACKET, Literal = Convert.ToChar(_ch).ToString()};
                break;
            case (byte) ']':
                tok = new Token {Type = TokenType.RBRACKET, Literal = Convert.ToChar(_ch).ToString()};
                break;
            case (byte) ':':
                tok = new Token {Type = TokenType.COLON, Literal = Convert.ToChar(_ch).ToString()};
                break;
            default:
                if (IsLetter(_ch))
                {
                    var literal = ReadIdentifier();
                    tok = new Token {Type = LookupIdent(literal), Literal = literal};
                    return tok;
                }

                if (IsDigit(_ch))
                {
                    var literal = ReadNumber();
                    tok = new Token {Type = TokenType.INT, Literal = literal};
                    return tok;
                }
                tok = new Token {Type = TokenType.ILLEGAL, Literal = Convert.ToChar(_ch).ToString()};
                break;
        }
        ReadChar();
        return tok;
    }

    private string ReadString()
    {
        var position = _position + 1;
        while (true)
        {
            ReadChar();
            if (_ch == '"' || _ch == 0)
            {
                break;
            }
        }

        return _input.Substring(position, _position - position);
    }

    private string ReadNumber()
    {
        var position = _position;
        while (IsDigit(_ch))
        {
            ReadChar();
        }
        
        return _input.Substring(position, _position-position);
    }

    private bool IsDigit(byte ch)
    {
        var c = Convert.ToChar(ch);
        return '0' <= ch && ch <= '9';
    }

    private void SkipWhitespace()
    {
        var c = Convert.ToChar(_ch);
        while (c is ' ' or '\t' or '\r' or '\n')
        {
           ReadChar();
           c = Convert.ToChar(_ch);
        }
    }

    private string ReadIdentifier()
    {
        var position = _position;

        while (IsLetter(_ch))
        {
            ReadChar();
        }

        return _input.Substring(position, _position-position);
    }

    private bool IsLetter(byte ch)
    {
        var c = Convert.ToChar(ch);
        var res = c is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or '_';
        return res;
    }

    private void ReadChar()
    {
        if (_readPosition >= _input.Length)
        {
            _ch = 0;
        }
        else
        {
            _ch = (byte) _input[_readPosition];
        }

        _position = _readPosition;
        _readPosition += 1;
    }


    private static readonly Dictionary<string, string> _keywords = new()
    {
        {"fn", TokenType.FUNCTION},
        {"let", TokenType.LET},
        {"true", TokenType.TRUE},
        {"false", TokenType.FALSE},
        {"if", TokenType.IF},
        {"else", TokenType.ELSE},
        {"return", TokenType.RETURN},
        {"macro", TokenType.MACRO}
    };

    private string LookupIdent(string ident)
    {
        if (_keywords.ContainsKey(ident))
        {
            return _keywords[ident];
        }

        return TokenType.IDENT;
    }

    private byte PeekChar()
    {
        if (_readPosition >= _input.Length)
        {
            return 0;
        }

        return (byte) _input[_readPosition];
    }
}