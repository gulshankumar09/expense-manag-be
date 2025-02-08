namespace SharedLibrary.Exceptions;

public class BaseException : Exception
{
    public string Code { get; }

    public BaseException(string message, string code = "general_error") : base(message)
    {
        Code = code;
    }
}