using System.Text.Json;
using Monkey.Core.AST;
using Monkey.Core.Code;
using Monkey.Core.Compiler;
using Monkey.Core.Object;
using Object = Monkey.Core.Object.Object;
using String = Monkey.Core.Object.String;

namespace Monkey.Test;

public class CompilerTest
{
    [Test]
    public void TestDefineResolveBuiltins()
    {
        var global = new SymbolTable();
        var firstLocal = new SymbolTable(global);
        var secondLocal = new SymbolTable(firstLocal);

        var expected = new List<Symbol>
        {
            new() {Name = "a", Scope = SymbolScope.BuiltinScope, Index = 0},
            new() {Name = "c", Scope = SymbolScope.BuiltinScope, Index = 1},
            new() {Name = "e", Scope = SymbolScope.BuiltinScope, Index = 2},
            new() {Name = "f", Scope = SymbolScope.BuiltinScope, Index = 3},
        };

        for (var i = 0; i < expected.Count; i++)
        {
            var v = expected[i];
            global.DefineBuiltin(i, v.Name);
        }

        foreach (var table in new List<SymbolTable> {global, firstLocal, secondLocal})
        {
            foreach (var sym in expected)
            {
                var (result, ok) = table.Resolve(sym.Name);
                if (!ok)
                {
                    Assert.Fail($"name {sym.Name} is not resolvable");
                    continue;
                }

                if (!result.Equals(sym))
                {
                    Assert.Fail($"expected {sym.Name} to resolve to {sym}, got={result}");
                }
            }
        }
    }
    [Test]
    public void TestBuiltins()
    {
        var tests = new List<CompilerTestCase>()
        {
            new()
            {
                Input = @"len([]); push([], 1);",
                ExpectedConstants = new() { 1 },
                ExpectedInstructions = new()
                {
                    Code.Make(Opcode.OpGetBuiltin, new() {0}),
                    Code.Make(Opcode.OpArray, new() {0}),
                    Code.Make(Opcode.OpCall, new() {1}),
                    Code.Make(Opcode.OpPop),
                    Code.Make(Opcode.OpGetBuiltin, new() {5}),
                    Code.Make(Opcode.OpArray, new() {0}),
                    Code.Make(Opcode.OpConstant, new() {0}),
                    Code.Make(Opcode.OpCall, new() {2}),
                    Code.Make(Opcode.OpPop)
                }
            },
            new()
            {
                Input = @"fn() { len([]) }",
                ExpectedConstants = new()
                {
                    new List<Instructions>
                    {
                        Code.Make(Opcode.OpGetBuiltin, new() {0}),
                        Code.Make(Opcode.OpArray, new() {0}),
                        Code.Make(Opcode.OpCall, new() {1}),
                        Code.Make(Opcode.OpReturnValue)
                    }
                },
                ExpectedInstructions = new()
                {
                    Code.Make(Opcode.OpClosure, new() {0,0}),
                    Code.Make(Opcode.OpPop)
                }
            }
        };
        
        RunCompilerTests(tests);
    }
    [Test]
    public void TestResolveNestedLocal()
    {
        var global = new SymbolTable();
        global.Define("a");
        global.Define("b");

        var firstLocal = new SymbolTable(global);
        firstLocal.Define("c");
        firstLocal.Define("d");

        var secondLocal = new SymbolTable(firstLocal);
        secondLocal.Define("e");
        secondLocal.Define("f");

        var tests = new[]
        {
            new
            {
                Table = firstLocal,
                ExpectedSymbols = new List<Symbol>
                {
                    new() {Name = "a", Scope = SymbolScope.GlobalScope, Index = 0},
                    new() {Name = "b", Scope = SymbolScope.GlobalScope, Index = 1},
                    new() {Name = "c", Scope = SymbolScope.LocalScope, Index = 0},
                    new() {Name = "d", Scope = SymbolScope.LocalScope, Index = 1},
                }
            },
            new
            {
                Table = secondLocal,
                ExpectedSymbols = new List<Symbol>
                {
                    new() {Name = "a", Scope = SymbolScope.GlobalScope, Index = 0},
                    new() {Name = "b", Scope = SymbolScope.GlobalScope, Index = 1},
                    new() {Name = "e", Scope = SymbolScope.LocalScope, Index = 0},
                    new() {Name = "f", Scope = SymbolScope.LocalScope, Index = 1},
                }
            }
        };

        foreach (var test in tests)
        {
            foreach (var symbol in test.ExpectedSymbols)
            {
                var (result, ok) = test.Table.Resolve(symbol.Name);
                if (!ok)
                {
                    Assert.Fail($"name {symbol.Name} is not resolvable");
                    return;
                }

                if (!result.Equals(symbol))
                {
                    Assert.Fail($"expected {symbol.Name} to resolve to {symbol}, got {result}");
                }

            }
        }
    }
    [Test]
    public void TestResolveLocal()
    {
        var global = new SymbolTable();
        global.Define("a");
        global.Define("b");

        var local = new SymbolTable(global);
        local.Define("c");
        local.Define("d");

        var expected = new List<Symbol>
        {
            new() {Name = "a", Scope = SymbolScope.GlobalScope, Index = 0},
            new() {Name = "b", Scope = SymbolScope.GlobalScope, Index = 1},
            new() {Name = "c", Scope = SymbolScope.LocalScope, Index = 0},
            new() {Name = "d", Scope = SymbolScope.LocalScope, Index = 1}
        };

        foreach (var symbol in expected)
        {
            var (result, ok) = local.Resolve(symbol.Name);
            if (!ok)
            {
                Assert.Fail($"name {symbol.Name} is not resolvable");
                return;
            }

            if (!result.Equals(symbol))
            {
                Assert.Fail($"expected {symbol.Name} to resolve to {symbol}, got {result}");
            }
        }
    }
    [Test]
    public void TestLetStatementScopes()
    {
        var tests = new List<CompilerTestCase>
        {
            new()
            {
                Input = @"let num = 55; fn() { num }",
                ExpectedConstants = new()
                {
                    55,
                    new List<Instructions>
                    {
                        Code.Make(Opcode.OpGetGlobal, new() {0}),
                        Code.Make(Opcode.OpReturnValue)
                    }
                },
                ExpectedInstructions= new()
                {
                Code.Make(Opcode.OpConstant, new() {0}),
                Code.Make(Opcode.OpSetGlobal, new() {0}),
                Code.Make(Opcode.OpClosure, new() {1, 0}),
                Code.Make(Opcode.OpPop),
                }
            },
            new()
            {
                Input = @"fn() { let num = 55; num }",
                ExpectedConstants = new()
                {
                    55,
                    new List<Instructions> 
                    {
                        Code.Make(Opcode.OpConstant, new() {0}),
                        Code.Make(Opcode.OpSetLocal, new() {0}),
                        Code.Make(Opcode.OpGetLocal, new() {0}),
                        Code.Make(Opcode.OpReturnValue)
                    }
                },
                ExpectedInstructions = new()
                {
                    Code.Make(Opcode.OpClosure, new() {1,0}),
                    Code.Make(Opcode.OpPop)

                }
            },
            new()
            {
                Input = @"fn() { let a = 55; let b = 77; a + b }",
                ExpectedConstants = new()
                {
                    55,
                    77,
                    new List<Instructions>
                    {
                        Code.Make(Opcode.OpConstant, new() {0}),
                        Code.Make(Opcode.OpSetLocal, new() {0}),
                        Code.Make(Opcode.OpConstant, new() {1}),
                        Code.Make(Opcode.OpSetLocal, new() {1}),
                        Code.Make(Opcode.OpGetLocal, new() {0}),
                        Code.Make(Opcode.OpGetLocal, new() {1}),
                        Code.Make(Opcode.OpAdd),
                        Code.Make(Opcode.OpReturnValue)
                    }
                },
                ExpectedInstructions = new()
                {
                    Code.Make(Opcode.OpClosure, new() {2,0}),
                    Code.Make(Opcode.OpPop)
                }
            }
        };
        
        RunCompilerTests(tests);
    }
    [Test]
    public void TestFunctionCalls()
    {
        var tests = new List<CompilerTestCase>()
        {
            new()
            {
                Input = "fn() { 24 }();",
                ExpectedConstants = new()
                {
                    24,
                    new List<Instructions>
                    {
                        Code.Make(Opcode.OpConstant, new() {0}),
                        Code.Make(Opcode.OpReturnValue)
                    }
                },
                ExpectedInstructions = new()
                {
                    Code.Make(Opcode.OpClosure, new() {1, 0}),
                    Code.Make(Opcode.OpCall, new() {0}),
                    Code.Make(Opcode.OpPop)
                }
            },
            new()
            {
                Input = @"let noArg = fn() { 24 };
                            noArg();",
                ExpectedConstants = new()
                {
                    24,
                    new List<Instructions>
                    {
                        Code.Make(Opcode.OpConstant, new() {0}),
                        Code.Make(Opcode.OpReturnValue)
                    }
                },
                ExpectedInstructions = new()
                {
                    Code.Make(Opcode.OpClosure, new() {1,0}),
                    Code.Make(Opcode.OpSetGlobal, new() {0}),
                    Code.Make(Opcode.OpGetGlobal, new() {0}),
                    Code.Make(Opcode.OpCall, new() {0}),
                    Code.Make(Opcode.OpPop)
                }
            },
            new()
            {
                Input = "let oneArg = fn(a) { }; oneArg(24);",
                ExpectedConstants = new()
                {
                    new List<Instructions>
                    {
                        Code.Make(Opcode.OpReturn)
                    },
                    24
                },
                ExpectedInstructions = new()
                {
                    Code.Make(Opcode.OpClosure, new() {0,0}),
                    Code.Make(Opcode.OpSetGlobal, new() {0}),
                    Code.Make(Opcode.OpGetGlobal, new() {0}),
                    Code.Make(Opcode.OpConstant, new() {1}),
                    Code.Make(Opcode.OpCall, new() {1}),
                    Code.Make(Opcode.OpPop)
                }
            },
            new()
            {
                Input = "let manyArg = fn(a, b, c) { }; manyArg(24, 25, 26);",
                ExpectedConstants = new()
                {
                    new List<Instructions>
                    {
                        Code.Make(Opcode.OpReturn)
                    },
                    24,
                    25,
                    26
                },
                ExpectedInstructions = new()
                {
                    Code.Make(Opcode.OpClosure, new() {0,0}),
                    Code.Make(Opcode.OpSetGlobal, new() {0}),
                    Code.Make(Opcode.OpGetGlobal, new() {0}),
                    Code.Make(Opcode.OpConstant, new() {1}),
                    Code.Make(Opcode.OpConstant, new() {2}),
                    Code.Make(Opcode.OpConstant, new() {3}),
                    Code.Make(Opcode.OpCall, new() {3}),
                    Code.Make(Opcode.OpPop)
                }
            },
            new()
            {
                Input = "let oneArg = fn(a) { a }; oneArg(24);",
                ExpectedConstants = new()
                {
                    new List<Instructions>
                    {
                        Code.Make(Opcode.OpGetLocal, new() {0}),
                        Code.Make(Opcode.OpReturnValue)
                    },
                    24
                },
                ExpectedInstructions = new()
                {
                    Code.Make(Opcode.OpClosure, new() {0,0}),
                    Code.Make(Opcode.OpSetGlobal, new() {0}),
                    Code.Make(Opcode.OpGetGlobal, new() {0}),
                    Code.Make(Opcode.OpConstant, new() {1}),
                    Code.Make(Opcode.OpCall, new() {1}),
                    Code.Make(Opcode.OpPop)
                }
            },
            new()
            {
                Input = "let manyArg = fn(a, b, c) { a; b; c }; manyArg(24, 25, 26)",
                ExpectedConstants = new()
                {
                    new List<Instructions>
                    {
                        Code.Make(Opcode.OpGetLocal, new() {0}),
                        Code.Make(Opcode.OpPop),
                        Code.Make(Opcode.OpGetLocal, new() {1}),
                        Code.Make(Opcode.OpPop),
                        Code.Make(Opcode.OpGetLocal, new() {2}),
                        Code.Make(Opcode.OpReturnValue)
                    },
                    24,
                    25,
                    26
                },
                ExpectedInstructions = new()
                {
                    Code.Make(Opcode.OpClosure, new() {0,0}),
                    Code.Make(Opcode.OpSetGlobal, new() {0}),
                    Code.Make(Opcode.OpGetGlobal, new() {0}),
                    Code.Make(Opcode.OpConstant, new() {1}),
                    Code.Make(Opcode.OpConstant, new() {2}),
                    Code.Make(Opcode.OpConstant, new() {3}),
                    Code.Make(Opcode.OpCall, new() {3}),
                    Code.Make(Opcode.OpPop)
                }
            }
        };
        
        RunCompilerTests(tests);
    }
    [Test]
    public void TestFunctionsWithoutReturnValue()
    {
        var tests = new List<CompilerTestCase>
        {
            new()
            {
                Input = "fn() { }",
                ExpectedConstants = new()
                {
                    new List<Instructions>
                    {
                        Code.Make(Opcode.OpReturn)
                    }
                },
                ExpectedInstructions = new()
                {
                    Code.Make(Opcode.OpClosure, new() {0, 0}),
                    Code.Make(Opcode.OpPop)
                }
            }
        };
        
        RunCompilerTests(tests);
    }
    [Test]
    public void TestCompilerScopes()
    {
        var compiler = new Compiler();
        if (compiler.ScopeIndex != 0)
        {
            Assert.Fail($"ScopeIndex wrong. Got {compiler.ScopeIndex}, want {0}");
        }
        var globalSymbolTable = compiler.SymbolTable;

        compiler.Emit(Opcode.OpMul);

        compiler.EnterScope();
        if (compiler.ScopeIndex != 1)
        {
            Assert.Fail($"ScopeIndex wrong. Got {compiler.ScopeIndex}, want {1}");
        }

        compiler.Emit(Opcode.OpSub);

        if (compiler.Scopes[compiler.ScopeIndex].Instructions.Count != 1)
        {
            Assert.Fail($"instructions length wrong. got {compiler.Scopes[compiler.ScopeIndex].Instructions.Count}");
        }

        var last = compiler.Scopes[compiler.ScopeIndex].LastInstruction;
        if (last.Opcode != Opcode.OpSub)
        {
            Assert.Fail($"LastInstruction.Opcode wrong. Got {last.Opcode}, Want {Opcode.OpSub}");
        }

        if (compiler.SymbolTable.Outer != globalSymbolTable)
        {
            Assert.Fail($"compiler did not enclose symbolTable");
        }

        compiler.LeaveScope();
        if (compiler.ScopeIndex != 0)
        {
            Assert.Fail($"ScopeIndex wrong. Got {compiler.ScopeIndex}, want {0}");
        }

        if (compiler.SymbolTable != globalSymbolTable)
        {
            Assert.Fail($"compiler did not restore global symbol table");
        }

        if (compiler.SymbolTable.Outer != null)
        {
            Assert.Fail($"compiler modified global symbol table incorrectly");
        }
        compiler.Emit(Opcode.OpAdd);

        if (compiler.Scopes[compiler.ScopeIndex].Instructions.Count != 2)
        {
            Assert.Fail($"instructions length wrong, got {compiler.Scopes[compiler.ScopeIndex].Instructions.Count}");
        }

        last = compiler.Scopes[compiler.ScopeIndex].LastInstruction;
        if (last.Opcode != Opcode.OpAdd)
        {
            Assert.Fail($"LastInstruction.Opcode wrong. Got {last.Opcode}, Want {Opcode.OpAdd}");
        }

        var previous = compiler.Scopes[compiler.ScopeIndex].PreviousInstruction;
        if (previous.Opcode != Opcode.OpMul)
        {
            
            Assert.Fail($"PreviousInstruction.Opcode wrong. Got {previous.Opcode}, Want {Opcode.OpMul}");
        }
    }
    [Test]
    public void TestFunctions()
    {
        var tests = new List<CompilerTestCase>
        {
            new()
            {
                Input = "fn() { return 5 + 10; }",
                ExpectedConstants = new()
                {
                    5, 10, new List<Instructions>
                    {
                        Code.Make(Opcode.OpConstant, new() {0}),
                        Code.Make(Opcode.OpConstant, new() {1}),
                        Code.Make(Opcode.OpAdd),
                        Code.Make(Opcode.OpReturnValue)
                    }
                },
                ExpectedInstructions = new()
                {
                    Code.Make(Opcode.OpClosure, new() {2,0}),
                    Code.Make(Opcode.OpPop)
                }
            },
            new()
            {
                Input = "fn() { 5 + 10 }",
                ExpectedConstants = new()
                {
                    5, 10, new List<Instructions>
                    {
                        Code.Make(Opcode.OpConstant, new() {0}),
                        Code.Make(Opcode.OpConstant, new() {1}),
                        Code.Make(Opcode.OpAdd),
                        Code.Make(Opcode.OpReturnValue)
                    }
                },
                ExpectedInstructions = new()
                {
                    Code.Make(Opcode.OpClosure, new() {2, 0}),
                    Code.Make(Opcode.OpPop)
                }
            },
            new()
            {
                Input = "fn() { 1; 2 }",
                ExpectedConstants = new()
                {
                    1, 2, new List<Instructions>
                    {
                        Code.Make(Opcode.OpConstant, new() {0}),
                        Code.Make(Opcode.OpPop),
                        Code.Make(Opcode.OpConstant, new() {1}),
                        Code.Make(Opcode.OpReturnValue)
                    }
                },
                ExpectedInstructions = new()
                {
                    Code.Make(Opcode.OpClosure, new() {2, 0}),
                    Code.Make(Opcode.OpPop)
                }
            },
        };
        
        RunCompilerTests(tests);
    }
    [Test]
    public void TestIndexExpressions()
    {
        var tests = new List<CompilerTestCase>
        {
            new()
            {
                Input = "[1, 2, 3][1 + 1]",
                ExpectedConstants = new() {1, 2, 3, 1, 1},
                ExpectedInstructions = new()
                {
                    Code.Make(Opcode.OpConstant, new() {0}),
                    Code.Make(Opcode.OpConstant, new() {1}),
                    Code.Make(Opcode.OpConstant, new() {2}),
                    Code.Make(Opcode.OpArray, new() {3}),
                    Code.Make(Opcode.OpConstant, new() {3}),
                    Code.Make(Opcode.OpConstant, new() {4}),
                    Code.Make(Opcode.OpAdd),
                    Code.Make(Opcode.OpIndex),
                    Code.Make(Opcode.OpPop)
                }
            },
            new()
            {
                Input = "{1: 2}[2 - 1]",
                ExpectedConstants = new() {1, 2, 2, 1},
                ExpectedInstructions = new()
                {
                    Code.Make(Opcode.OpConstant, new() {0}),
                    Code.Make(Opcode.OpConstant, new() {1}),
                    Code.Make(Opcode.OpHash, new() {2}),
                    Code.Make(Opcode.OpConstant, new() {2}),
                    Code.Make(Opcode.OpConstant, new() {3}),
                    Code.Make(Opcode.OpSub),
                    Code.Make(Opcode.OpIndex),
                    Code.Make(Opcode.OpPop)
                }
            }
        };
        
        RunCompilerTests(tests);
    }
    [Test]
    public void TestHashLiterals()
    {
        var tests = new List<CompilerTestCase>
        {
            new()
            {
                Input = "{}",
                ExpectedConstants = new(),
                ExpectedInstructions = new()
                {
                    Code.Make(Opcode.OpHash, new() {0}),
                    Code.Make(Opcode.OpPop)
                }
            },
            new()
            {
                Input = "{1: 2, 3: 4, 5: 6}",
                ExpectedConstants = new() {1, 2, 3, 4, 5, 6},
                ExpectedInstructions = new()
                {
                   Code.Make(Opcode.OpConstant, new() {0}),
                   Code.Make(Opcode.OpConstant, new() {1}),
                   Code.Make(Opcode.OpConstant, new() {2}),
                   Code.Make(Opcode.OpConstant, new() {3}),
                   Code.Make(Opcode.OpConstant, new() {4}),
                   Code.Make(Opcode.OpConstant, new() {5}),
                   Code.Make(Opcode.OpHash, new() {6}),
                   Code.Make(Opcode.OpPop)
                }
            },
            new()
            {
                Input = "{1: 2 + 3, 4: 5 * 6}",
                ExpectedConstants = new() {1, 2, 3, 4, 5, 6},
                ExpectedInstructions = new()
                {
                    
                   Code.Make(Opcode.OpConstant, new() {0}),
                   Code.Make(Opcode.OpConstant, new() {1}),
                   Code.Make(Opcode.OpConstant, new() {2}),
                   Code.Make(Opcode.OpAdd),
                   Code.Make(Opcode.OpConstant, new() {3}),
                   Code.Make(Opcode.OpConstant, new() {4}),
                   Code.Make(Opcode.OpConstant, new() {5}),
                   Code.Make(Opcode.OpMul),
                   Code.Make(Opcode.OpHash, new() {4}),
                   Code.Make(Opcode.OpPop)
                }
            }
        };
        
        RunCompilerTests(tests);
    }
    [Test]
    public void TestArrayLiterals()
    {
        var tests = new List<CompilerTestCase>
        {
            new()
            {
                Input = "[]",
                ExpectedConstants = new List<object>(),
                ExpectedInstructions = new List<Instructions>
                {
                    Code.Make(Opcode.OpArray, new List<int> {0}),
                    Code.Make(Opcode.OpPop)
                }
            },
            new()
            {
                Input = "[1, 2, 3]",
                ExpectedConstants = new() {1, 2, 3},
                ExpectedInstructions = new()
                {
                    Code.Make(Opcode.OpConstant, new() {0}),
                    Code.Make(Opcode.OpConstant, new() {1}),
                    Code.Make(Opcode.OpConstant, new() {2}),
                    Code.Make(Opcode.OpArray, new() {3}),
                    Code.Make(Opcode.OpPop)
                }
            },
            new()
            {
                Input = "[1 + 2, 3 - 4, 5 * 6]",
                ExpectedConstants = new() {1, 2, 3, 4, 5, 6},
                ExpectedInstructions = new()
                {
                    Code.Make(Opcode.OpConstant, new() {0}),
                    Code.Make(Opcode.OpConstant, new() {1}),
                    Code.Make(Opcode.OpAdd),
                    Code.Make(Opcode.OpConstant, new() {2}),
                    Code.Make(Opcode.OpConstant, new() {3}),
                    Code.Make(Opcode.OpSub),
                    Code.Make(Opcode.OpConstant, new() {4}),
                    Code.Make(Opcode.OpConstant, new() {5}),
                    Code.Make(Opcode.OpMul),
                    Code.Make(Opcode.OpArray, new() {3}),
                    Code.Make(Opcode.OpPop)
                }
            }
        };
        
        RunCompilerTests(tests);
    }
    [Test]
    public void TestStringExpressions()
    {
        var tests = new List<CompilerTestCase>
        {
            new()
            {
                Input = @"""monkey""",
                ExpectedConstants = new List<object> {"monkey"},
                ExpectedInstructions = new List<Instructions>
                {
                    Code.Make(Opcode.OpConstant, new List<int> {0}),
                    Code.Make(Opcode.OpPop)
                }
            },
            new()
            {
                Input = @"""mon"" + ""key""",
                ExpectedConstants = new List<object> {"mon", "key"},
                ExpectedInstructions = new List<Instructions>
                {
                    Code.Make(Opcode.OpConstant, new List<int> {0}),
                    Code.Make(Opcode.OpConstant, new List<int> {1}),
                    Code.Make(Opcode.OpAdd),
                    Code.Make(Opcode.OpPop)
                }
            }
        };
        
        RunCompilerTests(tests);
    }
    [Test]
    public void TestDefine()
    {
        var expected = new Dictionary<string, Symbol>
        {
            {"a", new Symbol {Name = "a", Scope = SymbolScope.GlobalScope, Index = 0}},
            {"b", new Symbol {Name = "b", Scope = SymbolScope.GlobalScope, Index = 1}},
            {"c", new Symbol {Name = "c", Scope = SymbolScope.LocalScope, Index = 0}},
            {"d", new Symbol {Name = "d", Scope = SymbolScope.LocalScope, Index = 1}},
            {"e", new Symbol {Name = "e", Scope = SymbolScope.LocalScope, Index = 0}},
            {"f", new Symbol {Name = "f", Scope = SymbolScope.LocalScope, Index = 1}},
        };

        var global = new SymbolTable();

        var a = global.Define("a");
        if (!a.Equals(expected["a"]))
        {
            Assert.Fail($"expected a={expected["a"]}, got={a}");
        }

        var b = global.Define("b");
        if (!b.Equals(expected["b"]))
        {
            Assert.Fail($"expected b={expected["b"]}, got={b}");
        }

        var firstLocal = new SymbolTable(global);

        var c = firstLocal.Define("c");
        if (!c.Equals(expected["c"]))
        {
            Assert.Fail($"expected c={expected["c"]}, got={c}");
        }
        
        var d = firstLocal.Define("d");
        if (!d.Equals(expected["d"]))
        {
            Assert.Fail($"expected d={expected["d"]}, got={d}");
        }
        
        var secondLocal = new SymbolTable(firstLocal);

        var e = secondLocal.Define("e");
        if (!e.Equals(expected["e"]))
        {
            Assert.Fail($"expected e={expected["e"]}, got={e}");
        }
        
        var f = secondLocal.Define("f");
        if (!f.Equals(expected["f"]))
        {
            Assert.Fail($"expected f={expected["f"]}, got={f}");
        }
    }

