namespace SimpleLexer;

class Program
{
    static string sampleCode = @"  var x = 10;
  var y = 20;
  if (x < y) {
      print x;
  } else {
      print y;
  }
  while (x != 0) {
      x = x - 1;
  }";

    static void Main()
    {
        Lexer lexer = new Lexer(sampleCode);
        foreach (Token token in lexer.Tokenize())
        {
            Console.WriteLine(token.ToString());
        }
    }
}
