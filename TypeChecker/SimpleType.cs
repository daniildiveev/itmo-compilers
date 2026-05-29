namespace SimpleTypeChecker;

public enum SimpleType
{
    Number,
    Bool,
    Unknown
}

public class FunctionSignature
{
    public List<string> ParameterNames { get; }
    public List<SimpleType> ParameterTypes { get; }
    public SimpleType ReturnType { get; set; }

    public FunctionSignature(List<string> names, List<SimpleType> types, SimpleType returnType)
    {
        ParameterNames = names;
        ParameterTypes = types;
        ReturnType = returnType;
    }
}
