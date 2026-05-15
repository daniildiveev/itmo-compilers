using SimpleLexer;
using SimpleParser;

namespace SimpleInterpreter;

public class Interpreter
{
    private Environment _env = new Environment(null);

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
            Console.WriteLine(value);
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
        throw new Exception("[Runtime Error] Unknown statement type");
    }

    public object Eval(Expression expr)
    {
        if (expr.GetType() == typeof(NumberExpression))
        {
            NumberExpression ne = (NumberExpression)expr;
            return ne.Value;
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
        throw new Exception("[Runtime Error] Unknown expression type");
    }
}
