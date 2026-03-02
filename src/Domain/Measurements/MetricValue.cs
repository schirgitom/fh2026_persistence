namespace Domain.Measurements;

public sealed record MetricValue(MetricType Type, decimal Value, string Unit);
