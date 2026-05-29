using SimpleLexer;
using SimpleParser;

namespace SimpleOptimizer;

// AST -> AST optimizer. Rebuilds the (immutable) tree bottom-up and repeats
// to a fixpoint, because constant folding enables algebraic simplification,
// which in turn enables dead-code elimination.
public class Optimizer
{
    private bool _changed;

    public List<Statement> Optimize(List<Statement> statements)
    {
        List<Statement> current = statements;
        for (int pass = 0; pass < 10; pass++)
        {
            _changed = false;
            current = OptStmtList(current);
            if (!_changed)
                break;
        }
        return current;
    }

    private List<Statement> OptStmtList(List<Statement> stmts)
    {
        List<Statement> result = new List<Statement>();
        bool terminated = false;
        for (int i = 0; i < stmts.Count; i++)
        {
            if (terminated)
            {
                _changed = true;
                continue;
            }
            Statement s = OptStmt(stmts[i]);
            if (s.GetType() == typeof(BlockStatement) && ((BlockStatement)s).Statements.Count == 0)
            {
                _changed = true;
                continue;
            }
            result.Add(s);
            if (s.GetType() == typeof(ReturnStatement))
                terminated = true;
        }
        return result;
    }

    private Statement OptStmt(Statement stmt)
    {
        if (stmt.GetType() == typeof(VarStatement))
        {
            VarStatement s = (VarStatement)stmt;
            Expression? init = s.Initializer != null ? OptExpr(s.Initializer) : null;
            return new VarStatement(s.Name, init);
        }
        if (stmt.GetType() == typeof(AssignStatement))
        {
            AssignStatement s = (AssignStatement)stmt;
            return new AssignStatement(s.Name, OptExpr(s.Value));
        }
        if (stmt.GetType() == typeof(PrintStatement))
        {
            PrintStatement s = (PrintStatement)stmt;
            return new PrintStatement(OptExpr(s.Expression));
        }
        if (stmt.GetType() == typeof(ExpressionStatement))
        {
            ExpressionStatement s = (ExpressionStatement)stmt;
            return new ExpressionStatement(OptExpr(s.Expression));
        }
        if (stmt.GetType() == typeof(ReturnStatement))
        {
            ReturnStatement s = (ReturnStatement)stmt;
            Expression? value = s.Value != null ? OptExpr(s.Value) : null;
            return new ReturnStatement(value);
        }
        if (stmt.GetType() == typeof(BlockStatement))
        {
            BlockStatement s = (BlockStatement)stmt;
            return new BlockStatement(OptStmtList(s.Statements));
        }
        if (stmt.GetType() == typeof(FunctionDeclaration))
        {
            FunctionDeclaration s = (FunctionDeclaration)stmt;
            BlockStatement body = new BlockStatement(OptStmtList(s.Body.Statements));
            return new FunctionDeclaration(s.Name, s.Parameters, body);
        }
        if (stmt.GetType() == typeof(IfStatement))
        {
            IfStatement s = (IfStatement)stmt;
            Expression cond = OptExpr(s.Condition);
            if (TryEvalConst(cond, out object v) && v is bool b)
            {
                _changed = true;
                if (b)
                    return OptStmt(s.Then);
                return s.Else != null ? OptStmt(s.Else) : new BlockStatement(new List<Statement>());
            }
            Statement thenBranch = OptStmt(s.Then);
            Statement? elseBranch = s.Else != null ? OptStmt(s.Else) : null;
            return new IfStatement(cond, thenBranch, elseBranch);
        }
        if (stmt.GetType() == typeof(WhileStatement))
        {
            WhileStatement s = (WhileStatement)stmt;
            Expression cond = OptExpr(s.Condition);
            if (TryEvalConst(cond, out object v) && v is bool b && !b)
            {
                // while (false) never runs.
                _changed = true;
                return new BlockStatement(new List<Statement>());
            }
            return new WhileStatement(cond, OptStmt(s.Body));
        }
        if (stmt.GetType() == typeof(IndexAssignStatement))
        {
            IndexAssignStatement s = (IndexAssignStatement)stmt;
            return new IndexAssignStatement(OptExpr(s.Array), OptExpr(s.Index), OptExpr(s.Value));
        }
        return stmt;
    }