    [Test]
    public void TestResolveGlobal()
    {
        var global = new SymbolTable();
        global.Define("a");
        global.Define("b");

        var expected = new List<Symbol>
        {
            new Symbol {Name = "a", Scope = SymbolScope.GlobalScope, Index = 0},
            new Symbol {Name = "b", Scope = SymbolScope.GlobalScope, Index = 1},
        };

        foreach (var symbol in expected)
        {
            var (result, ok) = global.Resolve(symbol.Name);
            if (!ok)
            {
                Assert.Fail($"name {symbol.Name} is not resolvable");
            }

            if (!result.Equals(symbol))
            {
                Assert.Fail($"expected {symbol.Name} to resolve to {symbol}, got {result}");
            }
        }
    }
    [Test]
    public void TestGlobalLetStatements()
    {
        var tests = new List<CompilerTestCase>
        {
            new()
            {
                Input = @"let one = 1;
                        let two = 2;",
                ExpectedConstants = new List<object> {1, 2},
                ExpectedInstructions = new List<Instructions>
                {
                    Code.Make(Opcode.OpConstant, new List<int> {0}),
                    Code.Make(Opcode.OpSetGlobal, new List<int> {0}),
                    Code.Make(Opcode.OpConstant, new List<int> {1}),
                    Code.Make(Opcode.OpSetGlobal, new List<int> {1})
                }
            },
            new()
            {
                Input = @"let one = 1; 
                        one;",
                ExpectedConstants = new List<object> {1},
                ExpectedInstructions = new List<Instructions>
                {
                    Code.Make(Opcode.OpConstant, new List<int> {0}),
                    Code.Make(Opcode.OpSetGlobal, new List<int> {0}),
                    Code.Make(Opcode.OpGetGlobal, new List<int> {0}),
                    Code.Make(Opcode.OpPop)
                }
            },
            new()
            {
                Input = @"let one = 1; 
                        let two = one;
                        two;",
                ExpectedConstants = new List<object> {1},
                ExpectedInstructions = new List<Instructions>
                {
                    Code.Make(Opcode.OpConstant, new List<int> {0}),
                    Code.Make(Opcode.OpSetGlobal, new List<int> {0}),
                    Code.Make(Opcode.OpGetGlobal, new List<int> {0}),
                    Code.Make(Opcode.OpSetGlobal, new List<int> {1}),
                    Code.Make(Opcode.OpGetGlobal, new List<int> {1}),
                    Code.Make(Opcode.OpPop)
                }
            }
        };
        
        RunCompilerTests(tests);
    }
    [Test]
    public void TestConditionals()
    {
        var tests = new List<CompilerTestCase>
        {
            new()
            {
                Input = "if (true) { 10 }; 3333;",
                ExpectedConstants = new List<object> {10, 3333},
                ExpectedInstructions = new List<Instructions>
                {
                    // 0000
                    Code.Make(Opcode.OpTrue),
                    // 0001
                    Code.Make(Opcode.OpJumpNotTruthy, new List<int> {10}),
                    // 0004
                    Code.Make(Opcode.OpConstant, new List<int> {0}),
                    // 0007
                    Code.Make(Opcode.OpJump, new List<int> {11}),
                    // 0010
                    Code.Make(Opcode.OpNull),
                    // 0011
                    Code.Make(Opcode.OpPop),
                    // 0012
                    Code.Make(Opcode.OpConstant, new List<int> {1}),
                    // 0015
                    Code.Make(Opcode.OpPop)
                }
            },
            new()
            {
                Input = "if (true) { 10 } else { 20 }; 3333;",
                ExpectedConstants = new List<object> {10, 20, 3333},
                ExpectedInstructions = new List<Instructions>
                {
                    // 0000
                    Code.Make(Opcode.OpTrue),
                    // 0001
                    Code.Make(Opcode.OpJumpNotTruthy, new List<int> {10}),
                    // 0004
                    Code.Make(Opcode.OpConstant, new List<int> {0}),
                    // 0007
                    Code.Make(Opcode.OpJump, new List<int> {13}),
                    // 0010
                    Code.Make(Opcode.OpConstant, new List<int> {1}),
                    // 0013
                    Code.Make(Opcode.OpPop),
                    // 0014
                    Code.Make(Opcode.OpConstant, new List<int> {2}),
                    // 0017
                    Code.Make(Opcode.OpPop)
                }
            },
            new()
            {
                Input = "if (true) { 10 };",
                ExpectedConstants = new List<object> {10},
                ExpectedInstructions = new List<Instructions>
                {
                    // 0000
                    Code.Make(Opcode.OpTrue),
                    // 0001
                    Code.Make(Opcode.OpJumpNotTruthy, new List<int> {10}),
                    // 0004
                    Code.Make(Opcode.OpConstant, new List<int> {0}),
                    // 0007
                    Code.Make(Opcode.OpJump, new List<int> {11}),
                    // 0010
                    Code.Make(Opcode.OpNull),
                    // 0011
                    Code.Make(Opcode.OpPop),
                }
            },
        };
        
        RunCompilerTests(tests);
    }
    [Test]
    public void TestBooleanExpressions()
    {
        var tests = new List<CompilerTestCase> 
        {
            new()
            {
                Input = "true",
                ExpectedConstants = new List<object>(),
                ExpectedInstructions = new List<Instructions>
                {
                    Code.Make(Opcode.OpTrue),
                    Code.Make(Opcode.OpPop)
                }
            },
            new()
            {
                Input = "false",
                ExpectedConstants = new List<object>(),
                ExpectedInstructions = new List<Instructions>
                {
                    Code.Make(Opcode.OpFalse),
                    Code.Make(Opcode.OpPop)
                }
            },
            new()
            {
                Input = "1 > 2",
                ExpectedConstants = new List<object> {1, 2},
                ExpectedInstructions = new List<Instructions>
                {
                    Code.Make(Opcode.OpConstant, new List<int> {0}),
                    Code.Make(Opcode.OpConstant, new List<int> {1}),
                    Code.Make(Opcode.OpGreaterThan),
                    Code.Make(Opcode.OpPop)
                }
            },
            new()
            {
                Input = "1 < 2",
                ExpectedConstants = new List<object> {2, 1},
                ExpectedInstructions = new List<Instructions>
                {
                    Code.Make(Opcode.OpConstant, new List<int> {0}),
                    Code.Make(Opcode.OpConstant, new List<int> {1}),
                    Code.Make(Opcode.OpGreaterThan),
                    Code.Make(Opcode.OpPop)
                }
            },
            new()
            {
                Input = "1 == 2",
                ExpectedConstants = new List<object> {1, 2},
                ExpectedInstructions = new List<Instructions>
                {
                    Code.Make(Opcode.OpConstant, new List<int> {0}),
                    Code.Make(Opcode.OpConstant, new List<int> {1}),
                    Code.Make(Opcode.OpEqual),
                    Code.Make(Opcode.OpPop)
                }
            },
            new()
            {
                Input = "1 != 2",
                ExpectedConstants = new List<object> {1, 2},
                ExpectedInstructions = new List<Instructions>
                {
                    Code.Make(Opcode.OpConstant, new List<int> {0}),
                    Code.Make(Opcode.OpConstant, new List<int> {1}),
                    Code.Make(Opcode.OpNotEqual),
                    Code.Make(Opcode.OpPop)
                }
            },
            new()
            {
                Input = "true == false",
                ExpectedConstants = new List<object> {},
                ExpectedInstructions = new List<Instructions>
                {
                    Code.Make(Opcode.OpTrue),
                    Code.Make(Opcode.OpFalse),
                    Code.Make(Opcode.OpEqual),
                    Code.Make(Opcode.OpPop)
                }
            },
            new()
            {
                Input = "true != false",
                ExpectedConstants = new List<object> {},
                ExpectedInstructions = new List<Instructions>
                {
                    Code.Make(Opcode.OpTrue),
                    Code.Make(Opcode.OpFalse),
                    Code.Make(Opcode.OpNotEqual),
                    Code.Make(Opcode.OpPop)
                }
            },
            new()
            {
                Input = "!true",
                ExpectedConstants = new List<object>(),
                ExpectedInstructions = new List<Instructions>
                {
                    Code.Make(Opcode.OpTrue),
                    Code.Make(Opcode.OpBang),
                    Code.Make(Opcode.OpPop)
                }
            }
        };
        
        RunCompilerTests(tests);
    }
    private struct CompilerTestCase
    {
        public string Input { get; set; }
        public List<object> ExpectedConstants { get; set; }
        public List<Instructions> ExpectedInstructions { get; set; }
    }
    [Test]
    public void TestIntegerArithmetic()
    {
        var tests = new List<CompilerTestCase>
        {
            new()
            {
                Input = "1 + 2",
                ExpectedConstants = new List<object> {1, 2},
                ExpectedInstructions = new List<Instructions> {Code.Make(Opcode.OpConstant, new List<int> {0}), Code.Make(Opcode.OpConstant, new List<int> {1}), Code.Make(Opcode.OpAdd), Code.Make(Opcode.OpPop)}
            },
            new()
            {
                Input = "1; 2",
                ExpectedConstants = new List<object> {1, 2},
                ExpectedInstructions = new List<Instructions> {Code.Make(Opcode.OpConstant, new List<int> {0}), Code.Make(Opcode.OpPop), Code.Make(Opcode.OpConstant, new List<int> {1}), Code.Make(Opcode.OpPop)}
            },
            new()
            {
                Input = "1 - 2",
                ExpectedConstants = new List<object> {1, 2},
                ExpectedInstructions = new List<Instructions> {Code.Make(Opcode.OpConstant, new List<int> {0}), Code.Make(Opcode.OpConstant, new List<int> {1}), Code.Make(Opcode.OpSub), Code.Make(Opcode.OpPop)}
            },
            new()
            {
                Input = "1 * 2",
                ExpectedConstants = new List<object> {1, 2},
                ExpectedInstructions = new List<Instructions> {Code.Make(Opcode.OpConstant, new List<int> {0}), Code.Make(Opcode.OpConstant, new List<int> {1}), Code.Make(Opcode.OpMul), Code.Make(Opcode.OpPop)}
            },
            new()
            {
                Input = "1 / 2",
                ExpectedConstants = new List<object> {1, 2},
                ExpectedInstructions = new List<Instructions> {Code.Make(Opcode.OpConstant, new List<int> {0}), Code.Make(Opcode.OpConstant, new List<int> {1}), Code.Make(Opcode.OpDiv), Code.Make(Opcode.OpPop)}
            },
            new()
            {
                Input = "-1",
                ExpectedConstants = new List<object> {1},
                ExpectedInstructions = new List<Instructions> {Code.Make(Opcode.OpConstant, new List<int> {0}), Code.Make(Opcode.OpMinus), Code.Make(Opcode.OpPop)}
            }
        };

        RunCompilerTests(tests);
    }

