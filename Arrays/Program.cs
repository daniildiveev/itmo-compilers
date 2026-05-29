using SimpleLexer;
using SimpleParser;
using SimpleSemantic;
using SimpleTypeChecker;
using SimpleOptimizer;

namespace SimpleArrays;


class Program
{
    static string literalAndIndex = @"var a = [10, 20, 30];
print a;
print a[0];
print a[2];";

    static string indexWrite = @"var a = [1, 2, 3];
a[1] = 42;
print a;";

    static string lengthBuiltin = @"var a = [5, 6, 7, 8];
print length(a);
var empty = [];
print length(empty);";

    static string loopSum = @"var a = [1, 2, 3, 4, 5];
var sum = 0;
var i = 0;
while (i < length(a)) {
  sum = sum + a[i];
  i = i + 1;
}
print sum;";

    static string nestedExprIndex = @"var a = [100, 200, 300];
var i = 1;
print a[i + 1];";

    static string constFold = @"print [1 + 1, 2 * 3, 10 - 4][2];";

    static string outOfBounds = @"var a = [1, 2];
print a[5];";

    static string typeMismatch = @"var a = [1, 2 > 1];
print a;";

    static string indexNotNumber = @"var a = [1, 2, 3];
var b = [9];
print a[b];";

    static void RunCase(string label, string source)
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
            List<SemanticError> semErrors = analyzer.Analyze();
            if (semErrors.Count > 0)
            {
                for (int i = 0; i < semErrors.Count; i++)
                    Console.WriteLine(semErrors[i].ToString());
                Console.WriteLine();
                return;
            }

            TypeChecker checker = new TypeChecker(statements);
            List<TypeError> typeErrors = checker.Check();
            if (typeErrors.Count > 0)
            {
                for (int i = 0; i < typeErrors.Count; i++)
                    Console.WriteLine(typeErrors[i].ToString());
                Console.WriteLine();
                return;
            }

            Optimizer optimizer = new Optimizer();
            List<Statement> optimized = optimizer.Optimize(statements);

            SimpleInterpreter.Interpreter interp = new SimpleInterpreter.Interpreter();
            interp.Interpret(optimized);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        Console.WriteLine();
    }

    static void Main()
    {
        RunCase("LITERAL + INDEX READ", literalAndIndex);     // [10, 20, 30] / 10 / 30
        RunCase("INDEX WRITE", indexWrite);                    // [1, 42, 3]
        RunCase("LENGTH BUILTIN", lengthBuiltin);              // 4 / 0
        RunCase("LOOP SUM", loopSum);                          // 15
        RunCase("COMPUTED INDEX", nestedExprIndex);            // 300
        RunCase("CONST FOLD IN LITERAL", constFold);           // 6
        RunCase("OUT OF BOUNDS (runtime error)", outOfBounds); // [Runtime Error] Array index 5 out of bounds (length 2)
        RunCase("TYPE MISMATCH (type error)", typeMismatch);   // [Type Error] Array elements must have the same type
        RunCase("INDEX NOT NUMBER (type error)", indexNotNumber); // [Type Error] Array index must be Number
    }
}
