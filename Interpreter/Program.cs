using SimpleLexer;
using SimpleParser;

namespace SimpleInterpreter;

class Program
{
    static string sampleCode = @"var x = 11;
var y = 0;
while (x > 0) {
  y = y + x;
  x = x - 1;
  print y;
}
print y;
if (y == 66) { print 1; } else { print 0; }";

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
