using SimpleLexer;

namespace SimpleParser;

public abstract class Expression
{
}

public abstract class Statement
{
}

public class NumberExpression : Expression
{
    public double Value { get; }

    public NumberExpression(double value)
    {
        Value = value;
    }
}

public class VariableExpression : Expression
{
    public string Name { get; }

    public VariableExpression(string name)
    {
        Name = name;
    }
}

public class BinaryExpression : Expression
{
    public Expression Left { get; }
    public TokenType Operator { get; }
    public Expression Right { get; }

    public BinaryExpression(Expression left, TokenType op, Expression right)
    {
        Left = left;
        Operator = op;
        Right = right;
    }
}

public class UnaryExpression : Expression
{
    public TokenType Operator { get; }
    public Expression Operand { get; }

    public UnaryExpression(TokenType op, Expression operand)
    {
        Operator = op;
        Operand = operand;
    }
}

public class GroupExpression : Expression
{
    public Expression Inner { get; }

    public GroupExpression(Expression inner)
    {
        Inner = inner;
    }
}

public class VarStatement : Statement
{
    public string Name { get; }
    public Expression? Initializer { get; }

    public VarStatement(string name, Expression? initializer)
    {
        Name = name;
        Initializer = initializer;
    }
}

public class AssignStatement : Statement
{
    public string Name { get; }
    public Expression Value { get; }

    public AssignStatement(string name, Expression value)
    {
        Name = name;
        Value = value;
    }
}

public class PrintStatement : Statement
{
    public Expression Expression { get; }

    public PrintStatement(Expression expression)
    {
        Expression = expression;
    }
}

public class BlockStatement : Statement
{
    public List<Statement> Statements { get; }

    public BlockStatement(List<Statement> statements)
    {
        Statements = statements;
    }
}

public class IfStatement : Statement
{
    public Expression Condition { get; }
    public Statement Then { get; }
    public Statement? Else { get; }

    public IfStatement(Expression condition, Statement thenBranch, Statement? elseBranch)
    {
        Condition = condition;
        Then = thenBranch;
        Else = elseBranch;
    }
}

public class WhileStatement : Statement
{
    public Expression Condition { get; }
    public Statement Body { get; }

    public WhileStatement(Expression condition, Statement body)
    {
        Condition = condition;
        Body = body;
    }
}

public class ExpressionStatement : Statement
{
    public Expression Expression { get; }

    public ExpressionStatement(Expression expression)
    {
        Expression = expression;
    }
}

public class FunctionDeclaration : Statement
{
    public string Name { get; }
    public List<string> Parameters { get; }
    public BlockStatement Body { get; }

    public FunctionDeclaration(string name, List<string> parameters, BlockStatement body)
    {
        Name = name;
        Parameters = parameters;
        Body = body;
    }
}

public class ReturnStatement : Statement
{
    public Expression? Value { get; }

    public ReturnStatement(Expression? value)
    {
        Value = value;
    }
}

public class CallExpression : Expression
{
    public string Callee { get; }
    public List<Expression> Arguments { get; }

    public CallExpression(string callee, List<Expression> arguments)
    {
        Callee = callee;
        Arguments = arguments;
    }
}
