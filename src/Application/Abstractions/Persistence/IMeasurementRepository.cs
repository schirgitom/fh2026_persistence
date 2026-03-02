using Domain.Measurements;

namespace Application.Abstractions.Persistence;

public interface IMeasurementRepository
{
    Task UpsertAsync(Measurement measurement, CancellationToken cancellationToken);
}
