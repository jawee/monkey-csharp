using Monkey.Core.Compiler;
using Monkey.Core.Object;
using Environment = Monkey.Core.Object.Environment;

namespace Monkey.Core;

public class Repl {
    private static readonly string PROMPT = ">> ";

    public void Start(TextReader input, TextWriter output)
    {
        Console.SetIn(input);
        Console.SetOut(output);

        var globals = new List<Object.Object>();
        var symbolTable = new SymbolTable();
        var builtins = new Builtins();
        for (var i = 0; i < builtins.Count; i++)
        {
            var v = builtins[i];
            symbolTable.DefineBuiltin(i, v.Name);
        }

        while (true) {
            Console.Write(PROMPT);

            var scanned = Console.ReadLine();
            if (scanned == null) 
            {
                return;
            }

            var lexer = new Lexer(scanned);
            var parser = new Parser(lexer);

            var program = parser.ParseProgram();

            if (parser.Errors().Count != 0)
            {
                PrintParserErrors(output, parser.Errors());
                continue;
            }

            var comp = new Compiler.Compiler(symbolTable, globals);
            var err = comp.Compile(program);
            if (err is not null)
            {
                Console.WriteLine($"Woops! Compilation failed:\n {err}");
            }

            var code = comp.Bytecode();
            var constants = code.Constants;
            

            var machine = new Vm.Vm(code, globals);
            err = machine.Run();
            if (err is not null)
            {
                Console.WriteLine($"Woops! Executing bytecode failed:\n {err}");
            }

            var lastPopped = machine.LastPoppedStackElem();
            Console.WriteLine($"{lastPopped?.Inspect()}");
        }
    }
    
    private void PrintParserErrors(TextWriter output, List<string> errors)
    {
        Console.SetOut(output);
        Console.WriteLine("Woops! We ran into some monkey business here!");
        Console.WriteLine("Parser errors:");
        foreach (var error in errors)
        {
            Console.WriteLine($"\t{error}");
        }
    }
}
