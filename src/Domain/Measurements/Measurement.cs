namespace Domain.Measurements;

public sealed record Measurement
{
    private Measurement(
        string aquariumId,
        DateTimeOffset timestamp,
        decimal? temperature,
        decimal? mg,
        decimal? kh,
        decimal? ca,
        decimal? ph,
        decimal? oxygen,
        decimal? pump)
    {
        AquariumId = aquariumId;
        Timestamp = timestamp;
        Temperature = temperature;
        Mg = mg;
        Kh = kh;
        Ca = ca;
        Ph = ph;
        Oxygen = oxygen;
        Pump = pump;
    }

    public string AquariumId { get; }
    public DateTimeOffset Timestamp { get; }
    public decimal? Temperature { get; }
    public decimal? Mg { get; }
    public decimal? Kh { get; }
    public decimal? Ca { get; }
    public decimal? Ph { get; }
    public decimal? Oxygen { get; }
    public decimal? Pump { get; }

    public static Measurement Create(
        string aquariumId,
        DateTimeOffset timestamp,
        IReadOnlyCollection<MetricValue> metrics,
        TimeSpan futureTolerance,
        DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(aquariumId))
        {
            throw new DomainValidationException("AquariumId must be provided.");
        }

        if (timestamp > nowUtc.Add(futureTolerance))
        {
            throw new DomainValidationException(
                $"Timestamp '{timestamp:O}' is in the future beyond tolerance '{futureTolerance}'.");
        }

        var metricMap = metrics
            .GroupBy(x => x.Type.Value, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.Last().Value, StringComparer.OrdinalIgnoreCase);

        var temperature = GetMetric(metricMap, MetricType.Temperature);
        var mg = GetMetric(metricMap, MetricType.Mg);
        var kh = GetMetric(metricMap, MetricType.Kh);
        var ca = GetMetric(metricMap, MetricType.Ca);
        var ph = GetMetric(metricMap, MetricType.Ph);
        var oxygen = GetMetric(metricMap, MetricType.Oxygen);
        var pump = GetMetric(metricMap, MetricType.Pump);

        if (temperature is <= 0m)
        {
            throw new DomainValidationException("Temperature must be greater than zero.");
        }

        if (ph is < 0m or > 14m)
        {
            throw new DomainValidationException("pH must be in range 0 to 14.");
        }

        if (oxygen is < 0m)
        {
            throw new DomainValidationException("Oxygen must be greater than or equal to zero.");
        }

        return new Measurement(
            aquariumId.Trim(),
            timestamp,
            temperature,
            mg,
            kh,
            ca,
            ph,
            oxygen,
            pump);
    }

    private static decimal? GetMetric(IReadOnlyDictionary<string, decimal> metrics, MetricType type)
    {
        return metrics.TryGetValue(type.Value, out var value) ? value : null;
    }
}
