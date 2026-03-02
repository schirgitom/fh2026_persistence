using Application.Contracts;

namespace Application.Measurements;

public sealed record IngestMeasurementCommand(MeasurementDto Measurement);
