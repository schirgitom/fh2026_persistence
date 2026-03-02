using Domain;
using Domain.Measurements;

namespace UnitTests.Domain;

public sealed class MetricTypeTests
{
    [Fact]
    public void Parse_KnownMetric_ReturnsExpectedType()
    {
        var metricType = MetricType.Parse("temperature");

        Assert.Equal(MetricType.Temperature, metricType);
    }

    [Fact]
    public void Parse_UnknownMetric_ThrowsDomainValidationException()
    {
        Assert.Throws<DomainValidationException>(() => MetricType.Parse("invalid-metric"));
    }

    [Fact]
    public void Constructor_NormalizesValue()
    {
        var metricType = new MetricType("  Ph  ");

        Assert.Equal("ph", metricType.Value);
    }
}
