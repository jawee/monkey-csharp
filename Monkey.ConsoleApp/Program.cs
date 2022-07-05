// See https://aka.ms/new-console-template for more information
using Monkey.Core;

Console.WriteLine("Hello. This is the Monkey programming language!");
Console.WriteLine("Feel free to type in commands");
var repl = new Repl();
repl.Start(Console.In, Console.Out);
