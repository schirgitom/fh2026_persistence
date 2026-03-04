using Domain.Aggregates;

namespace Application.Abstractions.Services;

public interface IAggregateQueryService
{
    Task<IReadOnlyList<AggregateResult>> GetAsync(
        string aquariumId,
        AggregateResolution resolution,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken);
}
