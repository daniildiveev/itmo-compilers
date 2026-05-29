using SimpleLexer;
using SimpleParser;

namespace SimpleInterpreter;

public class FunctionValue
{
    public string Name { get; }
    public List<string> Parameters { get; }
    public BlockStatement Body { get; }
    public Environment Closure { get; }

    public FunctionValue(string name, List<string> parameters, BlockStatement body, Environment closure)
    {
        Name = name;
        Parameters = parameters;
        Body = body;
        Closure = closure;
    }
}

public class ReturnException : Exception
{
    public object Value { get; }
    public ReturnException(object value) { Value = value; }
}

public class Interpreter
{
    private Environment _env = new Environment(null);
    private Dictionary<string, FunctionValue> _functions = new Dictionary<string, FunctionValue>();

    public void Interpret(List<Statement> statements)
    {
        for (int i = 0; i < statements.Count; i++)
        {
            Exec(statements[i]);
        }
    }

    public void Exec(Statement stmt)
    {
        if (stmt.GetType() == typeof(VarStatement))
        {
            VarStatement vs = (VarStatement)stmt;
            object value;
            if (vs.Initializer != null)
                value = Eval(vs.Initializer);
            else
                value = 0.0;
            _env.Define(vs.Name, value);
            return;
        }
        if (stmt.GetType() == typeof(AssignStatement))
        {
            AssignStatement asg = (AssignStatement)stmt;
            object value = Eval(asg.Value);
            _env.Set(asg.Name, value);
            return;
        }
        if (stmt.GetType() == typeof(PrintStatement))
        {
            PrintStatement ps = (PrintStatement)stmt;
            object value = Eval(ps.Expression);
            Console.WriteLine(Stringify(value));
            return;
        }
        if (stmt.GetType() == typeof(BlockStatement))
        {
            BlockStatement bs = (BlockStatement)stmt;
            Environment oldEnv = _env;
            _env = new Environment(oldEnv);
            try
            {
                for (int i = 0; i < bs.Statements.Count; i++)
                {
                    Exec(bs.Statements[i]);
                }
            }
            finally
            {
                _env = oldEnv;
            }
            return;
        }
        if (stmt.GetType() == typeof(IfStatement))
        {
            IfStatement ifs = (IfStatement)stmt;
            object cond = Eval(ifs.Condition);
            bool condBool;
            try
            {
                condBool = (bool)cond;
            }
            catch
            {
                throw new Exception("[Runtime Error] Type mismatch: expected bool");
            }
            if (condBool)
                Exec(ifs.Then);
            else if (ifs.Else != null)
                Exec(ifs.Else);
            return;
        }
        if (stmt.GetType() == typeof(WhileStatement))
        {
            WhileStatement ws = (WhileStatement)stmt;
            while (true)
            {
                object cond = Eval(ws.Condition);
                bool condBool;
                try
                {
                    condBool = (bool)cond;
                }
                catch
                {
                    throw new Exception("[Runtime Error] Type mismatch: expected bool");
                }
                if (!condBool) break;
                Exec(ws.Body);
            }
            return;
        }
        if (stmt.GetType() == typeof(ExpressionStatement))
        {
            ExpressionStatement es = (ExpressionStatement)stmt;
            Eval(es.Expression);
            return;
        }
        if (stmt.GetType() == typeof(FunctionDeclaration))
        {
            FunctionDeclaration fd = (FunctionDeclaration)stmt;
            _functions[fd.Name] = new FunctionValue(fd.Name, fd.Parameters, fd.Body, _env);
            return;
        }
        if (stmt.GetType() == typeof(ReturnStatement))
        {
            ReturnStatement rs = (ReturnStatement)stmt;
            object value = 0.0;
            if (rs.Value != null)
                value = Eval(rs.Value);
            throw new ReturnException(value);
        }
        if (stmt.GetType() == typeof(IndexAssignStatement))
        {
            IndexAssignStatement ias = (IndexAssignStatement)stmt;
            object arrObj = Eval(ias.Array);
            object idxObj = Eval(ias.Index);
            object value = Eval(ias.Value);
            List<object> list = AsArray(arrObj);
            int idx = AsIndex(idxObj);
            if (idx < 0 || idx >= list.Count)
                throw new Exception("[Runtime Error] Array index " + idx + " out of bounds (length " + list.Count + ")");
            list[idx] = value;
            return;
        }
        throw new Exception("[Runtime Error] Unknown statement type");
    }

