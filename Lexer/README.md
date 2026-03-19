## 1. What is a lexer?

A lexer reads raw source text from left to right and splits it into small pieces called tokens. Each token marks something meaningful, like a number, a name, or a plus sign. This lexer is built for a tiny language: it turns sample programs made of keywords, names, numbers, and punctuation into a stream of tokens you could feed to a parser later.

## 2. Token types

**Literals:** `NUMBER` is a run of digits (like `10`). `STRING` would be quoted text; this project lists it so the token set is complete, but the scanner here does not read string literals yet.

**Identifier:** `ID` is a name made of letters and digits (like `x` or `foo2`). If the text matches a keyword, the lexer uses the keyword type instead of `ID`.

**Keywords:** `VAR`, `PRINT`, `IF`, `ELSE`, and `WHILE` are reserved words that control variables, output, and control flow.

**Operators:** `PLUS`, `MINUS`, `STAR`, `SLASH` are arithmetic. `EQ` is assignment `=`; `EQEQ` and `NEQ` are equals and not-equals; `LT`, `GT`, `LTEQ`, `GTEQ` compare values; `EXCL` is `!`; `AND` and `OR` are `&&` and `||`.

**Grouping and punctuation:** `LPAREN`/`RPAREN` are `(` and `)`; `LBRACE`/`RBRACE` are `{` and `}`; `SEMICOLON` ends a statement.

**End of file:** `EOF` marks that the whole input has been read.

## 3. How the lexer works

The `Tokenize` method walks the input while the current index is still inside the string. It looks at the next character without moving yet. If that character is a space, tab, carriage return, or newline, it moves past it and continues the loop without emitting a token. If it is a digit, it reads every following digit as one `NUMBER` token. If it is a letter, it reads letters and digits as one word, then checks a keyword table; if there is a match it emits that keyword, otherwise it emits `ID`. For anything else it tries to read an operator or punctuation: it first checks whether the next two characters together form a known two-character operator (so `==` wins over `=`). If not, it checks a single character. If neither works, it stops with an error that names the bad character and the line and column where it started. After the loop finishes, it always emits one `EOF` token.

## 4. File overview

`Lexer.csproj` sets up the project as a .NET 8 console app. `TokenType.cs` lists every kind of token as an enum. `Token.cs` holds one token’s type, text, position in the file, line, and column, and formats a short string for printing. `Lexer.cs` holds the scanning logic, the keyword and operator tables, and the helper methods that read numbers, words, and symbols. `Program.cs` stores a small sample program and prints every token the lexer produces.

## 5. How to run

Open a terminal in the `Lexer` folder (the one that contains `Lexer.csproj`) and run:

```bash
dotnet run
```

The project file sets the output type to `Exe` so the tooling treats this as a runnable console program.

The first lines of output look like this (line and column numbers count from 1; the sample program starts with two spaces):

```
[1:3] Token(VAR, 'var')
[1:7] Token(ID, 'x')
[1:9] Token(EQ, '=')
[1:11] Token(NUMBER, '10')
[1:13] Token(SEMICOLON, ';')
[2:3] Token(VAR, 'var')
```

You will then see the rest of the tokens for the sample, ending with an `EOF` line.
