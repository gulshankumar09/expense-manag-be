using System.Text.Json.Serialization;

namespace SharedLibrary.Models;

/// <summary>
/// Represents a basic result without data, only success/failure status
/// </summary>
public class Result : IResult
{
    public bool IsSuccess { get; private set; }
    public object? Error { get; private set; }

    [JsonIgnore]
    private Dictionary<string, string> SetHeaders  = new Dictionary<string, string>();

    [JsonIgnore]
    public IReadOnlyDictionary<string, string> Headers => SetHeaders;

    private Result(bool isSuccess, object? error = null)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public void AddHeader(string key, string value)
    {
        SetHeaders[key] = value;
    }

    public IReadOnlyDictionary<string, string> GetHeaders()
    {
        return Headers.ToDictionary(x => x.Key, x => x.Value);
    }

    public static Result Success() => new(true);
    public static Result Failure(object error) => new(false, error);

    // Convert to Result<T>
    public Result<T> ToTyped<T>(T? data = default) => IsSuccess
        ? Result<T>.Success(data!)
        : Result<T>.Failure(Error!);
}

/// <summary>
/// Represents a result of an operation that can succeed with data or fail with an error
/// </summary>
/// <typeparam name="T">The type of the result data</typeparam>
public class Result<T> : IResult<T>
{
    public bool IsSuccess { get; private set; }
    public T? Data { get; private set; }
    public object? Error { get; private set; }

    [JsonIgnore]
    private Dictionary<string, string> SetHeaders = new Dictionary<string, string>();

    [JsonIgnore]
    public IReadOnlyDictionary<string, string> Headers => SetHeaders;

    private Result(bool isSuccess, T? data, object? error)
    {
        IsSuccess = isSuccess;
        Data = data;
        Error = error;
    }

    public void AddHeader(string key, string value)
    {
        SetHeaders[key] = value;
    }

    public IReadOnlyDictionary<string, string> GetHeaders()
    {
        return Headers.ToDictionary(x => x.Key, x => x.Value);
    }

    public static Result<T> Success(T data) => new(true, data, null);
    public static Result<T> Failure(object error) => new(false, default, error);

    /// <summary>
    /// Creates a new successful result from another result type
    /// </summary>
    public static Result<T> From<TSource>(Result<TSource> result, Func<TSource, T> mapper)
    {
        if (result.IsSuccess && result.Data != null)
        {
            var newResult = Success(mapper(result.Data));
            foreach (var header in result.GetHeaders())
            {
                newResult.AddHeader(header.Key, header.Value);
            }
            return newResult;
        }

        return new Result<T>(false, default, result.Error);
    }
}