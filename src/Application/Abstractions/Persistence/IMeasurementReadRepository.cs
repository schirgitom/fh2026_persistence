using Application.Abstractions.Models;

namespace Application.Abstractions.Persistence;

public interface IMeasurementReadRepository
{
    Task<IReadOnlyList<MeasurementReadModel>> GetMeasurementsAsync(
        string aquariumId,
        DateTimeOffset from,
        DateTimeOffset to,
        int limit,
        CancellationToken cancellationToken);

    Task<MeasurementReadModel?> GetLatestMeasurementAsync(
        string aquariumId,
        CancellationToken cancellationToken);
}
