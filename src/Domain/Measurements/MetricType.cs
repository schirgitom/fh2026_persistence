using System.Globalization;

namespace Domain.Measurements;

public readonly record struct MetricType
{
    public static readonly MetricType Temperature = new("temperature");
    public static readonly MetricType Mg = new("mg");
    public static readonly MetricType Kh = new("kh");
    public static readonly MetricType Ca = new("ca");
    public static readonly MetricType Ph = new("ph");
    public static readonly MetricType Oxygen = new("oxygen");
    public static readonly MetricType Pump = new("pump");

    private static readonly IReadOnlyDictionary<string, MetricType> KnownValues =
        new Dictionary<string, MetricType>(StringComparer.OrdinalIgnoreCase)
        {
            [Temperature.Value] = Temperature,
            [Mg.Value] = Mg,
            [Kh.Value] = Kh,
            [Ca.Value] = Ca,
            [Ph.Value] = Ph,
            [Oxygen.Value] = Oxygen,
            [Pump.Value] = Pump
        };

    public MetricType(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainValidationException("Metric type must be provided.");
        }

        Value = value.Trim().ToLower(CultureInfo.InvariantCulture);
    }

    public string Value { get; }

    public static MetricType Parse(string value)
    {
        if (!TryParse(value, out var metricType))
        {
            throw new DomainValidationException($"Unsupported metric type '{value}'.");
        }

        return metricType;
    }

    public static bool TryParse(string value, out MetricType metricType)
    {
        return KnownValues.TryGetValue(value, out metricType);
    }

    public override string ToString() => Value;
}
