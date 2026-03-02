using Application.Contracts;
using Domain.Measurements;
using System.Text.Json;

namespace Infrastructure.Messaging;

public static class MeasurementMessageParser
{
    public static MeasurementDto Parse(string payload)
    {
        using var document = JsonDocument.Parse(payload);
        var root = document.RootElement;

        var metricsElement = GetProperty(root, "Metrics");
        if (metricsElement.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException($"Metrics must be an object, but was '{metricsElement.ValueKind}'.");
        }

        var aquariumId = GetRequiredString(root, "AquariumId");
        var timestamp = GetRequiredDateTimeOffset(root, "Timestamp");
        var metrics = ParseNamedMetrics(metricsElement);

        return new MeasurementDto(aquariumId, timestamp, metrics);
    }

    private static IReadOnlyCollection<MetricValueDto> ParseNamedMetrics(JsonElement metricsElement)
    {
        var metrics = new List<MetricValueDto>();

        foreach (var metricProperty in metricsElement.EnumerateObject())
        {
            var metricType = ParseMetricTypeFromName(metricProperty.Name);
            var valueContainer = metricProperty.Value;

            var value = GetRequiredDecimal(valueContainer, "Value");
            var unit = GetRequiredString(valueContainer, "Unit");

            metrics.Add(new MetricValueDto(metricType, value, unit));
        }

        return metrics;
    }

    private static MetricType ParseMetricTypeFromName(string metricName)
    {
        var normalized = NormalizeMetricName(metricName);
        return normalized switch
        {
            "watertemperature" or "temperature" => MetricType.Temperature,
            "magnesium" or "mg" => MetricType.Mg,
            "alkalinity" or "kh" => MetricType.Kh,
            "calcium" or "ca" => MetricType.Ca,
            "ph" => MetricType.Ph,
            "oxygen" => MetricType.Oxygen,
            "waveflow" or "pump" => MetricType.Pump,
            _ => throw new JsonException($"Unsupported metric name '{metricName}'.")
        };
    }

    private static string NormalizeMetricName(string metricName)
    {
        return new string(metricName
            .Trim()
            .ToLowerInvariant()
            .Where(char.IsLetterOrDigit)
            .ToArray());
    }

    private static JsonElement GetProperty(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property) &&
            !element.TryGetProperty(propertyName.ToLowerInvariant(), out property))
        {
            throw new JsonException($"Required property '{propertyName}' is missing.");
        }

        return property;
    }

    private static string GetRequiredString(JsonElement element, string propertyName)
    {
        var property = GetProperty(element, propertyName);
        var value = property.GetString();
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new JsonException($"Property '{propertyName}' is required.");
        }

        return value;
    }

    private static DateTimeOffset GetRequiredDateTimeOffset(JsonElement element, string propertyName)
    {
        var property = GetProperty(element, propertyName);
        if (!property.TryGetDateTimeOffset(out var value))
        {
            throw new JsonException($"Property '{propertyName}' must be a valid DateTimeOffset.");
        }

        return value;
    }

    private static decimal GetRequiredDecimal(JsonElement element, string propertyName)
    {
        var property = GetProperty(element, propertyName);
        if (property.ValueKind != JsonValueKind.Number || !property.TryGetDecimal(out var value))
        {
            throw new JsonException($"Property '{propertyName}' must be a numeric value.");
        }

        return value;
    }
}
