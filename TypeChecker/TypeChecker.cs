using SimpleLexer;
using SimpleParser;

namespace SimpleTypeChecker;

public class TypeChecker
{
    private List<Statement> _statements;
    private TypeEnvironment _env;
    private List<TypeError> _errors;
    private Dictionary<string, FunctionSignature> _functions;
    private SimpleType _currentReturnType;
    private bool _inFunction;

    public TypeChecker(List<Statement> statements)
    {
        _statements = statements;
        _env = new TypeEnvironment();
        _errors = new List<TypeError>();
        _functions = new Dictionary<string, FunctionSignature>();
        _currentReturnType = SimpleType.Unknown;
        _inFunction = false;
    }

    public List<TypeError> Check()
    {
        _env.EnterScope();
        HoistFunctions(_statements);
        for (int i = 0; i < _statements.Count; i++)
            CheckStatement(_statements[i]);
        _env.ExitScope();
        return _errors;
    }

    private void HoistFunctions(List<Statement> statements)
    {
        for (int i = 0; i < statements.Count; i++)
        {
            if (statements[i].GetType() == typeof(FunctionDeclaration))
            {
                FunctionDeclaration f = (FunctionDeclaration)statements[i];
                if (_functions.ContainsKey(f.Name))
                    continue;
                List<SimpleType> paramTypes = new List<SimpleType>();
                for (int j = 0; j < f.Parameters.Count; j++)
                    paramTypes.Add(SimpleType.Unknown);
                _functions[f.Name] = new FunctionSignature(f.Parameters, paramTypes, SimpleType.Unknown);
            }
        }
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
        else if (stmt.GetType() == typeof(FunctionDeclaration))
        {
            FunctionDeclaration s = (FunctionDeclaration)stmt;
            FunctionSignature sig = _functions[s.Name];
            _env.EnterScope();
            for (int i = 0; i < s.Parameters.Count; i++)
                _env.Declare(s.Parameters[i], SimpleType.Unknown);
            bool prevIn = _inFunction;
            SimpleType prevRet = _currentReturnType;
            _inFunction = true;
            _currentReturnType = SimpleType.Unknown;
            for (int i = 0; i < s.Body.Statements.Count; i++)
                CheckStatement(s.Body.Statements[i]);
            sig.ReturnType = _currentReturnType;
            _inFunction = prevIn;
            _currentReturnType = prevRet;
            _env.ExitScope();
        }
        else if (stmt.GetType() == typeof(ReturnStatement))
        {
            ReturnStatement s = (ReturnStatement)stmt;
            SimpleType t = SimpleType.Unknown;
            if (s.Value != null)
                t = InferExpression(s.Value);
            if (_currentReturnType == SimpleType.Unknown)
                _currentReturnType = t;
            else if (t != SimpleType.Unknown && t != _currentReturnType)
                _errors.Add(new TypeError("Inconsistent return types: " + _currentReturnType + " and " + t));
        }
    }

    private SimpleType InferExpression(Expression expr)
    {
        if (expr.GetType() == typeof(NumberExpression))
        {
            return SimpleType.Number;
        }
        else if (expr.GetType() == typeof(BooleanExpression))
        {
            return SimpleType.Bool;
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
        else if (expr.GetType() == typeof(CallExpression))
        {
            CallExpression e = (CallExpression)expr;
            List<SimpleType> argTypes = new List<SimpleType>();
            for (int i = 0; i < e.Arguments.Count; i++)
                argTypes.Add(InferExpression(e.Arguments[i]));
            if (!_functions.ContainsKey(e.Callee))
                return SimpleType.Unknown;
            FunctionSignature sig = _functions[e.Callee];
            if (sig.ParameterTypes.Count != argTypes.Count)
                return sig.ReturnType;
            for (int i = 0; i < argTypes.Count; i++)
            {
                if (sig.ParameterTypes[i] == SimpleType.Unknown)
                    sig.ParameterTypes[i] = argTypes[i];
                else if (argTypes[i] != SimpleType.Unknown && argTypes[i] != sig.ParameterTypes[i])
                    _errors.Add(new TypeError("Argument " + (i + 1) + " of '" + e.Callee + "' must be " + sig.ParameterTypes[i] + ", got " + argTypes[i]));
            }
            return sig.ReturnType;
        }
        return SimpleType.Unknown;
    }
}
