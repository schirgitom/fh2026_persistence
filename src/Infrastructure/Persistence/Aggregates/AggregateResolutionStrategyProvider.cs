using Domain.Aggregates;

namespace Infrastructure.Persistence.Aggregates;

public sealed class AggregateResolutionStrategyProvider : IAggregateResolutionStrategyProvider
{
    private readonly IReadOnlyDictionary<AggregateResolution, IAggregateResolutionStrategy> _strategies;

    public AggregateResolutionStrategyProvider(IEnumerable<IAggregateResolutionStrategy> strategies)
    {
        _strategies = strategies.ToDictionary(x => x.Resolution);
    }

    public IAggregateResolutionStrategy Get(AggregateResolution resolution)
    {
        if (_strategies.TryGetValue(resolution, out var strategy))
        {
            return strategy;
        }

        throw new ArgumentOutOfRangeException(nameof(resolution), resolution, "Unsupported aggregate resolution.");
    }
}
