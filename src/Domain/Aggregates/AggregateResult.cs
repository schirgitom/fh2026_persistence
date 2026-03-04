namespace Domain.Aggregates;

public sealed record AggregateResult(
    DateTimeOffset Bucket,
    double? AvgTemperature,
    double? MinTemperature,
    double? MaxTemperature,
    double? StdDevTemperature,
    double? AvgPh,
    double? MinPh,
    double? MaxPh,
    double? StdDevPh,
    double? AvgOxygen,
    double? MinOxygen,
    double? MaxOxygen,
    double? AvgMg,
    double? AvgKh,
    double? AvgCa,
    double? AvgPump,
    double? MaxPump);
