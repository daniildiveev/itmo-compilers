using SimpleLexer;

namespace SimpleParser;

public class AstPrinter
{
    public void Print(List<Statement> statements)
    {
        for (int i = 0; i < statements.Count; i++)
            PrintStatement(statements[i], "", i == statements.Count - 1);
    }

    private static string ChildPrefix(string parentPrefix, bool parentIsLast)
    {
        return parentPrefix + (parentIsLast ? "    " : "|   ");
    }

    private void PrintLine(string prefix, bool isLast, string text)
    {
        Console.WriteLine(prefix + (isLast ? "`-- " : "|-- ") + text);
    }

    private void PrintStatement(Statement stmt, string prefix, bool isLast)
    {
        if (stmt.GetType() == typeof(VarStatement))
        {
            VarStatement s = (VarStatement)stmt;
            PrintLine(prefix, isLast, "VarDecl: " + s.Name);
            if (s.Initializer != null)
            {
                string next = ChildPrefix(prefix, isLast);
                PrintExpression(s.Initializer, next, true);
            }
        }
        else if (stmt.GetType() == typeof(AssignStatement))
        {
            AssignStatement s = (AssignStatement)stmt;
            PrintLine(prefix, isLast, "Assign: " + s.Name);
            string next = ChildPrefix(prefix, isLast);
            PrintExpression(s.Value, next, true);
        }
        else if (stmt.GetType() == typeof(PrintStatement))
        {
            PrintStatement ps = (PrintStatement)stmt;
            PrintLine(prefix, isLast, "Print");
            string next = ChildPrefix(prefix, isLast);
            PrintExpression(ps.Expression, next, true);
        }
        else if (stmt.GetType() == typeof(IfStatement))
        {
            IfStatement s = (IfStatement)stmt;
            PrintLine(prefix, isLast, "If");
            string next = ChildPrefix(prefix, isLast);
            PrintExpression(s.Condition, next, false);
            if (s.Else != null)
            {
                PrintStatement(s.Then, next, false);
                PrintStatement(s.Else, next, true);
            }
            else
            {
                PrintStatement(s.Then, next, true);
            }
        }
        else if (stmt.GetType() == typeof(WhileStatement))
        {
            WhileStatement s = (WhileStatement)stmt;
            PrintLine(prefix, isLast, "While");
            string next = ChildPrefix(prefix, isLast);
            PrintExpression(s.Condition, next, false);
            PrintStatement(s.Body, next, true);
        }
        else if (stmt.GetType() == typeof(BlockStatement))
        {
            BlockStatement s = (BlockStatement)stmt;
            PrintLine(prefix, isLast, "Block");
            string next = ChildPrefix(prefix, isLast);
            for (int i = 0; i < s.Statements.Count; i++)
                PrintStatement(s.Statements[i], next, i == s.Statements.Count - 1);
        }
        else if (stmt.GetType() == typeof(ExpressionStatement))
        {
            ExpressionStatement s = (ExpressionStatement)stmt;
            PrintExpression(s.Expression, prefix, isLast);
        }
        else if (stmt.GetType() == typeof(FunctionDeclaration))
        {
            FunctionDeclaration s = (FunctionDeclaration)stmt;
            PrintLine(prefix, isLast, "FunDecl: " + s.Name + "(" + string.Join(", ", s.Parameters) + ")");
            string next = ChildPrefix(prefix, isLast);
            PrintStatement(s.Body, next, true);
        }
        else if (stmt.GetType() == typeof(ReturnStatement))
        {
            ReturnStatement s = (ReturnStatement)stmt;
            PrintLine(prefix, isLast, "Return");
            if (s.Value != null)
            {
                string next = ChildPrefix(prefix, isLast);
                PrintExpression(s.Value, next, true);
            }
        }
    }

    private void PrintExpression(Expression expr, string prefix, bool isLast)
    {
        if (expr.GetType() == typeof(NumberExpression))
        {
            NumberExpression e = (NumberExpression)expr;
            PrintLine(prefix, isLast, "Number: " + e.Value);
        }
        else if (expr.GetType() == typeof(BooleanExpression))
        {
            BooleanExpression e = (BooleanExpression)expr;
            PrintLine(prefix, isLast, "Bool: " + (e.Value ? "true" : "false"));
        }
        else if (expr.GetType() == typeof(VariableExpression))
        {
            VariableExpression e = (VariableExpression)expr;
            PrintLine(prefix, isLast, "Var: " + e.Name);
        }
        else if (expr.GetType() == typeof(BinaryExpression))
        {
            BinaryExpression e = (BinaryExpression)expr;
            PrintLine(prefix, isLast, "Binary: " + e.Operator);
            string next = ChildPrefix(prefix, isLast);
            PrintExpression(e.Left, next, false);
            PrintExpression(e.Right, next, true);
        }
        else if (expr.GetType() == typeof(UnaryExpression))
        {
            UnaryExpression e = (UnaryExpression)expr;
            PrintLine(prefix, isLast, "Unary: " + e.Operator);
            string next = ChildPrefix(prefix, isLast);
            PrintExpression(e.Operand, next, true);
        }
        else if (expr.GetType() == typeof(GroupExpression))
        {
            GroupExpression e = (GroupExpression)expr;
            PrintLine(prefix, isLast, "Group");
            string next = ChildPrefix(prefix, isLast);
            PrintExpression(e.Inner, next, true);
        }
        else if (expr.GetType() == typeof(CallExpression))
        {
            CallExpression e = (CallExpression)expr;
            PrintLine(prefix, isLast, "Call: " + e.Callee);
            string next = ChildPrefix(prefix, isLast);
            for (int i = 0; i < e.Arguments.Count; i++)
                PrintExpression(e.Arguments[i], next, i == e.Arguments.Count - 1);
        }
    }
}
