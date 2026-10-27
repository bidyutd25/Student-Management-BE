namespace demoapi.Common.Results;

public sealed class ServiceResult<T>
{
    public Outcome Outcome { get; }

    public T? Value { get; }

    public string? Error { get; }

    private ServiceResult(Outcome outcome, T? value, string? error)
    {
        Outcome = outcome;
        Value = value;
        Error = error;
    }

    public static ServiceResult<T> Ok(T value) =>
        new(Outcome.Ok, value, null);

    public static ServiceResult<T> NotFound(string error) =>
        new(Outcome.NotFound, default, error);

    public static ServiceResult<T> Conflict(string error) =>
        new(Outcome.Conflict, default, error);

    public static ServiceResult<T> BadRequest(string error) =>
        new(Outcome.BadRequest, default, error);
}