    private Expression OptExpr(Expression expr)
    {
        if (expr.GetType() == typeof(NumberExpression)) return expr;
        if (expr.GetType() == typeof(BooleanExpression)) return expr;
        if (expr.GetType() == typeof(VariableExpression)) return expr;

        if (expr.GetType() == typeof(GroupExpression))
        {
            // Parentheses carry no meaning after parsing: unwrap.
            GroupExpression g = (GroupExpression)expr;
            _changed = true;
            return OptExpr(g.Inner);
        }
        if (expr.GetType() == typeof(CallExpression))
        {
            CallExpression c = (CallExpression)expr;
            List<Expression> args = new List<Expression>();
            for (int i = 0; i < c.Arguments.Count; i++)
                args.Add(OptExpr(c.Arguments[i]));
            return new CallExpression(c.Callee, args);
        }
        if (expr.GetType() == typeof(UnaryExpression))
        {
            UnaryExpression u = (UnaryExpression)expr;
            Expression operand = OptExpr(u.Operand);
            Expression cur = new UnaryExpression(u.Operator, operand);
            if (TryEvalConst(cur, out object v))
            {
                _changed = true;
                return MakeConst(v);
            }

            if (operand.GetType() == typeof(UnaryExpression))
            {
                UnaryExpression inner = (UnaryExpression)operand;
                if (u.Operator == inner.Operator &&
                    (u.Operator == TokenType.MINUS || u.Operator == TokenType.EXCL))
                {
                    _changed = true;
                    return inner.Operand;
                }
            }
            return cur;
        }
        if (expr.GetType() == typeof(BinaryExpression))
        {
            BinaryExpression be = (BinaryExpression)expr;
            Expression left = OptExpr(be.Left);
            Expression right = OptExpr(be.Right);
            Expression cur = new BinaryExpression(left, be.Operator, right);
            if (TryEvalConst(cur, out object v))
            {
                _changed = true;
                return MakeConst(v);
            }
            Expression? simplified = Simplify(be.Operator, left, right);
            if (simplified != null)
            {
                _changed = true;
                return simplified;
            }
            return cur;
        }
        if (expr.GetType() == typeof(ArrayLiteralExpression))
        {
            ArrayLiteralExpression a = (ArrayLiteralExpression)expr;
            List<Expression> elements = new List<Expression>();
            for (int i = 0; i < a.Elements.Count; i++)
                elements.Add(OptExpr(a.Elements[i]));
            return new ArrayLiteralExpression(elements);
        }
        if (expr.GetType() == typeof(ArrayIndexExpression))
        {
            ArrayIndexExpression a = (ArrayIndexExpression)expr;
            return new ArrayIndexExpression(OptExpr(a.Array), OptExpr(a.Index));
        }
        return expr;
    }

    private Expression? Simplify(TokenType op, Expression l, Expression r)
    {
        if (op == TokenType.PLUS)
        {
            if (IsNum(r, 0)) return l;
            if (IsNum(l, 0)) return r;
        }
        else if (op == TokenType.MINUS)
        {
            if (IsNum(r, 0)) return l;
        }
        else if (op == TokenType.STAR)
        {
            if (IsNum(r, 1)) return l;
            if (IsNum(l, 1)) return r;
            if (IsNum(r, 0) && IsPure(l)) return new NumberExpression(0);
            if (IsNum(l, 0) && IsPure(r)) return new NumberExpression(0);
        }
        else if (op == TokenType.SLASH)
        {
            if (IsNum(r, 1)) return l;
        }
        else if (op == TokenType.AND)
        {
            if (IsBool(r, out bool aR)) { if (aR) return l; if (IsPure(l)) return new BooleanExpression(false); }
            if (IsBool(l, out bool aL)) { if (aL) return r; if (IsPure(r)) return new BooleanExpression(false); }
        }
        else if (op == TokenType.OR)
        {
            if (IsBool(r, out bool oR)) { if (!oR) return l; if (IsPure(l)) return new BooleanExpression(true); }
            if (IsBool(l, out bool oL)) { if (!oL) return r; if (IsPure(r)) return new BooleanExpression(true); }
        }
        return null;
    }

