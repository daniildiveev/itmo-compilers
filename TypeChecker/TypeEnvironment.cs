namespace SimpleTypeChecker;

public class TypeEnvironment
{
    private List<Dictionary<string, SimpleType>> _scopes;

    public TypeEnvironment()
    {
        _scopes = new List<Dictionary<string, SimpleType>>();
    }

    public void EnterScope()
    {
        _scopes.Add(new Dictionary<string, SimpleType>());
    }

    public void ExitScope()
    {
        _scopes.RemoveAt(_scopes.Count - 1);
    }

    public void Declare(string name, SimpleType type)
    {
        _scopes[_scopes.Count - 1][name] = type;
    }

    public SimpleType Lookup(string name)
    {
        for (int i = _scopes.Count - 1; i >= 0; i--)
        {
            if (_scopes[i].ContainsKey(name))
                return _scopes[i][name];
        }
        return SimpleType.Unknown;
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

    public bool IsDeclaredInCurrentScope(string name)
    {
        return _scopes[_scopes.Count - 1].ContainsKey(name);
    }
}
