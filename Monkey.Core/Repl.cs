using Monkey.Core.Object;
using Environment = Monkey.Core.Object.Environment;

namespace Monkey.Core;

public class Repl {
    private static readonly string PROMPT = ">> ";

    public void Start(TextReader input, TextWriter output)
    {
        Console.SetIn(input);
        Console.SetOut(output);

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

            var comp = new Compiler.Compiler();
            var err = comp.Compile(program);
            if (err is not null)
            {
                Console.WriteLine($"Woops! Compilation failed:\n {err}");
            }

            var machine = new Vm.Vm(comp.Bytecode());
            err = machine.Run();
            if (err is not null)
            {
                Console.WriteLine($"Woops! Executing bytecode failed:\n {err}");
            }

            var stackTop = machine.StackTop();
            Console.WriteLine($"{stackTop?.Inspect()}");
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
