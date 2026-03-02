using Domain;
using Domain.Measurements;

namespace UnitTests.Domain;

public sealed class MeasurementTests
{
    [Fact]
    public void Create_WithValidData_ReturnsMeasurement()
    {
        var now = DateTimeOffset.UtcNow;
        var measurement = Measurement.Create(
            "aq-1",
            now,
            new[]
            {
                new MetricValue(MetricType.Temperature, 24.0m, "C"),
                new MetricValue(MetricType.Ph, 7.2m, "pH"),
                new MetricValue(MetricType.Oxygen, 8.0m, "mg/l"),
                new MetricValue(MetricType.Pump, 1m, "state")
            },
            TimeSpan.FromSeconds(30),
            now);

        Assert.Equal("aq-1", measurement.AquariumId);
        Assert.Equal(24.0m, measurement.Temperature);
        Assert.Equal(7.2m, measurement.Ph);
        Assert.Equal(8.0m, measurement.Oxygen);
        Assert.Equal(1m, measurement.Pump);
    }

    [Fact]
    public void Create_WithNonPositiveTemperature_Throws()
    {
        var now = DateTimeOffset.UtcNow;

        Assert.Throws<DomainValidationException>(() => Measurement.Create(
            "aq-1",
            now,
            new[]
            {
                new MetricValue(MetricType.Temperature, 0m, "C"),
                new MetricValue(MetricType.Ph, 7.0m, "pH"),
                new MetricValue(MetricType.Oxygen, 1.0m, "mg/l")
            },
            TimeSpan.FromSeconds(30),
            now));
    }

    [Fact]
    public void Create_WithPhOutOfRange_Throws()
    {
        var now = DateTimeOffset.UtcNow;

        Assert.Throws<DomainValidationException>(() => Measurement.Create(
            "aq-1",
            now,
            new[]
            {
                new MetricValue(MetricType.Temperature, 24m, "C"),
                new MetricValue(MetricType.Ph, 14.1m, "pH"),
                new MetricValue(MetricType.Oxygen, 1.0m, "mg/l")
            },
            TimeSpan.FromSeconds(30),
            now));
    }

    [Fact]
    public void Create_WithNegativeOxygen_Throws()
    {
        var now = DateTimeOffset.UtcNow;

        Assert.Throws<DomainValidationException>(() => Measurement.Create(
            "aq-1",
            now,
            new[]
            {
                new MetricValue(MetricType.Temperature, 24m, "C"),
                new MetricValue(MetricType.Ph, 7.0m, "pH"),
                new MetricValue(MetricType.Oxygen, -0.1m, "mg/l")
            },
            TimeSpan.FromSeconds(30),
            now));
    }

    [Fact]
    public void Create_WithFutureTimestampBeyondTolerance_Throws()
    {
        var now = DateTimeOffset.UtcNow;
        var futureTimestamp = now.AddSeconds(31);

        Assert.Throws<DomainValidationException>(() => Measurement.Create(
            "aq-1",
            futureTimestamp,
            new[]
            {
                new MetricValue(MetricType.Temperature, 24m, "C"),
                new MetricValue(MetricType.Ph, 7.0m, "pH"),
                new MetricValue(MetricType.Oxygen, 1.0m, "mg/l")
            },
            TimeSpan.FromSeconds(30),
            now));
    }
}
