using Domain.Measurements;

namespace Application.Contracts;

public sealed record MeasurementDto(
    string AquariumId,
    DateTimeOffset Timestamp,
    IReadOnlyCollection<MetricValueDto> Metrics);

public sealed record MetricValueDto(
    MetricType Type,
    decimal Value,
    string Unit);
