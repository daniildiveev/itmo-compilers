namespace SimpleInterpreter;

public class Environment
{
    private Dictionary<string, object> _values = new Dictionary<string, object>();
    private Environment? _parent;

    public Environment(Environment? parent)
    {
        _parent = parent;
    }

    public void Define(string name, object value)
    {
        if (_values.ContainsKey(name))
            throw new Exception("[Runtime Error] Variable '" + name + "' already defined");
        _values[name] = value;
    }

    public object Get(string name)
    {
        if (_values.ContainsKey(name))
            return _values[name];
        if (_parent != null)
            return _parent.Get(name);
        throw new Exception("[Runtime Error] Undefined variable '" + name + "'");
    }

    public void Set(string name, object value)
    {
        if (_values.ContainsKey(name))
        {
            _values[name] = value;
            return;
        }
        if (_parent != null)
        {
            _parent.Set(name, value);
            return;
        }
        throw new Exception("[Runtime Error] Undefined variable '" + name + "'");
    }
}