    private void RunCompilerTests(List<CompilerTestCase> tests)
    {
        foreach (var tt in tests)
        {
            Console.WriteLine($"{JsonSerializer.Serialize(tt)}");
            var program = Parse(tt.Input);

            var compiler = new Compiler();
            var err = compiler.Compile(program);
            if (err != null)
            {
                Assert.Fail($"compiler error: {err}");
            }

            var bytecode = compiler.Bytecode();

            err = TestInstructions(tt.ExpectedInstructions, bytecode.Instructions);
            if (err != null)
            {
                Assert.Fail($"TestInstructions failed: {err}");
            }

            err = TestConstants(tt.ExpectedConstants, bytecode.Constants);
            if (err != null)
            {
                Assert.Fail($"TestConstants failed: {err}");
            }

        }
    }

    private string? TestConstants(List<object> expected, List<Object> actual)
    {
        if (expected.Count != actual.Count)
        {
            return $"wrong number of constants. Got '{JsonSerializer.Serialize(actual)}'. Want '{JsonSerializer.Serialize(expected)}'";
        }

        for (var i = 0; i < expected.Count; i++)
        {
            var constant = expected[i];
            if (constant is int number)
            {
                var err = TestIntegerObject(number, actual[i]);
                if (err is not null)
                {
                    return $"constant {i} - TestIntegerObject failed: {err}";
                }
            }

            if (constant is string str)
            {
                var err = TestStringObject(str, actual[i]);
                if (err is not null)
                {
                    return $"constant {i} - TestStringObjectFailed: {err}";
                }
            }

            if (constant is List<Instructions> list)
            {
                if (actual[i] is not CompiledFunction func)
                {
                    return $"constant {i} - not a function: {actual[i]}";
                }

                var err = TestInstructions(list, func.Instructions);
                if (err is not null)
                {
                    return $"constant {i} - TestInstructions failed: {err}";
                }
            }
        }

        return null;
    }

