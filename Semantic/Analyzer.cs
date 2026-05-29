using SimpleLexer;
using SimpleParser;

namespace SimpleSemantic;

public class Analyzer
{
    private List<Statement> _statements;
    private SymbolTable _table;
    private List<SemanticError> _errors;
    private int _functionDepth;

    public Analyzer(List<Statement> statements)
    {
        _statements = statements;
        _table = new SymbolTable();
        _errors = new List<SemanticError>();
    }

    public List<SemanticError> Analyze()
    {
        _table.EnterScope();
        HoistFunctions(_statements);
        for (int i = 0; i < _statements.Count; i++)
            AnalyzeStatement(_statements[i]);
        _table.ExitScope();
        return _errors;
    }

    private void HoistFunctions(List<Statement> statements)
    {
        for (int i = 0; i < statements.Count; i++)
        {
            if (statements[i].GetType() == typeof(FunctionDeclaration))
            {
                FunctionDeclaration f = (FunctionDeclaration)statements[i];
                if (_table.IsFunctionDeclared(f.Name))
                {
                    _errors.Add(new SemanticError("Function '" + f.Name + "' is already declared"));
                    continue;
                }
                _table.DeclareFunction(f.Name, f.Parameters.Count);
            }
        }
    }

    private void AnalyzeStatement(Statement stmt)
    {
        if (stmt.GetType() == typeof(VarStatement))
        {
            VarStatement s = (VarStatement)stmt;
            if (_table.IsDeclaredInCurrentScope(s.Name))
                _errors.Add(new SemanticError("Variable '" + s.Name + "' is already declared in this scope"));
            _table.Declare(s.Name);
            if (s.Initializer != null)
                AnalyzeExpression(s.Initializer);
        }
        else if (stmt.GetType() == typeof(AssignStatement))
        {
            AssignStatement s = (AssignStatement)stmt;
            if (!_table.IsDeclared(s.Name))
                _errors.Add(new SemanticError("Variable '" + s.Name + "' is assigned but was never declared"));
            AnalyzeExpression(s.Value);
        }
        else if (stmt.GetType() == typeof(PrintStatement))
        {
            PrintStatement s = (PrintStatement)stmt;
            AnalyzeExpression(s.Expression);
        }
        else if (stmt.GetType() == typeof(BlockStatement))
        {
            BlockStatement s = (BlockStatement)stmt;
            _table.EnterScope();
            for (int i = 0; i < s.Statements.Count; i++)
                AnalyzeStatement(s.Statements[i]);
            _table.ExitScope();
        }
        else if (stmt.GetType() == typeof(IfStatement))
        {
            IfStatement s = (IfStatement)stmt;
            AnalyzeExpression(s.Condition);
            AnalyzeStatement(s.Then);
            if (s.Else != null)
                AnalyzeStatement(s.Else);
        }
        else if (stmt.GetType() == typeof(WhileStatement))
        {
            WhileStatement s = (WhileStatement)stmt;
            AnalyzeExpression(s.Condition);
            AnalyzeStatement(s.Body);
        }
        else if (stmt.GetType() == typeof(ExpressionStatement))
        {
            ExpressionStatement s = (ExpressionStatement)stmt;
            AnalyzeExpression(s.Expression);
        }
        else if (stmt.GetType() == typeof(FunctionDeclaration))
        {
            FunctionDeclaration s = (FunctionDeclaration)stmt;
            HashSet<string> seen = new HashSet<string>();
            for (int i = 0; i < s.Parameters.Count; i++)
            {
                if (!seen.Add(s.Parameters[i]))
                    _errors.Add(new SemanticError("Duplicate parameter '" + s.Parameters[i] + "' in function '" + s.Name + "'"));
            }
            _table.EnterScope();
            for (int i = 0; i < s.Parameters.Count; i++)
                _table.Declare(s.Parameters[i]);
            _functionDepth++;
            for (int i = 0; i < s.Body.Statements.Count; i++)
                AnalyzeStatement(s.Body.Statements[i]);
            _functionDepth--;
            _table.ExitScope();
        }
        else if (stmt.GetType() == typeof(ReturnStatement))
        {
            ReturnStatement s = (ReturnStatement)stmt;
            if (_functionDepth == 0)
                _errors.Add(new SemanticError("'return' outside of function"));
            if (s.Value != null)
                AnalyzeExpression(s.Value);
        }
    }

    private void AnalyzeExpression(Expression expr)
    {
        if (expr.GetType() == typeof(NumberExpression))
        {
        }
        else if (expr.GetType() == typeof(BooleanExpression))
        {
        }
        else if (expr.GetType() == typeof(VariableExpression))
        {
            VariableExpression e = (VariableExpression)expr;
            if (!_table.IsDeclared(e.Name))
                _errors.Add(new SemanticError("Variable '" + e.Name + "' is used but was never declared"));
        }
        else if (expr.GetType() == typeof(BinaryExpression))
        {
            BinaryExpression e = (BinaryExpression)expr;
            AnalyzeExpression(e.Left);
            if (e.Operator == TokenType.SLASH && e.Right.GetType() == typeof(NumberExpression))
            {
                NumberExpression divisor = (NumberExpression)e.Right;
                if (divisor.Value == 0)
                    _errors.Add(new SemanticError("Division by zero detected"));
            }
            AnalyzeExpression(e.Right);
        }
        else if (expr.GetType() == typeof(UnaryExpression))
        {
            UnaryExpression e = (UnaryExpression)expr;
            AnalyzeExpression(e.Operand);
        }
        else if (expr.GetType() == typeof(GroupExpression))
        {
            GroupExpression e = (GroupExpression)expr;
            AnalyzeExpression(e.Inner);
        }
        else if (expr.GetType() == typeof(CallExpression))
        {
            CallExpression e = (CallExpression)expr;
            if (!_table.IsFunctionDeclared(e.Callee))
                _errors.Add(new SemanticError("Function '" + e.Callee + "' is called but was never declared"));
            else
            {
                int arity = _table.GetArity(e.Callee);
                if (arity != e.Arguments.Count)
                    _errors.Add(new SemanticError("Function '" + e.Callee + "' expects " + arity + " arguments, got " + e.Arguments.Count));
            }
            for (int i = 0; i < e.Arguments.Count; i++)
                AnalyzeExpression(e.Arguments[i]);
        }
    }
}
