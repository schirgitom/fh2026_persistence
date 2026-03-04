using Domain.Aggregates;

namespace Application.Abstractions.Persistence;

public interface IAggregateReadRepository
{
    Task<IReadOnlyList<AggregateResult>> GetAggregatesAsync(
        string aquariumId,
        AggregateResolution resolution,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken);
}
