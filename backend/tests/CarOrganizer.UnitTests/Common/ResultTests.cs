using CarOrganizer.Application.Common;

namespace CarOrganizer.UnitTests.Common;

public class ResultTests
{
    [Fact]
    public void Success_SetsSucceededToTrue()
    {
        var result = Result.Success();

        Assert.True(result.Succeeded);
    }

    [Fact]
    public void Success_HasNoErrors()
    {
        var result = Result.Success();

        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Failure_SetsSucceededToFalse()
    {
        var result = Result.Failure(["something went wrong"]);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public void Failure_PreservesTheProvidedError()
    {
        var result = Result.Failure(["boom"]);

        Assert.Equal(["boom"], result.Errors);
    }

    [Fact]
    public void Failure_PreservesAllErrorsInOrder()
    {
        var errors = new[] { "first", "second", "third" };

        var result = Result.Failure(errors);

        Assert.Equal(errors, result.Errors);
    }

    [Fact]
    public void Failure_WithEmptyCollection_IsStillAFailureWithNoErrors()
    {
        var result = Result.Failure([]);

        Assert.False(result.Succeeded);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Failure_CopiesErrors_SoLaterMutationOfSourceDoesNotLeak()
    {
        var source = new List<string> { "original" };

        var result = Result.Failure(source);
        source.Add("added later");

        Assert.Equal(["original"], result.Errors);
    }
}
