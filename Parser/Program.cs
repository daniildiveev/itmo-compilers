using SimpleLexer;

namespace SimpleParser;

class Program
{
    static string sampleCode = @"  var limit = 10;
  var current = 0;

  while (current < limit) {
      if (current == 5) {
          print current * 100;
      } else {
          print current;
      }
      current = current + 1;
  }";

    static void Main()
    {
        try
        {   
            Console.WriteLine("Parsing sample code...");
            Console.WriteLine("--------------------------------");
            Console.WriteLine(sampleCode);
            Console.WriteLine("--------------------------------");
            Lexer lexer = new Lexer(sampleCode);
            List<Token> tokens = new List<Token>();
            foreach (Token t in lexer.Tokenize())
                tokens.Add(t);
            // Console.WriteLine("=== TOKENS ===");
            // for (int i = 0; i < tokens.Count; i++)
            //     Console.WriteLine(tokens[i].ToString());
            Parser parser = new Parser(tokens);
            List<Statement> statements = parser.Parse();
            Console.WriteLine("=== AST ===");
            AstPrinter printer = new AstPrinter();
            printer.Print(statements);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
