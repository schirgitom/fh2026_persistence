using Domain.Aggregates;
using Infrastructure.Persistence.Aggregates;

namespace UnitTests.Infrastructure;

public sealed class AggregateResolutionStrategyProviderTests
{
    private readonly AggregateResolutionStrategyProvider _provider = new(
    [
        new FiveMinuteAggregateResolutionStrategy(),
        new OneHourAggregateResolutionStrategy(),
        new OneDayAggregateResolutionStrategy()
    ]);

    [Theory]
    [InlineData(AggregateResolution.FiveMinutes, "5m")]
    [InlineData(AggregateResolution.OneHour, "1h")]
    [InlineData(AggregateResolution.OneDay, "1d")]
    public void Get_ReturnsExpectedStrategy(AggregateResolution resolution, string expectedSuffix)
    {
        var strategy = _provider.Get(resolution);
        Assert.Equal(expectedSuffix, strategy.ViewSuffix);
    }
}
