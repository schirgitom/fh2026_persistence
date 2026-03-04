using Application.Abstractions.Models;
using Application.Abstractions.Persistence;
using Application.Abstractions.Services;
using Domain.Aggregates;
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

    [GraphQLName("aggregates")]
    public async Task<IReadOnlyList<AggregateResult>> GetAggregatesAsync(
        [GraphQLName("aquariumId")]
        [ID] string aquariumId,
        [GraphQLName("resolution")]
        AggregateResolution resolution,
        [GraphQLName("from")]
        DateTimeOffset from,
        [GraphQLName("to")]
        DateTimeOffset to,
        [Service] IAggregateQueryService service,
        [Service] ILogger<MeasurementQueries> logger,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "GraphQL aggregates query for aquarium {AquariumId}, resolution {Resolution}, from {From}, to {To}",
            aquariumId,
            resolution,
            from,
            to);

        return await service.GetAsync(aquariumId, resolution, from, to, cancellationToken);
    }
}
