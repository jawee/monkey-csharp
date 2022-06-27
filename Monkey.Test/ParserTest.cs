using Monkey.Core.AST;

namespace Monkey.Test;

public class ParserTest
{
    [Test]
    public void TestLetStatements()
    {
        var input = @"
        let x = 5;
        let y = 10;
        let foobar = 838383;
        ";

        var lexer = new Lexer(input);
        var parser = new Parser(lexer);

        var program = parser.ParseProgram();
        CheckParserErrors(parser);
        if (program == null)
        {
            Assert.Fail("ParseProgram() returned null");
        }

        if (program.Statements.Count != 3)
        {
           Assert.Fail($"program.Statements does not contain 3 statements. Got '{program.Statements.Count}"); 
        }

        var tests = new[]
        {
            new {ExpectedIdentifier = "x"},
            new {ExpectedIdentifier = "y"},
            new {ExpectedIdentifier = "foobar"}
        };

        for (var i = 0; i < tests.Length; i++)
        {
            var test = tests[i];
            var stmt = program.Statements[i];

            if (!TestLetStatements(stmt, test.ExpectedIdentifier))
            {
                return;
            }
        }
    }

    private void CheckParserErrors(Parser parser)
    {
        var errors = parser.Errors();

        if (errors.Count == 0)
        {
            return;
        }
        
        Console.WriteLine($"Parser has {errors.Count} errors");
        foreach (var error in errors)
        {
           Console.WriteLine($"Parser error: '{error}'"); 
        }
        Assert.Fail();
    }

    private bool TestLetStatements(Statement s, string name)
    {
        if (s.TokenLiteral() != "let")
        {
            Assert.Fail($"s.TokenLiteral not 'let'. Got '{s.TokenLiteral()}'");
            return false;
        }

        if (s is not LetStatement letStmt)
        {
            Assert.Fail($"s is not a LetStatement.");
            return false;
        }
        
        if (letStmt.Name.Value != name)
        {
           Assert.Fail($"letStmt.Name.Value is not '{name}', got '{letStmt.Name.Value}'");
           return false;
        }

        if (letStmt.Name.TokenLiteral() != name)
        {
            Assert.Fail($"letStmt.Name.TokenLiteral() not '{name}', got '{letStmt.Name.TokenLiteral()}'");
            return false;
        }

        return true;
    }
}