namespace SimpleSemantic;

public class SemanticError
{
    public string Message { get; }

    public SemanticError(string message)
    {
        Message = message;
    }

    public override string ToString()
    {
        return "[Semantic Error] " + Message;
    }
}
