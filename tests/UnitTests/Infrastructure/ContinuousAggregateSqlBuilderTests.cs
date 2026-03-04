using Infrastructure.Persistence.Aggregates;

namespace UnitTests.Infrastructure;

public sealed class ContinuousAggregateSqlBuilderTests
{
    private readonly ContinuousAggregateSqlBuilder _sqlBuilder = new();

    [Fact]
    public void BuildCreateViewSql_ForOneHour_IncludesStdDev()
    {
        var sql = _sqlBuilder.BuildCreateViewSql(
            "measurements",
            "measurement_aq1_1h",
            "1 hour",
            "aq1",
            includeStandardDeviation: true);

        Assert.Contains("stddev(temperature)", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("stddev(ph)", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("time_bucket(INTERVAL '1 hour', \"timestamp\")", sql, StringComparison.Ordinal);
        Assert.Contains("WHERE aquarium_id = 'aq1'", sql, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildReadSql_ForFiveMinutes_UsesNullStdDevColumns()
    {
        var sql = _sqlBuilder.BuildReadSql("measurement_aq1_5m", includeStandardDeviation: false);

        Assert.Contains("NULL::double precision AS \"StdDevTemperature\"", sql, StringComparison.Ordinal);
        Assert.Contains("NULL::double precision AS \"StdDevPh\"", sql, StringComparison.Ordinal);
        Assert.DoesNotContain("stddev_temperature AS \"StdDevTemperature\"", sql, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildPolicySql_UsesParameterizedIntervals()
    {
        var sql = _sqlBuilder.BuildPolicySql();

        Assert.Contains("@StartOffset::interval", sql, StringComparison.Ordinal);
        Assert.Contains("@EndOffset::interval", sql, StringComparison.Ordinal);
        Assert.Contains("@ScheduleInterval::interval", sql, StringComparison.Ordinal);
    }
}
