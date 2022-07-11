using Monkey.Core.AST;

namespace Monkey.Test;

public class AstTest
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void TestString()
    {
        var statements = new List<Statement>();
        var token = new Token
        {
            Type = TokenType.LET,
            Literal = "let"
        };
        var name = new Identifier(new Token {Type = TokenType.IDENT, Literal = "myVar"}, "myVar");
        var value = new Identifier(new Token {Type = TokenType.IDENT, Literal = "anotherVar"}, "anotherVar");

        var letStatement = new LetStatement(token)
        {
            Name = name,
            Value = value
        };
        
        statements.Add(letStatement);
        var program = new Program();
        program.Statements = statements;

        if (!program.String().Equals("let myVar = anotherVar;"))
        {
            Assert.Fail($"program.String() wrong. got '{program.String()}'");
        }
    }
}