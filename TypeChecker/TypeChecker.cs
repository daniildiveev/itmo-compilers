using SimpleLexer;
using SimpleParser;

namespace SimpleTypeChecker;

public class TypeChecker
{
    private List<Statement> _statements;
    private TypeEnvironment _env;
    private List<TypeError> _errors;

    public TypeChecker(List<Statement> statements)
    {
        _statements = statements;
        _env = new TypeEnvironment();
        _errors = new List<TypeError>();
    }

    public List<TypeError> Check()
    {
        _env.EnterScope();
        for (int i = 0; i < _statements.Count; i++)
            CheckStatement(_statements[i]);
        _env.ExitScope();
        return _errors;
    }

    private void CheckStatement(Statement stmt)
    {
        if (stmt.GetType() == typeof(VarStatement))
        {
            VarStatement s = (VarStatement)stmt;
            SimpleType type = SimpleType.Unknown;
            if (s.Initializer != null)
                type = InferExpression(s.Initializer);
            _env.Declare(s.Name, type);
        }
        else if (stmt.GetType() == typeof(AssignStatement))
        {
            AssignStatement s = (AssignStatement)stmt;
            SimpleType declared = _env.Lookup(s.Name);
            SimpleType inferred = InferExpression(s.Value);
            if (declared != SimpleType.Unknown && inferred != SimpleType.Unknown && declared != inferred)
                _errors.Add(new TypeError("Cannot assign " + inferred + " value to variable '" + s.Name + "' declared as " + declared));
        }
        else if (stmt.GetType() == typeof(PrintStatement))
        {
            PrintStatement s = (PrintStatement)stmt;
            InferExpression(s.Expression);
        }
        else if (stmt.GetType() == typeof(BlockStatement))
        {
            BlockStatement s = (BlockStatement)stmt;
            _env.EnterScope();
            for (int i = 0; i < s.Statements.Count; i++)
                CheckStatement(s.Statements[i]);
            _env.ExitScope();
        }
        else if (stmt.GetType() == typeof(IfStatement))
        {
            IfStatement s = (IfStatement)stmt;
            SimpleType condType = InferExpression(s.Condition);
            if (condType != SimpleType.Bool && condType != SimpleType.Unknown)
                _errors.Add(new TypeError("If condition must be Bool, got " + condType));
            CheckStatement(s.Then);
            if (s.Else != null)
                CheckStatement(s.Else);
        }
        else if (stmt.GetType() == typeof(WhileStatement))
        {
            WhileStatement s = (WhileStatement)stmt;
            SimpleType condType = InferExpression(s.Condition);
            if (condType != SimpleType.Bool && condType != SimpleType.Unknown)
                _errors.Add(new TypeError("While condition must be Bool, got " + condType));
            CheckStatement(s.Body);
        }
        else if (stmt.GetType() == typeof(ExpressionStatement))
        {
            ExpressionStatement s = (ExpressionStatement)stmt;
            InferExpression(s.Expression);
        }
    }

    private SimpleType InferExpression(Expression expr)
    {
        if (expr.GetType() == typeof(NumberExpression))
        {
            return SimpleType.Number;
        }
        else if (expr.GetType() == typeof(VariableExpression))
        {
            VariableExpression e = (VariableExpression)expr;
            return _env.Lookup(e.Name);
        }
        else if (expr.GetType() == typeof(BinaryExpression))
        {
            BinaryExpression e = (BinaryExpression)expr;
            SimpleType left = InferExpression(e.Left);
            SimpleType right = InferExpression(e.Right);

            if (e.Operator == TokenType.PLUS || e.Operator == TokenType.MINUS ||
                e.Operator == TokenType.STAR || e.Operator == TokenType.SLASH)
            {
                if (left != SimpleType.Unknown && left != SimpleType.Number)
                    _errors.Add(new TypeError("Left operand of '" + e.Operator + "' must be Number, got " + left));
                if (right != SimpleType.Unknown && right != SimpleType.Number)
                    _errors.Add(new TypeError("Right operand of '" + e.Operator + "' must be Number, got " + right));
                return SimpleType.Number;
            }
            else if (e.Operator == TokenType.LT || e.Operator == TokenType.GT ||
                     e.Operator == TokenType.LTEQ || e.Operator == TokenType.GTEQ)
            {
                if (left != SimpleType.Unknown && left != SimpleType.Number)
                    _errors.Add(new TypeError("Left operand of '" + e.Operator + "' must be Number, got " + left));
                if (right != SimpleType.Unknown && right != SimpleType.Number)
                    _errors.Add(new TypeError("Right operand of '" + e.Operator + "' must be Number, got " + right));
                return SimpleType.Bool;
            }
            else if (e.Operator == TokenType.EQEQ || e.Operator == TokenType.NEQ)
            {
                if (left != SimpleType.Unknown && right != SimpleType.Unknown && left != right)
                    _errors.Add(new TypeError("Operands of '" + e.Operator + "' must have the same type, got " + left + " and " + right));
                return SimpleType.Bool;
            }
            else if (e.Operator == TokenType.AND || e.Operator == TokenType.OR)
            {
                if (left != SimpleType.Unknown && left != SimpleType.Bool)
                    _errors.Add(new TypeError("Left operand of '" + e.Operator + "' must be Bool, got " + left));
                if (right != SimpleType.Unknown && right != SimpleType.Bool)
                    _errors.Add(new TypeError("Right operand of '" + e.Operator + "' must be Bool, got " + right));
                return SimpleType.Bool;
            }
            return SimpleType.Unknown;
        }
        
        else if (expr.GetType() == typeof(UnaryExpression))
        {
            UnaryExpression e = (UnaryExpression)expr;
            SimpleType operand = InferExpression(e.Operand);
            if (e.Operator == TokenType.MINUS)
            {
                if (operand != SimpleType.Unknown && operand != SimpleType.Number)
                    _errors.Add(new TypeError("Unary '-' requires Number, got " + operand));
                return SimpleType.Number;
            }
            else if (e.Operator == TokenType.EXCL)
            {
                if (operand != SimpleType.Unknown && operand != SimpleType.Bool)
                    _errors.Add(new TypeError("Unary '!' requires Bool, got " + operand));
                return SimpleType.Bool;
            }
            return SimpleType.Unknown;
        }
        else if (expr.GetType() == typeof(GroupExpression))
        {
            GroupExpression e = (GroupExpression)expr;
            return InferExpression(e.Inner);
        }
        return SimpleType.Unknown;
    }
}
