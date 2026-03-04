using Application.Abstractions.Persistence;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Aggregates;

namespace UnitTests.Infrastructure;

public sealed class ContinuousAggregateServiceTests
{
    [Fact]
    public async Task EnsureAggregatesAsync_ForwardsCancellationTokenToUnitOfWork()
    {
        var unitOfWork = new CapturingUnitOfWork();
        var service = new ContinuousAggregateService(
            unitOfWork,
            new MeasurementDbContextScope(),
            new SqlIdentifierValidator(),
            [new FiveMinuteAggregateResolutionStrategy()],
            new ContinuousAggregateSqlBuilder());

        using var cts = new CancellationTokenSource();

        await service.EnsureAggregatesAsync("aq1", cts.Token);

        Assert.Equal(cts.Token, unitOfWork.LastToken);
    }

    private sealed class CapturingUnitOfWork : IUnitOfWork
    {
        public CancellationToken LastToken { get; private set; }

        public Task ExecuteAsync(
            string aquariumId,
            Func<CancellationToken, Task> operation,
            CancellationToken cancellationToken)
        {
            LastToken = cancellationToken;
            return Task.CompletedTask;
        }
    }
}
