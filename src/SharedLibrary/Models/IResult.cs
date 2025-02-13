using System.Text.Json.Serialization;

namespace SharedLibrary.Models;

/// <summary>
/// Represents the base interface for all result types
/// </summary>
public interface IResult
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful
    /// </summary>
    bool IsSuccess { get; }

    /// <summary>
    /// Gets the error information if the operation failed
    /// </summary>
    object? Error { get; }
    
    /// <summary>
    /// Gets the headers associated with the result
    /// </summary>
    [JsonIgnore]
    IReadOnlyDictionary<string, string> Headers { get; }
}

/// <summary>
/// Represents a result with data of type T
/// </summary>
/// <typeparam name="T">The type of the result data</typeparam>
public interface IResult<out T> : IResult
{
    /// <summary>
    /// Gets the data if the operation was successful
    /// </summary>
    T? Data { get; }
}