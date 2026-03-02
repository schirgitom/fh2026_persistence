using Application.Abstractions.Cqrs;
using Application.Abstractions.Persistence;
using Application.Configuration;
using Domain.Measurements;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Measurements;

public sealed class IngestMeasurementCommandHandler : ICommandHandler<IngestMeasurementCommand>
{
    private readonly IMeasurementRepository _measurementRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOptions<MeasurementValidationOptions> _validationOptions;
    private readonly ILogger<IngestMeasurementCommandHandler> _logger;

    public IngestMeasurementCommandHandler(
        IMeasurementRepository measurementRepository,
        IUnitOfWork unitOfWork,
        IOptions<MeasurementValidationOptions> validationOptions,
        ILogger<IngestMeasurementCommandHandler> logger)
    {
        _measurementRepository = measurementRepository;
        _unitOfWork = unitOfWork;
        _validationOptions = validationOptions;
        _logger = logger;
    }

    public async Task HandleAsync(IngestMeasurementCommand command, CancellationToken cancellationToken)
    {
        var dto = command.Measurement;
        var tolerance = TimeSpan.FromSeconds(_validationOptions.Value.FutureToleranceSeconds);

        var domainMetrics = dto.Metrics
            .Select(x => new MetricValue(x.Type, x.Value, x.Unit))
            .ToArray();

        var measurement = Measurement.Create(
            dto.AquariumId,
            dto.Timestamp,
            domainMetrics,
            tolerance,
            DateTimeOffset.UtcNow);

        _logger.LogInformation(
            "Ingesting measurement for aquarium {AquariumId} at {Timestamp}",
            measurement.AquariumId,
            measurement.Timestamp);

        await _unitOfWork.ExecuteAsync(
            measurement.AquariumId,
            async token => { await _measurementRepository.UpsertAsync(measurement, token); },
            cancellationToken);
    }
}
