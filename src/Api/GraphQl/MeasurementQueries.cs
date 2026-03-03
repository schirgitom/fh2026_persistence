using Application.Abstractions.Models;
using Application.Abstractions.Persistence;
using HotChocolate;

namespace Api.GraphQl;

public sealed class MeasurementQueries
{
    public async Task<IReadOnlyList<MeasurementReadModel>> GetMeasurementsAsync(
        [ID] string aquariumId,
        DateTimeOffset from,
        DateTimeOffset to,
        int limit,
        [Service] IMeasurementReadRepository repository,
        [Service] ILogger<MeasurementQueries> logger,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "GraphQL measurements query for aquarium {AquariumId}, from {From}, to {To}, limit {Limit}",
            aquariumId,
            from,
            to,
            limit);
        

        return await repository.GetMeasurementsAsync(aquariumId, from, to, limit, cancellationToken);
    }

    public async Task<MeasurementReadModel?> GetLatestMeasurementAsync(
        [ID] string aquariumId,
        [Service] IMeasurementReadRepository repository,
        [Service] ILogger<MeasurementQueries> logger,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("GraphQL latestMeasurement query for aquarium {AquariumId}", aquariumId);
        return await repository.GetLatestMeasurementAsync(aquariumId, cancellationToken);
    }
}
