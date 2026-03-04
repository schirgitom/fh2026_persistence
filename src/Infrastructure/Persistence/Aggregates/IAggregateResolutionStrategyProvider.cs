using Domain.Aggregates;

namespace Infrastructure.Persistence.Aggregates;

public interface IAggregateResolutionStrategyProvider
{
    IAggregateResolutionStrategy Get(AggregateResolution resolution);
}
