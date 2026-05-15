using SimpleLexer;
using SimpleParser;
using SimpleSemantic;
using SimpleTypeChecker;

namespace SimpleFunctions;

class Program
{
    static string factorialSample = @"fun fact(n) {
  if (n <= 1) { return 1; }
  return n * fact(n - 1);
}
print fact(5);
print fact(7);";

    static string addSample = @"fun add(a, b) {
  return a + b;
}
fun square(x) {
  return x * x;
}
var s = add(2, 3);
print s;
print square(add(4, 6));";

    static string mutualSample = @"fun isEven(n) {
  if (n == 0) { return 1; }
  return isOdd(n - 1);
}
fun isOdd(n) {
  if (n == 0) { return 0; }
  return isEven(n - 1);
}
print isEven(4);
print isEven(7);";

    static string errorSample = @"fun add(a, b) { return a + b; }
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

            Console.WriteLine("--- AST ---");
            AstPrinter printer = new AstPrinter();
            printer.Print(statements);

            Console.WriteLine("--- SEMANTIC ---");
            Analyzer analyzer = new Analyzer(statements);
            List<SemanticError> semErrors = analyzer.Analyze();
            if (semErrors.Count == 0)
                Console.WriteLine("OK");
            else
                for (int i = 0; i < semErrors.Count; i++)
                    Console.WriteLine(semErrors[i].ToString());

            Console.WriteLine("--- TYPE CHECK ---");
            TypeChecker checker = new TypeChecker(statements);
            List<TypeError> typeErrors = checker.Check();
            if (typeErrors.Count == 0)
                Console.WriteLine("OK");
            else
                for (int i = 0; i < typeErrors.Count; i++)
                    Console.WriteLine(typeErrors[i].ToString());

            if (semErrors.Count == 0 && typeErrors.Count == 0)
            {
                Console.WriteLine("--- OUTPUT ---");
                SimpleInterpreter.Interpreter interp = new SimpleInterpreter.Interpreter();
                interp.Interpret(statements);
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
        RunPipeline("FACTORIAL (recursion)", factorialSample);
        RunPipeline("ADD + SQUARE (nested call)", addSample);
        RunPipeline("MUTUAL RECURSION", mutualSample);
        RunPipeline("ERROR CASES", errorSample);
    }
}
