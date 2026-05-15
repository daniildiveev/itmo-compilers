# Interpreter

A tree-walking interpreter for the Simple language. It takes the AST produced
by `SimpleParser` and executes it directly, without any intermediate
compilation step.

## What is a tree-walking interpreter?

A tree-walking interpreter runs a program by recursively visiting the nodes of
its abstract syntax tree (AST). For every statement node, it performs the
corresponding action (declaring a variable, printing, looping, etc.). For every
expression node, it computes and returns a value. There is no bytecode, no
virtual machine, and no machine code generation — the AST *is* the program.

## Scoping with `Environment`

`Environment` is a small table that maps variable names to values. Each scope
(function body, block, etc.) has its own `Environment`, and each environment
holds an optional reference to a *parent* environment. When a variable is
looked up, the lookup walks up the parent chain until the variable is found or
the chain ends.

- `Define(name, value)` creates a variable in the **current** scope. It fails
  if a variable with that name already exists in the same scope.
- `Get(name)` returns the value. If the name is not defined in the current
  scope, it recurses into the parent. If nothing matches, it reports
  an undefined-variable error.
- `Set(name, value)` updates an existing variable. It walks up the parent
  chain to find the scope that defined it. Assigning to an undeclared variable
  is an error.

Entering a `BlockStatement` creates a new child environment; leaving the block
restores the previous one. This gives block-scoped variables for free.

## `Eval` vs `Exec`

The interpreter has two mutually recursive methods:

- `Eval(Expression)` computes and **returns** a value. Expressions do not
  produce any visible side effect beyond reading variables.
- `Exec(Statement)` **performs** an action and returns nothing. Statements can
  declare or update variables, print, loop, branch, or run a block of other
  statements.

Every statement is ultimately implemented in terms of `Eval` calls on its
sub-expressions plus some bookkeeping (scope changes, control flow).

## Runtime value model

Values are stored as plain `object` references, holding either:

- a `double` for numbers, or
- a `bool` for the result of comparisons and logical operators.

Operators cast the operands to the type they need. If the cast fails, the
interpreter throws a `[Runtime Error] Type mismatch: ...` exception. Division
by zero throws `[Runtime Error] Division by zero`.

## How to run

From the repository root:

```bash
cd Interpreter && dotnet run
```

## Expected output

The sample program in `Program.cs` sums the integers from 1 to 10 and prints
the result, then prints `1` if the sum equals 55:

```
55
1
```
