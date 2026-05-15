namespace SimpleTypeChecker;

public class TypeError
{
    public string Message { get; }

    public TypeError(string message)
    {
        Message = message;
    }

    public override string ToString()
    {
        return "[Type Error] " + Message;
    }
}