    private bool TryEvalConst(Expression expr, out object value)
    {
        value = null!;
        if (expr.GetType() == typeof(NumberExpression)) { value = ((NumberExpression)expr).Value; return true; }
        if (expr.GetType() == typeof(BooleanExpression)) { value = ((BooleanExpression)expr).Value; return true; }
        if (expr.GetType() == typeof(GroupExpression)) return TryEvalConst(((GroupExpression)expr).Inner, out value);
        if (expr.GetType() == typeof(UnaryExpression))
        {
            UnaryExpression u = (UnaryExpression)expr;
            if (!TryEvalConst(u.Operand, out object o)) return false;
            if (u.Operator == TokenType.MINUS && o is double d) { value = -d; return true; }
            if (u.Operator == TokenType.EXCL && o is bool b) { value = !b; return true; }
            return false;
        }
        if (expr.GetType() == typeof(BinaryExpression))
        {
            BinaryExpression be = (BinaryExpression)expr;
            if (!TryEvalConst(be.Left, out object lo)) return false;
            if (!TryEvalConst(be.Right, out object ro)) return false;
            TokenType op = be.Operator;
            if (op == TokenType.PLUS || op == TokenType.MINUS || op == TokenType.STAR || op == TokenType.SLASH)
            {
                if (lo is double l && ro is double r)
                {
                    if (op == TokenType.PLUS) { value = l + r; return true; }
                    if (op == TokenType.MINUS) { value = l - r; return true; }
                    if (op == TokenType.STAR) { value = l * r; return true; }
                    if (r == 0.0) return false; // preserve runtime division-by-zero error
                    value = l / r; return true;
                }
                return false;
            }
            if (op == TokenType.LT || op == TokenType.GT || op == TokenType.LTEQ || op == TokenType.GTEQ)
            {
                if (lo is double l && ro is double r)
                {
                    if (op == TokenType.LT) { value = l < r; return true; }
                    if (op == TokenType.GT) { value = l > r; return true; }
                    if (op == TokenType.LTEQ) { value = l <= r; return true; }
                    value = l >= r; return true;
                }
                return false;
            }
            if (op == TokenType.EQEQ) { value = lo.Equals(ro); return true; }
            if (op == TokenType.NEQ) { value = !lo.Equals(ro); return true; }
            if (op == TokenType.AND) { if (lo is bool l && ro is bool r) { value = l && r; return true; } return false; }
            if (op == TokenType.OR) { if (lo is bool l && ro is bool r) { value = l || r; return true; } return false; }
            return false;
        }
        return false;
    }

    private bool IsPure(Expression expr)
    {
        if (expr.GetType() == typeof(CallExpression)) return false;
        if (expr.GetType() == typeof(BinaryExpression))
        {
            BinaryExpression e = (BinaryExpression)expr;
            return IsPure(e.Left) && IsPure(e.Right);
        }
        if (expr.GetType() == typeof(UnaryExpression)) return IsPure(((UnaryExpression)expr).Operand);
        if (expr.GetType() == typeof(GroupExpression)) return IsPure(((GroupExpression)expr).Inner);
        if (expr.GetType() == typeof(ArrayIndexExpression))
        {
            ArrayIndexExpression e = (ArrayIndexExpression)expr;
            return IsPure(e.Array) && IsPure(e.Index);
        }
        if (expr.GetType() == typeof(ArrayLiteralExpression))
        {
            ArrayLiteralExpression e = (ArrayLiteralExpression)expr;
            for (int i = 0; i < e.Elements.Count; i++)
                if (!IsPure(e.Elements[i])) return false;
            return true;
        }
        return true;
    }

    private Expression MakeConst(object v)
    {
        if (v is bool b) return new BooleanExpression(b);
        return new NumberExpression((double)v);
    }

    private bool IsNum(Expression e, double val)
    {
        return e.GetType() == typeof(NumberExpression) && ((NumberExpression)e).Value == val;
    }

    private bool IsBool(Expression e, out bool val)
    {
        if (e.GetType() == typeof(BooleanExpression)) { val = ((BooleanExpression)e).Value; return true; }
        val = false;
        return false;
    }
}
