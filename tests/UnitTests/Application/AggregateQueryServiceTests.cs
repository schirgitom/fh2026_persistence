using Application.Abstractions.Persistence;
using Application.Aggregates;
using Domain.Aggregates;

namespace UnitTests.Application;

public sealed class AggregateQueryServiceTests
{
    [Fact]
    public async Task GetAsync_ForwardsCancellationTokenToRepository()
    {
        var repository = new CapturingAggregateReadRepository();
        var service = new AggregateQueryService(repository);
        var token = new CancellationTokenSource().Token;

        await service.GetAsync("aq1", AggregateResolution.FiveMinutes, DateTimeOffset.UtcNow.AddMinutes(-5), DateTimeOffset.UtcNow, token);

        Assert.Equal(token, repository.LastToken);
    }

    [Fact]
    public async Task GetAsync_WithFromGreaterThanTo_Throws()
    {
        var repository = new CapturingAggregateReadRepository();
        var service = new AggregateQueryService(repository);

        await Assert.ThrowsAsync<ArgumentException>(() => service.GetAsync(
            "aq1",
            AggregateResolution.FiveMinutes,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddMinutes(-1),
            CancellationToken.None));
    }

    private sealed class CapturingAggregateReadRepository : IAggregateReadRepository
    {
        public CancellationToken LastToken { get; private set; }

        public Task<IReadOnlyList<AggregateResult>> GetAggregatesAsync(
            string aquariumId,
            AggregateResolution resolution,
            DateTimeOffset from,
            DateTimeOffset to,
            CancellationToken cancellationToken)
        {
            LastToken = cancellationToken;
            return Task.FromResult<IReadOnlyList<AggregateResult>>([]);
        }
    }
}
