using System.Collections.Generic;

namespace SimpleLexer;

public class Lexer
{
    private readonly string _input;
    private int _position;
    private int _line = 1;
    private int _column = 1;
    private static readonly Dictionary<string, TokenType> Keywords = new Dictionary<string, TokenType>
    {
        { "var", TokenType.VAR }, { "print", TokenType.PRINT }, { "if", TokenType.IF },
        { "else", TokenType.ELSE }, { "while", TokenType.WHILE }
    };
    private static readonly Dictionary<string, TokenType> Operators = new Dictionary<string, TokenType>
    {
        { "==", TokenType.EQEQ }, { "!=", TokenType.NEQ }, { "<=", TokenType.LTEQ }, { ">=", TokenType.GTEQ },
        { "&&", TokenType.AND }, { "||", TokenType.OR }, { "+", TokenType.PLUS }, { "-", TokenType.MINUS },
        { "*", TokenType.STAR }, { "/", TokenType.SLASH }, { "=", TokenType.EQ }, { "<", TokenType.LT },
        { ">", TokenType.GT }, { "!", TokenType.EXCL }, { "(", TokenType.LPAREN }, { ")", TokenType.RPAREN },
        { "{", TokenType.LBRACE }, { "}", TokenType.RBRACE }, { ";", TokenType.SEMICOLON }
    };

    public Lexer(string input)
    {
        if (input == null) _input = "";
        else _input = input;
    }

    public IEnumerable<Token> Tokenize()
    {
        while (_position < _input.Length)
        {
            char p = Peek();
            if (p == ' ' || p == '\t' || p == '\r' || p == '\n') { Next(); continue; }
            if (p >= '0' && p <= '9') { yield return ReadNumber(); continue; }
            if ((p >= 'a' && p <= 'z') || (p >= 'A' && p <= 'Z')) { yield return ReadWord(); continue; }
            yield return ReadOperatorOrPunctuation();
        }
        yield return new Token(TokenType.EOF, "\0", _position, _line, _column);
    }

    private Token ReadNumber()
    {
        int startPos = _position, startLine = _line, startCol = _column;
        while (Peek() >= '0' && Peek() <= '9') Next();
        string text = _input.Substring(startPos, _position - startPos);
        return new Token(TokenType.NUMBER, text, startPos, startLine, startCol);
    }

    private Token ReadWord()
    {
        int startPos = _position, startLine = _line, startCol = _column;
        for (char p = Peek(); (p >= 'a' && p <= 'z') || (p >= 'A' && p <= 'Z') || (p >= '0' && p <= '9'); p = Peek())
            Next();
        string text = _input.Substring(startPos, _position - startPos);
        TokenType type = TokenType.ID;
        if (Keywords.ContainsKey(text)) type = Keywords[text];
        return new Token(type, text, startPos, startLine, startCol);
    }

    private Token ReadOperatorOrPunctuation()
    {
        int startPos = _position, startLine = _line, startCol = _column;
        if (_position + 1 < _input.Length)
        {
            string two = _input.Substring(_position, 2);
            if (Operators.ContainsKey(two))
            {
                Next(); Next();
                return new Token(Operators[two], two, startPos, startLine, startCol);
            }
        }
        string one = _input.Substring(_position, 1);
        if (Operators.ContainsKey(one))
        {
            Next();
            return new Token(Operators[one], one, startPos, startLine, startCol);
        }
        throw new Exception("[Lexer Error] Unexpected character '" + Peek() + "' at Line " + startLine + ", Column " + startCol);
    }

    private char Peek()
    {
        if (_position >= _input.Length) return '\0';
        return _input[_position];
    }

    private char Next()
    {
        if (_position >= _input.Length) return '\0';
        char c = _input[_position++];
        if (c == '\n') { _line++; _column = 1; }
        else _column++;
        return c;
    }
}
