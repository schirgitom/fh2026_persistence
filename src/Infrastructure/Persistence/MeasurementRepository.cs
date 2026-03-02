using Application.Abstractions.Models;
using Application.Abstractions.Persistence;
using Domain.Measurements;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence;

public sealed class MeasurementRepository : IMeasurementRepository, IMeasurementReadRepository
{
    private readonly IMeasurementDbContextFactory _dbContextFactory;
    private readonly IMeasurementDbContextScope _dbContextScope;
    private readonly ILogger<MeasurementRepository> _logger;

    public MeasurementRepository(
        IMeasurementDbContextFactory dbContextFactory,
        IMeasurementDbContextScope dbContextScope,
        ILogger<MeasurementRepository> logger)
    {
        _dbContextFactory = dbContextFactory;
        _dbContextScope = dbContextScope;
        _logger = logger;
    }

    public async Task UpsertAsync(Measurement measurement, CancellationToken cancellationToken)
    {
        var dbContext = _dbContextScope.Current
            ?? throw new InvalidOperationException(
                "No active unit of work. Start a unit of work before calling write repository methods.");

        _logger.LogInformation(
            "Persisting measurement using EF Core upsert for aquarium {AquariumId} at {Timestamp}",
            measurement.AquariumId,
            measurement.Timestamp);

        var existingEntity = await dbContext.Measurements.FindAsync(
            [measurement.AquariumId, measurement.Timestamp],
            cancellationToken);

        if (existingEntity is null)
        {
            dbContext.Measurements.Add(new MeasurementEntity
            {
                AquariumId = measurement.AquariumId,
                Timestamp = measurement.Timestamp,
                Temperature = measurement.Temperature,
                Mg = measurement.Mg,
                Kh = measurement.Kh,
                Ca = measurement.Ca,
                Ph = measurement.Ph,
                Oxygen = measurement.Oxygen,
                Pump = measurement.Pump
            });
        }
        else
        {
            existingEntity.Temperature = measurement.Temperature;
            existingEntity.Mg = measurement.Mg;
            existingEntity.Kh = measurement.Kh;
            existingEntity.Ca = measurement.Ca;
            existingEntity.Ph = measurement.Ph;
            existingEntity.Oxygen = measurement.Oxygen;
            existingEntity.Pump = measurement.Pump;
        }

    }

    public async Task<IReadOnlyList<MeasurementReadModel>> GetMeasurementsAsync(
        string aquariumId,
        DateTimeOffset from,
        DateTimeOffset to,
        int limit,
        CancellationToken cancellationToken)
    {
        var safeLimit = Math.Clamp(limit, 1, 5_000);
        await using var dbContext = await _dbContextFactory.CreateAsync(aquariumId, cancellationToken);

        _logger.LogInformation(
            "Querying measurements for aquarium {AquariumId} from {From} to {To} with limit {Limit}",
            aquariumId,
            from,
            to,
            safeLimit);

        return await dbContext.Measurements
            .AsNoTracking()
            .Where(x => x.AquariumId == aquariumId && x.Timestamp >= from && x.Timestamp <= to)
            .OrderByDescending(x => x.Timestamp)
            .Take(safeLimit)
            .Select(x => new MeasurementReadModel(
                x.AquariumId,
                x.Timestamp,
                x.Temperature,
                x.Mg,
                x.Kh,
                x.Ca,
                x.Ph,
                x.Oxygen,
                x.Pump))
            .ToListAsync(cancellationToken);
    }

    public async Task<MeasurementReadModel?> GetLatestMeasurementAsync(
        string aquariumId,
        CancellationToken cancellationToken)
    {
        await using var dbContext = await _dbContextFactory.CreateAsync(aquariumId, cancellationToken);

        _logger.LogInformation("Querying latest measurement for aquarium {AquariumId}", aquariumId);

        return await dbContext.Measurements
            .AsNoTracking()
            .Where(x => x.AquariumId == aquariumId)
            .OrderByDescending(x => x.Timestamp)
            .Select(x => new MeasurementReadModel(
                x.AquariumId,
                x.Timestamp,
                x.Temperature,
                x.Mg,
                x.Kh,
                x.Ca,
                x.Ph,
                x.Oxygen,
                x.Pump))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
