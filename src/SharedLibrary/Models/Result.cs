namespace SharedLibrary.Models;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T Data { get; }
    public string Error { get; }

    private Result(bool isSuccess, T data, string error)
    {
        IsSuccess = isSuccess;
        Data = data;
        Error = error;
    }

    public static Result<T> Success(T data) => new(true, data, string.Empty);
    public static Result<T> Failure(string error) => new(false, default, error);
} 