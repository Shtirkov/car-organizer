using CarOrganizer.Application.Common;

namespace CarOrganizer.UnitTests.Common;

public class ResultOfTTests
{
    [Fact]
    public void Success_SetsSucceededTrueAndCarriesTheValue()
    {
        var result = Result<string>.Success("token");

        Assert.True(result.Succeeded);
        Assert.Equal("token", result.Value);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Failure_SetsSucceededFalseWithDefaultValueAndErrors()
    {
        var result = Result<string>.Failure(["bad"]);

        Assert.False(result.Succeeded);
        Assert.Null(result.Value);
        Assert.Equal(["bad"], result.Errors);
    }

    [Fact]
    public void Failure_ForValueType_LeavesValueAtDefault()
    {
        var result = Result<int>.Failure(["bad"]);

        Assert.False(result.Succeeded);
        Assert.Equal(0, result.Value);
    }

    [Fact]
    public void Success_CanCarryAReferenceValue()
    {
        var payload = new { Id = 42 };

        var result = Result<object>.Success(payload);

        Assert.Same(payload, result.Value);
    }
}
