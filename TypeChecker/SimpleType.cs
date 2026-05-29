namespace SimpleTypeChecker;

public enum SimpleType
{
    Number,
    Bool,
    Unknown,
    NumberArray,
    BoolArray,
    UnknownArray
}

public static class ArrayTypes
{
    public static bool IsArray(SimpleType t)
    {
        return t == SimpleType.NumberArray || t == SimpleType.BoolArray || t == SimpleType.UnknownArray;
    }

    public static SimpleType ElementType(SimpleType arrayType)
    {
        if (arrayType == SimpleType.NumberArray) return SimpleType.Number;
        if (arrayType == SimpleType.BoolArray) return SimpleType.Bool;
        return SimpleType.Unknown;
    }

    public static SimpleType ArrayOf(SimpleType elemType)
    {
        if (elemType == SimpleType.Number) return SimpleType.NumberArray;
        if (elemType == SimpleType.Bool) return SimpleType.BoolArray;
        return SimpleType.UnknownArray;
    }
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
