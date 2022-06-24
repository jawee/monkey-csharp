using System.Security.Principal;
using Monkey.Core;

namespace Monkey.Test;

public class LexerTest
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
    }

    [Test]
    public void TestNextTokenCharacters()
    {
        var input = "=+(){},;";

        var tests = new[]
        {
            new {ExpectedType = TokenType.ASSIGN, ExpectedLiteral = "="},
            new {ExpectedType = TokenType.PLUS, ExpectedLiteral = "+"},
            new {ExpectedType = TokenType.LPAREN, ExpectedLiteral = "("},
            new {ExpectedType = TokenType.RPAREN, ExpectedLiteral = ")"},
            new {ExpectedType = TokenType.LBRACE, ExpectedLiteral = "{"},
            new {ExpectedType = TokenType.RBRACE, ExpectedLiteral = "}"},
            new {ExpectedType = TokenType.COMMA, ExpectedLiteral = ","},
            new {ExpectedType = TokenType.SEMICOLON, ExpectedLiteral = ";"},
            new {ExpectedType = TokenType.EOF, ExpectedLiteral = ""}
        }.ToList();

        var l = new Lexer(input);
        
        for (var i = 0; i < tests.Count; i++)
        {
            var test = tests[i];
            var tok = l.NextToken();

            if (tok.Type != test.ExpectedType)
            {
                Assert.Fail($"tests[{i}] - tokentype wrong. expected={test.ExpectedType}, got={tok.Type}");
            }

            if (tok.Literal != test.ExpectedLiteral)
            {
                Assert.Fail($"tests[{i}] - literal wrong. expected={test.ExpectedLiteral}, got={tok.Literal}");
            }
        }
    }
    [Test]
    public void TestNextToken()
    {
        var input = @"let five = 5;
        let ten = 10;

        let add = fn(x, y) {
            x + y;
        };

        let result = add(five, ten);";

        var tests = new[]
        {
            new {ExpectedType = TokenType.LET, ExpectedLiteral = "let"},
            new {ExpectedType = TokenType.IDENT, ExpectedLiteral = "five"},
            new {ExpectedType = TokenType.ASSIGN, ExpectedLiteral = "="},
            new {ExpectedType = TokenType.INT, ExpectedLiteral = "5"},
            new {ExpectedType = TokenType.SEMICOLON, ExpectedLiteral = ";"},
            new {ExpectedType = TokenType.LET, ExpectedLiteral = "let"},
            new {ExpectedType = TokenType.IDENT, ExpectedLiteral = "ten"},
            new {ExpectedType = TokenType.ASSIGN, ExpectedLiteral = "="},
            new {ExpectedType = TokenType.INT, ExpectedLiteral = "10"},
            new {ExpectedType = TokenType.SEMICOLON, ExpectedLiteral = ";"},
            new {ExpectedType = TokenType.LET, ExpectedLiteral = "let"},
            new {ExpectedType = TokenType.IDENT, ExpectedLiteral =  "add"},
            new {ExpectedType = TokenType.ASSIGN, ExpectedLiteral = "="},
            new {ExpectedType = TokenType.FUNCTION, ExpectedLiteral = "fn"},
            new {ExpectedType = TokenType.LPAREN, ExpectedLiteral = "("},
            new {ExpectedType = TokenType.IDENT, ExpectedLiteral = "x"},
            new {ExpectedType = TokenType.COMMA, ExpectedLiteral = ","},
            new {ExpectedType = TokenType.IDENT, ExpectedLiteral = "y"},
            new {ExpectedType = TokenType.RPAREN, ExpectedLiteral = ")"},
            new {ExpectedType = TokenType.LBRACE, ExpectedLiteral = "{"},
            new {ExpectedType = TokenType.IDENT, ExpectedLiteral = "x"},
            new {ExpectedType = TokenType.PLUS, ExpectedLiteral = "+"},
            new {ExpectedType = TokenType.IDENT, ExpectedLiteral = "y"},
            new {ExpectedType = TokenType.SEMICOLON, ExpectedLiteral = ";"},
            new {ExpectedType = TokenType.RBRACE, ExpectedLiteral = "}"},
            new {ExpectedType = TokenType.SEMICOLON, ExpectedLiteral = ";"},
            new {ExpectedType = TokenType.LET, ExpectedLiteral = "let"},
            new {ExpectedType = TokenType.IDENT, ExpectedLiteral = "result"},
            new {ExpectedType = TokenType.ASSIGN, ExpectedLiteral = "="},
            new {ExpectedType = TokenType.IDENT, ExpectedLiteral = "add"},
            new {ExpectedType = TokenType.LPAREN, ExpectedLiteral = "("},
            new {ExpectedType = TokenType.IDENT, ExpectedLiteral = "five"},
            new {ExpectedType = TokenType.COMMA, ExpectedLiteral = ","},
            new {ExpectedType = TokenType.IDENT, ExpectedLiteral = "ten"},
            new {ExpectedType = TokenType.RPAREN, ExpectedLiteral = ")"},
            new {ExpectedType = TokenType.SEMICOLON, ExpectedLiteral = ";"},
            new {ExpectedType = TokenType.EOF, ExpectedLiteral = ""},
        }.ToList();

        var l = new Lexer(input);
        
        for (var i = 0; i < tests.Count; i++)
        {
            var test = tests[i];
            var tok = l.NextToken();

            if (tok.Type != test.ExpectedType)
            {
                Assert.Fail($"tests[{i}] - TokenType wrong. expected={test.ExpectedType}, got={tok.Type}");
            }

            if (tok.Literal != test.ExpectedLiteral)
            {
                Assert.Fail($"tests[{i}] - Literal wrong. expected={test.ExpectedLiteral}, got={tok.Literal}");
            }
        }
    }
}