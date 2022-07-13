using Monkey.Core.AST;
using NUnit.Framework.Internal;

namespace Monkey.Test;

public class AstTest
{
    private struct TestModifyCase
    {
        public Node Input { get; set; }
        public Node Expected { get; set; }
    }
    [Test]
    public void TestModify()
    {
        var one = () => { return new IntegerLiteral {Token = new Token { Literal = "1", Type = TokenType.INT}, Value = 1}; };
        var two = () => { return new IntegerLiteral {Token = new Token { Literal = "2", Type = TokenType.INT}, Value = 2}; };

        var turnOneIntoTwo = (Node node) =>
        {
            if (node is not IntegerLiteral integer)
            {
                return node;
            }

            if (integer.Value != 1)
            {
                return integer;
            }

            integer.Value = 2;
            integer.Token.Literal = "2";
            return integer;
        };

        var tests = new TestModifyCase[]
        {
            new()
            {
                Input = one(),
                Expected = two()
            },
            new()
            {
                Input = new Program {Statements = new List<Statement> {new ExpressionStatement {Expression = one()}}},
                Expected = new Program {Statements = new List<Statement> {new ExpressionStatement {Expression = two()}}}
            },
            new()
            {
                Input = new InfixExpression { Left = one(), Operator = "+", Right = two()},
                Expected = new InfixExpression { Left = two(), Operator = "+", Right = two()}
            },
            new()
            {
                Input = new InfixExpression { Left = two(), Operator = "+", Right = one()},
                Expected = new InfixExpression { Left = two(), Operator = "+", Right = two()}
            },
            new()
            {
                Input = new PrefixExpression {Operator = "-", Right = one()},
                Expected = new PrefixExpression {Operator = "-", Right = two()}
            },
            new()
            {
                Input = new IndexExpression {Left = one(), Index = one()},
                Expected = new IndexExpression {Left = two(), Index = two()}
            },
            new()
            {
                Input = new IfExpression { Condition = one(), Consequence = new BlockStatement { Statements = new List<Statement>
                {
                    new ExpressionStatement {Expression = one()}
                }}, Alternative = new BlockStatement { Statements = new List<Statement> { new ExpressionStatement{Expression = one()}}}},
                Expected = new IfExpression { Condition = two(), Consequence = new BlockStatement { Statements = new List<Statement>
                {
                    new ExpressionStatement {Expression = two()}
                }}, Alternative = new BlockStatement { Statements = new List<Statement> { new ExpressionStatement{Expression = two()}}}},
            },
            new()
            {
                Input = new ReturnStatement (new Token()) {ReturnValue = one()},
                Expected = new ReturnStatement(new Token()) {ReturnValue = two()}
            },
            new()
            {
                Input = new LetStatement(new Token()) {Value = one()},
                Expected = new LetStatement(new Token()) {Value = two()}
            },
            new()
            {
                Input = new FunctionLiteral
                {
                    Parameters = new List<Identifier> { },
                    Body = new BlockStatement
                    {
                        Statements = new List<Statement>
                        {
                            new ExpressionStatement {Expression = one()}
                        }
                    }
                },
                Expected = new FunctionLiteral
                {
                    Parameters = new List<Identifier>(),
                    Body = new BlockStatement
                    {
                        Statements = new List<Statement>
                        {
                            new ExpressionStatement {Expression = two()}
                        }
                    }
                }
            },
            new()
            {
                Input = new ArrayLiteral {Elements = new List<Expression> {one(), two()}},
                Expected = new ArrayLiteral {Elements = new List<Expression> {two(), two()}}
            }
        };
        var i = 0;

        foreach (var test in tests)
        {
            Console.WriteLine(i++);
            var modified = Modifier.Modify(test.Input, turnOneIntoTwo);

            var equal = DeepEqual(modified, test.Expected);
            if (!equal)
            {
                Assert.Fail($"not equal. Got '{modified}', want '{test.Expected}'");
            }
        }

        var hashLiteral = new HashLiteral
        {
            Pairs = new Dictionary<Expression, Expression>
            {
                {one(), one()},
                {one(), one()}
            }
        };

        Modifier.Modify(hashLiteral, turnOneIntoTwo);

        foreach (var (k, v) in hashLiteral.Pairs)
        {
            var key = k as IntegerLiteral;
            if (key.Value != 2)
            {
                Assert.Fail($"value is not 2, got {key.Value}");
            }

            var val = v as IntegerLiteral;
            if (val.Value != 2)
            {
                Assert.Fail($"value is not 2, got {val.Value}");
            }
        }
    }

    private bool DeepEqual(Node modified, Node testExpected)
    {
        return modified.String().Equals(testExpected.String());
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