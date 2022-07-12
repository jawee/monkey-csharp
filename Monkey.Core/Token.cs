namespace Monkey.Core;

public class Token
{
    public string Type { get; init; }
    public string Literal { get; init; }

    public override string ToString()
    {
        return $"{{Type: '{Type}', Literal: '{Literal}'}}";
    }
}

public static class TokenType
{
    public static readonly string ILLEGAL = "ILLEGAL";
    public static readonly string EOF = "EOF";
    
    // Identifiers + literals
    public static readonly string IDENT = "IDENT";
    public static readonly string INT = "INT";
    public static readonly string STRING = "STRING";
    
    // Operators
    public static readonly string ASSIGN = "=";
    public static readonly string PLUS = "+";
    public static readonly string MINUS = "-";
    public static readonly string BANG = "!";
    public static readonly string ASTERISK = "*";
    public static readonly string SLASH = "/";
    
    public static readonly string LT = "<";
    public static readonly string GT = ">";
    
    // Delimiters
    public static readonly string COMMA = ",";
    public static readonly string SEMICOLON = ";";
    public static readonly string COLON = ":";
    
    public static readonly string LPAREN = "(";
    public static readonly string RPAREN = ")";
    public static readonly string LBRACE = "{";
    public static readonly string RBRACE = "}";
    public static readonly string LBRACKET = "[";
    public static readonly string RBRACKET = "]";
    
    // Keywords
    public static readonly string FUNCTION = "FUNCTION";
    public static readonly string LET = "LET";
    public static readonly string TRUE = "TRUE";
    public static readonly string FALSE = "FALSE";
    public static readonly string IF = "IF";
    public static readonly string ELSE = "ELSE";
    public static readonly string RETURN = "RETURN";

    public static readonly string EQ = "==";
    public static readonly string NOT_EQ = "!=";
}
