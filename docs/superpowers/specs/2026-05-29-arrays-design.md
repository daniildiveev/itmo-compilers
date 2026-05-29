# Arrays Support — Design Spec

Date: 2026-05-29
Branch: `feat/arrays`

## Goal

Add 1-dimensional array support across the full compiler pipeline: Lexer →
Parser → Semantic → TypeChecker → Optimizer → Interpreter.

## Language Surface

```
var a = [1, 2, 3];      // array literal
print a[0];             // index read
a[1] = 42;              // index write (mutation)
print length(a);        // builtin length
var empty = [];         // empty literal
```

### Scope Decisions

- **1D arrays only.** Element type is a scalar (Number or Bool). No nested
  `Number[][]`. Keeps `SimpleType` flat.
- **`length` is a builtin call.** Parsed as an ordinary `CallExpression`,
  special-cased ahead of user-function lookup in Semantic, TypeChecker, and
  Interpreter.
- **Empty `[]`** has element type `Unknown` (type `UnknownArray`); it unifies
  with the first concrete element type on use.
- **Out-of-bounds → runtime error.** The Interpreter throws a clear runtime
  error; out-of-bounds does not return a default value.
- **Typed elements.** All elements of a literal must share one scalar type;
  mismatched element types are a type error.

## Per-Stage Design

### 1. Lexer (`Lexer/TokenType.cs`, `Lexer/Lexer.cs`)

- Add `LBRACKET` (`[`) and `RBRACKET` (`]`) to the `TokenType` enum.
- Add `"["` and `"]"` to the `Operators` dictionary.
- No `Lexer.cs` logic change needed — single-char punctuation is handled by the
  existing `ReadOperatorOrPunctuation()`.

### 2. Parser

**`Parser/Nodes.cs` — new nodes:**

- `ArrayLiteralExpression : Expression`
  - `List<Expression> Elements`
- `ArrayIndexExpression : Expression`
  - `Expression Array`
  - `Expression Index`
- `IndexAssignStatement : Statement`
  - `Expression Array`
  - `Expression Index`
  - `Expression Value`

**`Parser/Parser.cs`:**

- `ParsePrimary()`: on `LBRACKET`, parse a comma-separated list of expressions
  until `RBRACKET` (allow empty) → `ArrayLiteralExpression`.
- After parsing a primary, run a postfix loop: while the next token is
  `LBRACKET`, consume `[`, parse an index expression, consume `]`, and wrap the
  current expression in `ArrayIndexExpression`. Chained `a[i][j]` is accepted
  structurally (typing may still reject it).
- `ParseAssignOrExpressionStatement()`: when a parsed primary is an
  `ArrayIndexExpression` followed by `EQ`, produce an `IndexAssignStatement`
  (`Array`, `Index`, `Value`). Otherwise fall through to the existing
  expression-statement / variable-assignment handling.

**`Parser/AstPrinter.cs`:** add render branches for the three new node types.

### 3. SimpleType (`TypeChecker/SimpleType.cs`)

Extend the `SimpleType` enum with flat array types (avoids a parameterized-type
rewrite):

- `NumberArray`
- `BoolArray`
- `UnknownArray`

Helpers:

- `ElementType(SimpleType arrayType)` → scalar type (`Number`, `Bool`,
  `Unknown`); `Unknown` if argument is not an array type.
- `ArrayOf(SimpleType elemType)` → matching array type.
- `IsArray(SimpleType)` predicate.

### 4. Semantic (`Semantic/Analyzer.cs`)

- `AnalyzeExpression`:
  - `ArrayLiteralExpression`: analyze each element expression.
  - `ArrayIndexExpression`: analyze the array expression and the index.
- `AnalyzeStatement`:
  - `IndexAssignStatement`: ensure the array target is declared; analyze index
    and value.
- Register `length` as a known builtin so arity/declaration checks pass
  (arity 1). The builtin must not collide with a user-defined function of the
  same name — a user `length` definition shadows the builtin (analyzer treats
  the declared function as authoritative).

### 5. TypeChecker (`TypeChecker/TypeChecker.cs`)

- `InferExpression`:
  - `ArrayLiteralExpression`: infer every element type; require all equal to one
    scalar type → `ArrayOf(scalar)`. Empty literal → `UnknownArray`. Mixed
    element types → type error.
  - `ArrayIndexExpression`: the array operand must be an array type; the index
    must be `Number`; result is `ElementType(arrayType)`.
- `CheckStatement`:
  - `IndexAssignStatement`: the target must be an array type; the assigned value
    type must equal `ElementType(arrayType)` (or the array is `UnknownArray`).
- `length(arr)` builtin: argument must be an array type; returns `Number`.
  Handled before user-function signature lookup.

### 6. Interpreter (`Interpreter/Interpreter.cs`)

- Array runtime value: `List<object>` (mutable; reference semantics so `a[i]=x`
  mutates the same array bound to `a`).
- `Eval`:
  - `ArrayLiteralExpression`: eval each element into a new `List<object>`.
  - `ArrayIndexExpression`: eval array (expect `List<object>`) and index (expect
    `double`); cast index to `int`; bounds-check; throw a runtime error on
    out-of-bounds; return the element.
- `Exec`:
  - `IndexAssignStatement`: eval array, index, value; bounds-check; assign.
- `length(arr)` builtin: return `(double)list.Count`. Handled before user
  function dispatch.

### 7. Optimizer (`Optimizer/Optimizer.cs`)

- `OptExpr`: recurse into `ArrayLiteralExpression.Elements` and into the
  `Array` / `Index` subexpressions of `ArrayIndexExpression`; return rebuilt
  nodes.
- `OptStmt`: handle `IndexAssignStatement` by optimizing its three subexpressions.
- `IsPure`: an `ArrayIndexExpression` is pure iff its subexpressions are pure;
  `IndexAssignStatement` is impure. An `ArrayLiteralExpression` is pure iff all
  elements are pure.
- No constant folding of array indexing (`[..][k]` → element) — low value, out
  of scope (YAGNI).

## Testing

- Add array sample programs to each stage's `Program.cs` driver where the stage
  already demonstrates samples.
- `Functions/Program.cs` full-pipeline cases:
  - build, index-read, index-write, `length`.
  - loop summing array elements using `length` as the bound.
  - out-of-bounds access → runtime error surfaced.
  - element-type mismatch literal → type error surfaced.

## Out of Scope

- Nested / multi-dimensional arrays.
- Array slicing, concatenation, push/pop, or other builtins beyond `length`.
- Compile-time bounds checking.
- Constant folding of array indexing.
