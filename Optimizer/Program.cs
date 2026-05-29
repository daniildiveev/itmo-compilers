using SimpleLexer;
using SimpleParser;

namespace SimpleOptimizer;

class Program
{
    static string[] samples = new string[]
    {
        // Constant folding + dead code
        @"var x = 2 + 3 * 4;
if (x > 1) { print x; } else { print 0; }
while (1 > 2) { print 99; }",

        // Algebraic identities
        @"var a;
a = 5;
print a * 1 + 0;
print a * 0;
print !!(a > 3);",

        // Unreachable code
        @"fun f(n) {
  return n + (0);
  print 12345;
}
print f(10);"
    };

    static void Demo(string source)
    {   
        Console.WriteLine("------");
        Console.WriteLine("=== SOURCE ===");
        Console.WriteLine(source);

        Lexer lexer = new Lexer(source);
        List<Token> tokens = new List<Token>();
        foreach (Token t in lexer.Tokenize())
            tokens.Add(t);

        Parser parser = new Parser(tokens);
        List<Statement> statements = parser.Parse();
        AstPrinter printer = new AstPrinter();
        Console.WriteLine("--- ORIGINAL AST ---");
        printer.Print(statements);

        Optimizer optimizer = new Optimizer();
        List<Statement> optimized = optimizer.Optimize(statements);
        Console.WriteLine("--- OPTIMIZED AST ---");
        printer.Print(optimized);
        Console.WriteLine();
    }

    static void Main()
    {
        for (int i = 0; i < samples.Length; i++)
            Demo(samples[i]);
    }
}
