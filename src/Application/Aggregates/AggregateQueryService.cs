using Application.Abstractions.Persistence;
using Application.Abstractions.Services;
using Domain.Aggregates;

namespace Application.Aggregates;

public sealed class AggregateQueryService : IAggregateQueryService
{
    private readonly IAggregateReadRepository _repository;

    public AggregateQueryService(IAggregateReadRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<AggregateResult>> GetAsync(
        string aquariumId,
        AggregateResolution resolution,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(aquariumId))
        {
            throw new ArgumentException("AquariumId must be provided.", nameof(aquariumId));
        }

        if (from > to)
        {
            throw new ArgumentException("From must be less than or equal to To.");
        }

        return await _repository.GetAggregatesAsync(
            aquariumId.Trim(),
            resolution,
            from,
            to,
            cancellationToken);
    }
}
