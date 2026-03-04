namespace Infrastructure.Persistence.Aggregates;

public sealed class ContinuousAggregateSqlBuilder
{
    public string BuildCreateViewSql(
        string sourceTableName,
        string viewName,
        string bucketInterval,
        string aquariumId,
        bool includeStandardDeviation)
    {
        var stdDevSelection = includeStandardDeviation
            ? ",\n       stddev(temperature) AS stddev_temperature,\n       stddev(ph) AS stddev_ph"
            : string.Empty;
        var escapedAquariumId = aquariumId.Replace("'", "''", StringComparison.Ordinal);

        return $"""
CREATE MATERIALIZED VIEW IF NOT EXISTS {viewName}
WITH (timescaledb.continuous) AS
SELECT time_bucket(INTERVAL '{bucketInterval}', "timestamp") AS bucket,
       avg(temperature) AS avg_temperature,
       min(temperature) AS min_temperature,
       max(temperature) AS max_temperature,
       avg(ph) AS avg_ph,
       min(ph) AS min_ph,
       max(ph) AS max_ph,
       avg(oxygen) AS avg_oxygen,
       min(oxygen) AS min_oxygen,
       max(oxygen) AS max_oxygen,
       avg(mg) AS avg_mg,
       avg(kh) AS avg_kh,
       avg(ca) AS avg_ca,
       avg(pump) AS avg_pump,
       max(pump) AS max_pump{stdDevSelection}
FROM {sourceTableName}
WHERE aquarium_id = '{escapedAquariumId}'
GROUP BY bucket
WITH NO DATA;
""";
    }

    public string BuildPolicySql()
    {
        return """
SELECT add_continuous_aggregate_policy(
    @ViewName,
    start_offset => @StartOffset::interval,
    end_offset => @EndOffset::interval,
    schedule_interval => @ScheduleInterval::interval,
    if_not_exists => TRUE);
""";
    }

    public string BuildReadSql(string viewName, bool includeStandardDeviation)
    {
        var stdDevTemperatureSelection = includeStandardDeviation
            ? "stddev_temperature AS \"StdDevTemperature\","
            : "NULL::double precision AS \"StdDevTemperature\",";
        var stdDevPhSelection = includeStandardDeviation
            ? "stddev_ph AS \"StdDevPh\","
            : "NULL::double precision AS \"StdDevPh\",";

        return $"""
SELECT bucket AS "Bucket",
       avg_temperature AS "AvgTemperature",
       min_temperature AS "MinTemperature",
       max_temperature AS "MaxTemperature",
       {stdDevTemperatureSelection}
       avg_ph AS "AvgPh",
       min_ph AS "MinPh",
       max_ph AS "MaxPh",
       {stdDevPhSelection}
       avg_oxygen AS "AvgOxygen",
       min_oxygen AS "MinOxygen",
       max_oxygen AS "MaxOxygen",
       avg_mg AS "AvgMg",
       avg_kh AS "AvgKh",
       avg_ca AS "AvgCa",
       avg_pump AS "AvgPump",
       max_pump AS "MaxPump"
FROM {viewName}
WHERE bucket >= @From AND bucket <= @To
ORDER BY bucket;
""";
    }
}
