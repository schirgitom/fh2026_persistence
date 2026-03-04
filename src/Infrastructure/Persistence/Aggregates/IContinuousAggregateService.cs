namespace Infrastructure.Persistence.Aggregates;

public interface IContinuousAggregateService
{
    Task EnsureAggregatesAsync(string aquariumId, CancellationToken cancellationToken);
}
