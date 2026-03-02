using Domain.Measurements;
using Infrastructure.Messaging;
using System.Text.Json;

namespace UnitTests.Infrastructure;

public sealed class MeasurementMessageParserTests
{
    [Fact]
    public void Parse_WithNamedMetricsObject_ParsesSuccessfully()
    {
        const string json = """
                            {
                              "AquariumId":"aac238ef-bcf8-42d2-a885-712cb5ff8d16",
                              "Timestamp":"2026-02-24T15:02:14.976+00:00",
                              "Metrics":{
                                "WaterTemperature":{"Value":24.4274688126323,"Unit":"C"},
                                "Magnesium":{"Value":300.496182729933,"Unit":"mg/L"},
                                "Alkalinity":{"Value":7.14286540380834,"Unit":"dKH"},
                                "Calcium":{"Value":1300.52952915719,"Unit":"ppm"},
                                "Ph":{"Value":7.20859415791431,"Unit":"pH"},
                                "WaveFlow":{"Value":1,"Unit":"L/min"}
                              }
                            }
                            """;

        var dto = MeasurementMessageParser.Parse(json);

        Assert.Equal("aac238ef-bcf8-42d2-a885-712cb5ff8d16", dto.AquariumId);
        Assert.Equal(6, dto.Metrics.Count);
        Assert.Equal(
            new[]
            {
                MetricType.Temperature,
                MetricType.Mg,
                MetricType.Kh,
                MetricType.Ca,
                MetricType.Ph,
                MetricType.Pump
            },
            dto.Metrics.Select(x => x.Type).ToArray());
    }

    [Fact]
    public void Parse_WithUnknownNamedMetric_ThrowsJsonException()
    {
        const string json = """
                            {
                              "AquariumId":"aq-1",
                              "Timestamp":"2026-02-24T15:02:14.976+00:00",
                              "Metrics":{
                                "UnknownMetric":{"Value":1,"Unit":"x"}
                              }
                            }
                            """;

        Assert.Throws<JsonException>(() => MeasurementMessageParser.Parse(json));
    }

    [Fact]
    public void Parse_WithArrayMetrics_ThrowsJsonException()
    {
        const string json = """
                            {
                              "AquariumId":"aq-1",
                              "Timestamp":"2026-02-24T14:56:54.991+00:00",
                              "Metrics":[
                                {"Type":1,"Value":24.3,"Unit":"C"}
                              ]
                            }
                            """;

        Assert.Throws<JsonException>(() => MeasurementMessageParser.Parse(json));
    }
}
