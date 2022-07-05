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

            Token tok = lexer.NextToken();

            while(tok.Type != TokenType.EOF) 
            {
                Console.WriteLine($"{tok}");
                tok = lexer.NextToken();
            }
        }
    }
}
