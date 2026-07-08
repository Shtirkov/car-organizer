namespace CarOrganizer.Application.Common;

/// <summary>
/// Outcome of an operation that can fail with one or more human-readable errors.
/// Lets the Application layer report failures without leaking Infrastructure types
/// (e.g. Identity's <c>IdentityResult</c>) to callers.
/// </summary>
public class Result
{
    public bool Succeeded { get; }

    public IReadOnlyList<string> Errors { get; }

    protected Result(bool succeeded, IEnumerable<string> errors)
    {
        Succeeded = succeeded;
        Errors = errors.ToArray();
    }

    public static Result Success() => new(true, []);

    public static Result Failure(IEnumerable<string> errors) => new(false, errors);
}

/// <summary>
/// A <see cref="Result"/> that also carries a <typeparamref name="T"/> value when it succeeds.
/// </summary>
public class Result<T> : Result
{
    /// <summary>The produced value on success; <c>default</c> when the result is a failure.</summary>
    public T? Value { get; }

    private Result(bool succeeded, T? value, IEnumerable<string> errors)
        : base(succeeded, errors)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(true, value, []);

    public static new Result<T> Failure(IEnumerable<string> errors) => new(false, default, errors);
}
