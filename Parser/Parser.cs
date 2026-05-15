using SimpleLexer;

namespace SimpleParser;

public class Parser
{
    private List<Token> _tokens;
    private int _position;
    public Parser(List<Token> tokens)
    {
        _tokens = tokens;
    }
    public List<Statement> Parse()
    {
        List<Statement> statements = new List<Statement>();
        while (Peek().Type != TokenType.EOF)
            statements.Add(ParseStatement());
        return statements;
    }
    private Token Peek()
    {
        if (_position < _tokens.Count)
            return _tokens[_position];
        return _tokens[_tokens.Count - 1];
    }
    private Token Previous()
    {
        return _tokens[_position - 1];
    }
    private Token Advance()
    {
        if (Peek().Type != TokenType.EOF)
            _position++;
        return Previous();
    }
    private bool Check(TokenType type)
    {
        return Peek().Type == type;
    }
    private bool Match(params TokenType[] types)
    {
        for (int i = 0; i < types.Length; i++)
            if (Check(types[i]))
            {
                Advance();
                return true;
            }
        return false;
    }
    private Token Consume(TokenType type, string errorMsg)
    {
        if (Check(type))
            return Advance();
        throw new Exception("[Parser Error] " + errorMsg + " at Line " + Peek().Line + ", Column " + Peek().Column);
    }
    private Statement ParseStatement()
    {
        if (Match(TokenType.VAR))
            return ParseVarDeclaration();
        if (Match(TokenType.PRINT))
            return ParsePrintStatement();
        if (Match(TokenType.IF))
            return ParseIfStatement();
        if (Match(TokenType.WHILE))
            return ParseWhileStatement();
        if (Match(TokenType.LBRACE))
            return ParseBlock();
        return ParseAssignOrExpressionStatement();
    }
    private Statement ParseVarDeclaration()
    {
        Token name = Consume(TokenType.ID, "Expected variable name after 'var'");
        Expression? init = null;
        if (Match(TokenType.EQ))
            init = ParseExpression();
        Consume(TokenType.SEMICOLON, "Expected ';' after variable declaration");
        return new VarStatement(name.Value, init);
    }
    private Statement ParsePrintStatement()
    {
        Expression expr = ParseExpression();
        Consume(TokenType.SEMICOLON, "Expected ';' after print");
        return new PrintStatement(expr);
    }
    private Statement ParseIfStatement()
    {
        Consume(TokenType.LPAREN, "Expected '(' after 'if'");
        Expression condition = ParseExpression();
        Consume(TokenType.RPAREN, "Expected ')' after if condition");
        Statement thenBranch = ParseStatement();
        Statement? elseBranch = null;
        if (Match(TokenType.ELSE))
            elseBranch = ParseStatement();
        return new IfStatement(condition, thenBranch, elseBranch);
    }
    private Statement ParseWhileStatement()
    {
        Consume(TokenType.LPAREN, "Expected '(' after 'while'");
        Expression condition = ParseExpression();
        Consume(TokenType.RPAREN, "Expected ')' after while condition");
        return new WhileStatement(condition, ParseStatement());
    }
    private Statement ParseBlock()
    {
        List<Statement> statements = new List<Statement>();
        while (!Check(TokenType.RBRACE) && Peek().Type != TokenType.EOF)
            statements.Add(ParseStatement());
        Consume(TokenType.RBRACE, "Expected '}' to close block");
        return new BlockStatement(statements);
    }
    private Statement ParseAssignOrExpressionStatement()
    {
        if (Check(TokenType.ID) && _position + 1 < _tokens.Count && _tokens[_position + 1].Type == TokenType.EQ)
        {
            Token name = Advance();
            Advance();
            Expression value = ParseExpression();
            Consume(TokenType.SEMICOLON, "Expected ';' after assignment");
            return new AssignStatement(name.Value, value);
        }
        Expression expr = ParseExpression();
        Consume(TokenType.SEMICOLON, "Expected ';' after expression");
        return new ExpressionStatement(expr);
    }
    private Expression ParseExpression()
    {
        return ParseLogical();
    }
    private Expression ParseLogical()
    {
        Expression expr = ParseComparison();
        while (Match(TokenType.AND, TokenType.OR))
            expr = new BinaryExpression(expr, Previous().Type, ParseComparison());
        return expr;
    }
    private Expression ParseComparison()
    {
        Expression expr = ParseAddition();
        while (Match(TokenType.EQEQ, TokenType.NEQ, TokenType.LT, TokenType.GT, TokenType.LTEQ, TokenType.GTEQ))
            expr = new BinaryExpression(expr, Previous().Type, ParseAddition());
        return expr;
    }
    private Expression ParseAddition()
    {
        Expression expr = ParseMultiplication();
        while (Match(TokenType.PLUS, TokenType.MINUS))
            expr = new BinaryExpression(expr, Previous().Type, ParseMultiplication());
        return expr;
    }
    private Expression ParseMultiplication()
    {
        Expression expr = ParseUnary();
        while (Match(TokenType.STAR, TokenType.SLASH))
            expr = new BinaryExpression(expr, Previous().Type, ParseUnary());
        return expr;
    }
    private Expression ParseUnary()
    {
        if (Match(TokenType.EXCL, TokenType.MINUS))
            return new UnaryExpression(Previous().Type, ParseUnary());
        return ParsePrimary();
    }
    private Expression ParsePrimary()
    {
        if (Match(TokenType.NUMBER))
            return new NumberExpression(double.Parse(Previous().Value, System.Globalization.CultureInfo.InvariantCulture));
        if (Match(TokenType.ID))
            return new VariableExpression(Previous().Value);
        if (Match(TokenType.LPAREN))
        {
            Expression inner = ParseExpression();
            Consume(TokenType.RPAREN, "Expected ')' after expression");
            return new GroupExpression(inner);
        }
        throw new Exception("[Parser Error] Unexpected token '" + Peek().Value + "' at Line " + Peek().Line + ", Column " + Peek().Column);
    }
}
