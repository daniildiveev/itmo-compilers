using SimpleLexer;
using SimpleParser;

namespace SimpleTypeChecker;

class Program
{
    static string errorSample = @"var a = 10;
var b = a == 5;
var c = a + b;
while (a) {
    print a;
}";

    static string cleanSample = @"var limit = 10;
var current = 5;
while (current + limit) {
    if (current == 5) {
        print current * 100;
    } else {
        print current;
    }
    current = current + 1;
}";

    static void RunPipeline(string label, string source)
    {
        Console.WriteLine("=== " + label + " ===");
        Console.WriteLine(source);
        Console.WriteLine("--------------------------------");
        try
        {
            Lexer lexer = new Lexer(source);
            List<Token> tokens = new List<Token>();
            foreach (Token t in lexer.Tokenize())
                tokens.Add(t);
            Parser parser = new Parser(tokens);
            List<Statement> statements = parser.Parse();
            TypeChecker checker = new TypeChecker(statements);
            List<TypeError> errors = checker.Check();
            Console.WriteLine("=== TYPE CHECK ===");
            if (errors.Count == 0)
            {
                Console.WriteLine("No type errors found.");
            }
            else
            {
                for (int i = 0; i < errors.Count; i++)
                    Console.WriteLine(errors[i].ToString());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        Console.WriteLine();
    }

    static void Main()
    {
        RunPipeline("SAMPLE WITH TYPE ERRORS", errorSample);
        RunPipeline("CLEAN SAMPLE", cleanSample);
    }
}
