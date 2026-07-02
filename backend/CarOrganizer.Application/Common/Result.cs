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