    private string? TestStringObject(string expected, Object actual)
    {
        if (actual is not String str)
        {
            return $"object is not string. got '{actual}'";
        }

        if (!str.Value.Equals(expected))
        {
            return $"object has wrong value. Got={str.Value} Want={expected}";
        }

        return null;
    }

    private string? TestIntegerObject(int expected, Object actual)
    {
        if (actual is not Integer result)
        {
            return $"object is not Integer. Got '{actual}'";
        }

        if (result.Value != expected)
        {
            return $"object has wrong value. Got '{result.Value}' Want '{expected}'";
        }

        return null;
    }

    private string? TestInstructions(List<Instructions> expected, Instructions actual)
    {
        var concatted = ConcatInstructions(expected);

        if (actual.Count != concatted.Count)
        {
            return $"wrong instructions length. Want \n'{concatted.String()}',\n Got \n'{actual.String()}'";
        }

        for (var i = 0; i < concatted.Count; i++)
        {
            var ins = concatted[i];
            if (actual[i] != ins)
            {
                return $"wrong instruction at {i}. Want '{concatted.String()}' Got '{actual.String()}'";
            }
        }

        return null;
    }

    private Instructions ConcatInstructions(List<Instructions> s)
    {
        var res = new Instructions();

        foreach (var ins in s)
        {
            res.AddRange(ins);
        }

        return res;
    }

    private Program Parse(string input)
    {
        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        return parser.ParseProgram();
    }
}