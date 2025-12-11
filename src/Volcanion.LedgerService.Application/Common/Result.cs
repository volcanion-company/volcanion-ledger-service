namespace Volcanion.LedgerService.Application.Common;

/// <summary>
/// Represents the outcome of an operation, encapsulating either a successful result with data or a failure with error
/// information.
/// </summary>
/// <remarks>Use the static factory methods to create instances representing success or failure. When <see
/// langword="IsSuccess"/> is <see langword="true"/>, <see langword="Data"/> contains the result value; otherwise, <see
/// langword="ErrorMessage"/> and <see langword="Errors"/> provide details about the failure. This type is commonly used
/// to standardize error handling and result reporting in APIs and service methods.</remarks>
/// <typeparam name="T">The type of the data returned when the operation is successful.</typeparam>
public class Result<T>
{
    /// <summary>
    /// Gets a value indicating whether the operation completed successfully.
    /// </summary>
    public bool IsSuccess { get; }
    /// <summary>
    /// Gets the data associated with the current instance.
    /// </summary>
    public T? Data { get; }
    /// <summary>
    /// Gets the error message associated with the current operation, if one is available.
    /// </summary>
    public string? ErrorMessage { get; }
    /// <summary>
    /// Gets the list of error messages associated with the current operation or object.
    /// </summary>
    /// <remarks>The returned list is read-only and reflects the errors that have occurred. The list will be
    /// empty if no errors are present.</remarks>
    public List<string> Errors { get; }

    /// <summary>
    /// Initializes a new instance of the Result class with the specified success state, data, error message, and
    /// optional list of errors.
    /// </summary>
    /// <param name="isSuccess">A value indicating whether the operation was successful. Set to <see langword="true"/> if successful; otherwise,
    /// <see langword="false"/>.</param>
    /// <param name="data">The data returned by the operation if successful, or <see langword="null"/> if the operation failed.</param>
    /// <param name="errorMessage">A message describing the error if the operation failed; otherwise, <see langword="null"/>.</param>
    /// <param name="errors">An optional list of error details associated with the operation. If <see langword="null"/>, an empty list is
    /// used.</param>
    protected Result(bool isSuccess, T? data, string? errorMessage, List<string>? errors = null)
    {
        IsSuccess = isSuccess;
        Data = data;
        ErrorMessage = errorMessage;
        Errors = errors ?? [];
    }

    /// <summary>
    /// Creates a successful result containing the specified data.
    /// </summary>
    /// <param name="data">The value to associate with the successful result. Can be any type supported by <typeparamref name="T"/>.</param>
    /// <returns>A <see cref="Result{T}"/> instance representing a successful operation with the provided data.</returns>
    public static Result<T> Success(T data) => new(true, data, null);
    
    /// <summary>
    /// Creates a failed result with the specified error message.
    /// </summary>
    /// <param name="errorMessage">The error message that describes the reason for the failure. Cannot be null or empty.</param>
    /// <returns>A <see cref="Result{T}"/> instance representing a failed operation, containing the provided error message.</returns>
    public static Result<T> Failure(string errorMessage) => new(false, default, errorMessage);
    
    /// <summary>
    /// Creates a failed result containing the specified error messages.
    /// </summary>
    /// <remarks>If multiple errors are provided, only the first error will be used as the primary error
    /// message. The full list of errors is available in the result for further inspection.</remarks>
    /// <param name="errors">A list of error messages describing the failure. Cannot be null or empty.</param>
    /// <returns>A <see cref="Result{T}"/> instance representing a failed operation, populated with the provided error messages.</returns>
    public static Result<T> Failure(List<string> errors) => new(false, default, errors.FirstOrDefault(), errors);
}

/// <summary>
/// Represents the outcome of an operation, indicating success or failure and providing error details if applicable.
/// </summary>
/// <remarks>Use the static methods <see cref="Success"/> and <see cref="Failure"/> to create instances
/// representing successful or failed results. When a failure occurs, error information is available through <see
/// cref="ErrorMessage"/> and <see cref="Errors"/>. This class is commonly used to standardize error handling and
/// reporting in APIs and service methods.</remarks>
public class Result
{
    /// <summary>
    /// Gets a value indicating whether the operation completed successfully.
    /// </summary>
    public bool IsSuccess { get; }
    /// <summary>
    /// Gets the error message associated with the current operation, if one is available.
    /// </summary>
    public string? ErrorMessage { get; }
    /// <summary>
    /// Gets the list of error messages associated with the current operation or object.
    /// </summary>
    public List<string> Errors { get; }

    /// <summary>
    /// Initializes a new instance of the Result class with the specified success state, error message, and optional
    /// list of errors.
    /// </summary>
    /// <param name="isSuccess">A value indicating whether the operation was successful. Set to <see langword="true"/> if successful; otherwise,
    /// <see langword="false"/>.</param>
    /// <param name="errorMessage">An optional error message describing the reason for failure. Can be <see langword="null"/> if the operation was
    /// successful or if no message is provided.</param>
    /// <param name="errors">An optional list of error details. If <see langword="null"/>, an empty list is used.</param>
    protected Result(bool isSuccess, string? errorMessage, List<string>? errors = null)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        Errors = errors ?? [];
    }

    /// <summary>
    /// Creates a new successful result instance.
    /// </summary>
    /// <returns>A <see cref="Result"/> representing a successful operation.</returns>
    public static Result Success() => new(true, null);
    
    /// <summary>
    /// Creates a failed result with the specified error message.
    /// </summary>
    /// <param name="errorMessage">The error message that describes the reason for the failure. Cannot be null or empty.</param>
    /// <returns>A <see cref="Result"/> instance representing a failure, containing the provided error message.</returns>
    public static Result Failure(string errorMessage) => new(false, errorMessage);
    
    /// <summary>
    /// Creates a failed result containing the specified error messages.
    /// </summary>
    /// <remarks>If multiple errors are provided, only the first error message will be used as the primary
    /// error. The returned result will indicate failure.</remarks>
    /// <param name="errors">A list of error messages describing the reasons for failure. Cannot be null or empty.</param>
    /// <returns>A <see cref="Result"/> instance representing a failure, populated with the provided error messages.</returns>
    public static Result Failure(List<string> errors) => new(false, errors.FirstOrDefault(), errors);
}
