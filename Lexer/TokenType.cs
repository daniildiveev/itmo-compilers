namespace SimpleLexer;

public enum TokenType
{
    NUMBER, ID, STRING,
    VAR, PRINT,
    IF, ELSE, WHILE,
    FUN, RETURN,
    PLUS, MINUS, STAR, SLASH,
    EQ, EQEQ, EXCL, NEQ,
    LT, GT, LTEQ, GTEQ,
    AND, OR,
    LPAREN, RPAREN,
    LBRACE, RBRACE,
    LBRACKET, RBRACKET,
    COMMA, SEMICOLON,
    EOF
}
