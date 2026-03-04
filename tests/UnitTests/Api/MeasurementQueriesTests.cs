using Api.GraphQl;
using Application.Abstractions.Services;
using Domain.Aggregates;
using Microsoft.Extensions.Logging.Abstractions;

namespace UnitTests.Api;

public sealed class MeasurementQueriesTests
{
    [Fact]
    public async Task GetAggregatesAsync_ForwardsCancellationTokenToApplicationService()
    {
        var service = new CapturingAggregateQueryService();
        var query = new MeasurementQueries();
        var token = new CancellationTokenSource().Token;

        await query.GetAggregatesAsync(
            "aq1",
            AggregateResolution.OneHour,
            DateTimeOffset.UtcNow.AddHours(-1),
            DateTimeOffset.UtcNow,
            service,
            NullLogger<MeasurementQueries>.Instance,
            token);

        Assert.Equal(token, service.LastToken);
    }

    private sealed class CapturingAggregateQueryService : IAggregateQueryService
    {
        public CancellationToken LastToken { get; private set; }

        public Task<IReadOnlyList<AggregateResult>> GetAsync(
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
