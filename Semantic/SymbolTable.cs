namespace SimpleSemantic;

public class SymbolTable
{
    private List<Dictionary<string, bool>> _scopes;
    private Dictionary<string, int> _functions;

    public SymbolTable()
    {
        _scopes = new List<Dictionary<string, bool>>();
        _functions = new Dictionary<string, int>();
    }

    public void EnterScope()
    {
        _scopes.Add(new Dictionary<string, bool>());
    }

    public void ExitScope()
    {
        _scopes.RemoveAt(_scopes.Count - 1);
    }

    public void Declare(string name)
    {
        _scopes[_scopes.Count - 1][name] = true;
    }

    public bool IsDeclaredInCurrentScope(string name)
    {
        return _scopes[_scopes.Count - 1].ContainsKey(name);
    }

    public bool IsDeclared(string name)
    {
        for (int i = _scopes.Count - 1; i >= 0; i--)
        {
            if (_scopes[i].ContainsKey(name))
                return true;
        }
        return false;
    }

    public void DeclareFunction(string name, int arity)
    {
        _functions[name] = arity;
    }

    public bool IsFunctionDeclared(string name)
    {
        return _functions.ContainsKey(name);
    }

    public int GetArity(string name)
    {
        return _functions[name];
    }
}
