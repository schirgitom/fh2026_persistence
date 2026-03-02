namespace Application.Abstractions.Models;

public sealed record MeasurementReadModel(
    string AquariumId,
    DateTimeOffset Timestamp,
    decimal? Temperature,
    decimal? Mg,
    decimal? Kh,
    decimal? Ca,
    decimal? Ph,
    decimal? Oxygen,
    decimal? Pump);
