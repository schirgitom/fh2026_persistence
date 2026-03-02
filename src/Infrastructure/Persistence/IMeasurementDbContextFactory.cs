namespace Infrastructure.Persistence;

public interface IMeasurementDbContextFactory
{
    Task<MeasurementDbContext> CreateAsync(string aquariumId, CancellationToken cancellationToken);
}
