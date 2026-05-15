# Functions

Adds function support to lang. Like teaching calculator to remember recipes.

## What new?

Two keywords + three statement types:

- `fun name(a, b) { ... }` — define recipe
- `return x;` — give answer back
- `name(1, 2)` — use recipe with ingredients

Example:

```
fun add(a, b) {
  return a + b;
}
print add(2, 3);   // prints 5
```

No type annotations on params. Compiler guesses types from how you call function.

## What changed in each package?

### Lexer
Taught it 3 new words: `fun`, `return`, `,` (comma).

### Parser
Three new AST node shapes:
- `FunctionDeclaration` — recipe card (name + params + body)
- `ReturnStatement` — "here is answer"
- `CallExpression` — "run this recipe with these args"

Parser now recognizes `fun foo(...)` at start of statement, `return ...;`, and `name(args)` inside expressions.

### Semantic
Tracks declared functions separately from variables (different namespace). Checks:
- function used but never defined → error
- wrong number of args → error
- `return` outside function → error
- duplicate param names → error

**Hoisting**: scans for `fun` decls first before analyzing bodies. Lets `isEven` call `isOdd` even though `isOdd` defined later (mutual recursion).

### TypeChecker
Each function gets `FunctionSignature` (param types + return type). Params start as `Unknown`. First call fills them in. Later calls with wrong types → error. Return type learned from first `return` in body; conflicting returns → error.

Same hoisting trick as Semantic.

### Interpreter
- `FunctionValue` — runtime recipe (params + body + closure env)
- `ReturnException` — thrown by `return`, caught by call site, carries result value
- Call: make new env, bind params to args, run body, catch `ReturnException`, get value

Closures work: function remembers env where it was defined.

### Functions (this package)
New driver. Runs full pipeline (lex → parse → AST print → semantic → typecheck → interpret) on 4 sample programs:
1. Factorial (recursion)
2. Nested calls (`square(add(4,6))`)
3. Mutual recursion (`isEven`/`isOdd`)
4. Error cases (wrong arity, undefined function, stray return)

## Run

```
dotnet run --project Functions/Functions.csproj
```
