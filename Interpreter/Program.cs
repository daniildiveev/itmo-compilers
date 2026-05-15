using SimpleLexer;
using SimpleParser;

namespace SimpleInterpreter;

class Program
{
    static string sampleCode = @"fun add(a, b) {
  return a + b;
}
fun fact(n) {
  if (n <= 1) { return 1; }
  return n * fact(n - 1);
}
var s = add(2, 3);
print s;
print fact(5);";

    static void Main()
    {
        try
        {
            Lexer lexer = new Lexer(sampleCode);
            List<Token> tokens = new List<Token>();
            foreach (Token t in lexer.Tokenize())
                tokens.Add(t);
            Parser parser = new Parser(tokens);
            List<Statement> statements = parser.Parse();
            Interpreter interp = new Interpreter();
            interp.Interpret(statements);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
