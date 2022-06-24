namespace Monkey.Core;

public class Token
{
    public string Type { get; init; }
    public string Literal { get; init; }
}

public static class TokenType
{
    public static readonly string ILLEGAL = "ILLEGAL";
    public static readonly string EOF = "EOF";
    
    // Identifiers + literals
    public static readonly string IDENT = "IDENT";
    public static readonly string INT = "INT";
    
    
    // Operators
    public static readonly string ASSIGN = "=";
    public static readonly string PLUS = "+";
    
    // Delimiters
    public static readonly string COMMA = ",";
    public static readonly string SEMICOLON = ";";
    
    public static readonly string LPAREN = "(";
    public static readonly string RPAREN = ")";
    public static readonly string LBRACE = "{";
    public static readonly string RBRACE = "}";
    
    // Keywords
    public static readonly string FUNCTION = "FUNCTION";
    public static readonly string LET = "LET";
}