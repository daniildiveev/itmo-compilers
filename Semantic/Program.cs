using SimpleLexer;
using SimpleParser;

namespace SimpleSemantic;

class Program
{
    static string errorSample = @"var x = 10;
var x = 20;
print y;
print x / 0;";

    static string cleanSample = @"fun add(a, b) {
  return a + b;
}
var s = add(2, 3);
print s;";

    static string funcErrorSample = @"fun add(a, b) { return a + b; }
print add(1);
print mul(1, 2);
return 5;";

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
            Analyzer analyzer = new Analyzer(statements);
            List<SemanticError> errors = analyzer.Analyze();
            Console.WriteLine("=== SEMANTIC ANALYSIS ===");
            if (errors.Count == 0)
            {
                Console.WriteLine("No semantic errors found.");
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
        RunPipeline("SAMPLE WITH ERRORS", errorSample);
        RunPipeline("CLEAN SAMPLE", cleanSample);
        RunPipeline("FUNCTION ERRORS", funcErrorSample);
    }
}
