using Infrastructure.Persistence.Aggregates;

namespace UnitTests.Infrastructure;

public sealed class SqlIdentifierValidatorTests
{
    private readonly SqlIdentifierValidator _validator = new();

    [Theory]
    [InlineData("aq_1")]
    [InlineData("AQ123")]
    [InlineData("abc")]
    public void EnsureValidAquariumId_WithValidInput_ReturnsTrimmedValue(string aquariumId)
    {
        var result = _validator.EnsureValidAquariumId($" {aquariumId} ");
        Assert.Equal(aquariumId, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void EnsureValidAquariumId_WithInvalidInput_Throws(string aquariumId)
    {
        Assert.Throws<ArgumentException>(() => _validator.EnsureValidAquariumId(aquariumId));
    }

    [Fact]
    public void BuildAggregateViewName_WithGuid_ProducesSqlSafeIdentifier()
    {
        var aquariumId = "6F9619FF-8B86-D011-B42D-00C04FC964FF";

        var viewName = _validator.BuildAggregateViewName(aquariumId, "1h");

        Assert.Equal("measurement_6f9619ff_8b86_d011_b42d_00c04fc964ff_1h", viewName);
    }
}
