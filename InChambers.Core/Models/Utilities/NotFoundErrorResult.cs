using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;

namespace InChambers.Core.Models.Utilities;

/// <summary>
/// Represents a Not found error result, derived from the <see cref="Result"/> class.
/// </summary>
public class NotFoundErrorResult : Result
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundErrorResult"/> class with a default failure status.
    /// </summary>
    public NotFoundErrorResult() : base(false)
    {
        Status = StatusCodes.Status404NotFound;
        Title = "Resource requested is not found";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundErrorResult"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message associated with the result.</param>
    public NotFoundErrorResult(string message) : base(false, message)
    {
        Status = StatusCodes.Status404NotFound;
        Title = "Resource requested is not found";
    }

    // Ignore error related properties

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    [JsonIgnore]
    public new string Detail { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    [JsonIgnore]
    public new string Instance { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    [JsonIgnore]
    public new string Path { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    [JsonIgnore]
    public new TraceInfo TraceInfo { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    [JsonIgnore]
    public new IDictionary<string, string[]> ValidationErrors { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    [JsonIgnore]
    public new object Content { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    [JsonIgnore]
    public new Paging Paging { get; set; }
}

/// <summary>
/// Represents an error result.
/// </summary>
public class NotFoundErrorResult<T> : Result<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundErrorResult"/> class with a default failure status.
    /// </summary>
    public NotFoundErrorResult() : base(false)
    {
        Status = StatusCodes.Status404NotFound;
        Title = "Resource requested is not found";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundErrorResult"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message associated with the result.</param>
    public NotFoundErrorResult(string message) : base(false, message)
    {
        Status = StatusCodes.Status404NotFound;
        Title = "Resource requested is not found";
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    [JsonIgnore]
    public new string Detail { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    [JsonIgnore]
    public new string Instance { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    [JsonIgnore]
    public new string Path { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    [JsonIgnore]
    public new TraceInfo TraceInfo { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    [JsonIgnore]
    public new IDictionary<string, string[]> ValidationErrors { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    [JsonIgnore]
    public new T Content { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    [JsonIgnore]
    public new Paging Paging { get; set; }
}