    public object Eval(Expression expr)
    {
        if (expr.GetType() == typeof(NumberExpression))
        {
            NumberExpression ne = (NumberExpression)expr;
            return ne.Value;
        }
        if (expr.GetType() == typeof(BooleanExpression))
        {
            BooleanExpression boolExpr = (BooleanExpression)expr;
            return boolExpr.Value;
        }
        if (expr.GetType() == typeof(VariableExpression))
        {
            VariableExpression ve = (VariableExpression)expr;
            return _env.Get(ve.Name);
        }
        if (expr.GetType() == typeof(GroupExpression))
        {
            GroupExpression ge = (GroupExpression)expr;
            return Eval(ge.Inner);
        }
        if (expr.GetType() == typeof(UnaryExpression))
        {
            UnaryExpression ue = (UnaryExpression)expr;
            object operand = Eval(ue.Operand);
            if (ue.Operator == TokenType.MINUS)
            {
                double d;
                try
                {
                    d = (double)operand;
                }
                catch
                {
                    throw new Exception("[Runtime Error] Type mismatch: expected double");
                }
                return -d;
            }
            if (ue.Operator == TokenType.EXCL)
            {
                bool b;
                try
                {
                    b = (bool)operand;
                }
                catch
                {
                    throw new Exception("[Runtime Error] Type mismatch: expected bool");
                }
                return !b;
            }
            throw new Exception("[Runtime Error] Unknown unary operator");
        }
        if (expr.GetType() == typeof(BinaryExpression))
        {
            BinaryExpression be = (BinaryExpression)expr;
            object left = Eval(be.Left);
            object right = Eval(be.Right);
            TokenType op = be.Operator;

            if (op == TokenType.PLUS || op == TokenType.MINUS || op == TokenType.STAR || op == TokenType.SLASH)
            {
                double l;
                double r;
                try
                {
                    l = (double)left;
                }
                catch
                {
                    throw new Exception("[Runtime Error] Type mismatch: expected double");
                }
                try
                {
                    r = (double)right;
                }
                catch
                {
                    throw new Exception("[Runtime Error] Type mismatch: expected double");
                }
                if (op == TokenType.PLUS) return l + r;
                if (op == TokenType.MINUS) return l - r;
                if (op == TokenType.STAR) return l * r;
                if (r == 0.0)
                    throw new Exception("[Runtime Error] Division by zero");
                return l / r;
            }
            if (op == TokenType.LT || op == TokenType.GT || op == TokenType.LTEQ || op == TokenType.GTEQ)
            {
                double l;
                double r;
                try
                {
                    l = (double)left;
                }
                catch
                {
                    throw new Exception("[Runtime Error] Type mismatch: expected double");
                }
                try
                {
                    r = (double)right;
                }
                catch
                {
                    throw new Exception("[Runtime Error] Type mismatch: expected double");
                }
                if (op == TokenType.LT) return l < r;
                if (op == TokenType.GT) return l > r;
                if (op == TokenType.LTEQ) return l <= r;
                return l >= r;
            }
            if (op == TokenType.EQEQ)
            {
                return left.Equals(right);
            }
            if (op == TokenType.NEQ)
            {
                return !left.Equals(right);
            }
            if (op == TokenType.AND)
            {
                bool l;
                bool r;
                try
                {
                    l = (bool)left;
                }
                catch
                {
                    throw new Exception("[Runtime Error] Type mismatch: expected bool");
                }
                try
                {
                    r = (bool)right;
                }
                catch
                {
                    throw new Exception("[Runtime Error] Type mismatch: expected bool");
                }
                return l && r;
            }
            if (op == TokenType.OR)
            {
                bool l;
                bool r;
                try
                {
                    l = (bool)left;
                }
                catch
                {
                    throw new Exception("[Runtime Error] Type mismatch: expected bool");
                }
                try
                {
                    r = (bool)right;
                }
                catch
                {
                    throw new Exception("[Runtime Error] Type mismatch: expected bool");
                }
                return l || r;
            }
            throw new Exception("[Runtime Error] Unknown binary operator");
        }
        if (expr.GetType() == typeof(ArrayLiteralExpression))
        {
            ArrayLiteralExpression ale = (ArrayLiteralExpression)expr;
            List<object> list = new List<object>();
            for (int i = 0; i < ale.Elements.Count; i++)
                list.Add(Eval(ale.Elements[i]));
            return list;
        }
        if (expr.GetType() == typeof(ArrayIndexExpression))
        {
            ArrayIndexExpression aie = (ArrayIndexExpression)expr;
            object arrObj = Eval(aie.Array);
            object idxObj = Eval(aie.Index);
            List<object> list = AsArray(arrObj);
            int idx = AsIndex(idxObj);
            if (idx < 0 || idx >= list.Count)
                throw new Exception("[Runtime Error] Array index " + idx + " out of bounds (length " + list.Count + ")");
            return list[idx];
        }
        if (expr.GetType() == typeof(CallExpression))
        {
            CallExpression ce = (CallExpression)expr;
            if (ce.Callee == "length" && !_functions.ContainsKey("length"))
            {
                if (ce.Arguments.Count != 1)
                    throw new Exception("[Runtime Error] Function 'length' expects 1 arguments, got " + ce.Arguments.Count);
                object arrObj = Eval(ce.Arguments[0]);
                List<object> list = AsArray(arrObj);
                return (double)list.Count;
            }
            if (!_functions.ContainsKey(ce.Callee))
                throw new Exception("[Runtime Error] Undefined function '" + ce.Callee + "'");
            FunctionValue fn = _functions[ce.Callee];
            if (fn.Parameters.Count != ce.Arguments.Count)
                throw new Exception("[Runtime Error] Function '" + ce.Callee + "' expects " + fn.Parameters.Count + " arguments, got " + ce.Arguments.Count);
            List<object> args = new List<object>();
            for (int i = 0; i < ce.Arguments.Count; i++)
                args.Add(Eval(ce.Arguments[i]));
            Environment callEnv = new Environment(fn.Closure);
            for (int i = 0; i < fn.Parameters.Count; i++)
                callEnv.Define(fn.Parameters[i], args[i]);
            Environment oldEnv = _env;
            _env = callEnv;
            object result = 0.0;
            try
            {
                for (int i = 0; i < fn.Body.Statements.Count; i++)
                    Exec(fn.Body.Statements[i]);
            }
            catch (ReturnException re)
            {
                result = re.Value;
            }
            finally
            {
                _env = oldEnv;
            }
            return result;
        }
        throw new Exception("[Runtime Error] Unknown expression type");
    }

    private List<object> AsArray(object o)
    {
        if (o is List<object> list) return list;
        throw new Exception("[Runtime Error] Type mismatch: expected array");
    }

    private int AsIndex(object o)
    {
        if (o is double d) return (int)d;
        throw new Exception("[Runtime Error] Type mismatch: expected number index");
    }

    private string Stringify(object value)
    {
        if (value is List<object> list)
        {
            List<string> parts = new List<string>();
            for (int i = 0; i < list.Count; i++)
                parts.Add(Stringify(list[i]));
            return "[" + string.Join(", ", parts) + "]";
        }
        return value.ToString();
    }
}
