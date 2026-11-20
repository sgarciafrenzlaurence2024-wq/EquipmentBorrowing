namespace EquipmentBorrowing.Application.Services;

public enum OperationOutcome
{
    Success,
    ValidationFailed,
    Failure,
    UnknownOutcome
}

public sealed record OperationResult<T>(
    bool IsSuccess,
    OperationOutcome Outcome,
    string Message,
    T? Data = default)
{
    public static OperationResult<T> Success(T data, string message = "Operation completed successfully.") =>
        new(true, OperationOutcome.Success, message, data);

    public static OperationResult<T> ValidationFailed(string message) =>
        new(false, OperationOutcome.ValidationFailed, message);

    public static OperationResult<T> Failure(string message) =>
        new(false, OperationOutcome.Failure, message);

    public static OperationResult<T> UnknownOutcome(string message) =>
        new(false, OperationOutcome.UnknownOutcome, message);
}

public sealed record OperationResult(
    bool IsSuccess,
    OperationOutcome Outcome,
    string Message)
{
    public static OperationResult Success(string message = "Operation completed successfully.") =>
        new(true, OperationOutcome.Success, message);

    public static OperationResult ValidationFailed(string message) =>
        new(false, OperationOutcome.ValidationFailed, message);

    public static OperationResult Failure(string message) =>
        new(false, OperationOutcome.Failure, message);

    public static OperationResult UnknownOutcome(string message) =>
        new(false, OperationOutcome.UnknownOutcome, message);
}