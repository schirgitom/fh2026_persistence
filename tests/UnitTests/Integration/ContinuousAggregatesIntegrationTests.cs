using Application.Abstractions.Persistence;
using Domain.Aggregates;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using Testcontainers.PostgreSql;

namespace UnitTests.Integration;

public sealed class ContinuousAggregatesIntegrationTests
{
    [Fact]
    public async Task EnsureAndReadAggregates_WithTimescaleDb_ProducesStdDevOnlyForOneHour()
    {
        var container = await StartTimescaleContainerOrNullAsync();
        if (container is null)
        {
            return;
        }

        await using var _ = container;

        var connectionString = container.GetConnectionString();
        await SetupDatabaseAsync(connectionString);

        var factory = new TestMeasurementDbContextFactory(connectionString);
        var scope = new MeasurementDbContextScope();
        var unitOfWork = new EfUnitOfWork(factory, scope, NullLogger<EfUnitOfWork>.Instance);

        var strategies = new IAggregateResolutionStrategy[]
        {
            new FiveMinuteAggregateResolutionStrategy(),
            new OneHourAggregateResolutionStrategy(),
            new OneDayAggregateResolutionStrategy()
        };

        var validator = new SqlIdentifierValidator();
        var sqlBuilder = new ContinuousAggregateSqlBuilder();
        var continuousService = new ContinuousAggregateService(unitOfWork, scope, validator, strategies, sqlBuilder);
        var strategyProvider = new AggregateResolutionStrategyProvider(strategies);
        var repository = new AggregateReadRepository(factory, validator, strategyProvider, sqlBuilder, continuousService);

        var from = DateTimeOffset.UtcNow.AddHours(-3);
        var to = DateTimeOffset.UtcNow;

        await continuousService.EnsureAggregatesAsync("aq1", CancellationToken.None);
        await RefreshContinuousAggregateAsync(connectionString, "measurement_aq1_1h", from, to);
        await RefreshContinuousAggregateAsync(connectionString, "measurement_aq1_5m", from, to);

        var oneHour = await repository.GetAggregatesAsync("aq1", AggregateResolution.OneHour, from, to, CancellationToken.None);
        var fiveMinutes = await repository.GetAggregatesAsync("aq1", AggregateResolution.FiveMinutes, from, to, CancellationToken.None);

        Assert.NotEmpty(oneHour);
        Assert.Contains(oneHour, x => x.StdDevTemperature is not null);
        Assert.Contains(oneHour, x => x.StdDevPh is not null);

        Assert.NotEmpty(fiveMinutes);
        Assert.All(fiveMinutes, x => Assert.Null(x.StdDevTemperature));
        Assert.All(fiveMinutes, x => Assert.Null(x.StdDevPh));
    }

    private static async Task SetupDatabaseAsync(string connectionString)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        var now = DateTimeOffset.UtcNow;

        await using var command = connection.CreateCommand();
        command.CommandText = """
CREATE EXTENSION IF NOT EXISTS timescaledb;

CREATE TABLE IF NOT EXISTS measurements(
    aquarium_id text NOT NULL,
    timestamp timestamptz NOT NULL,
    temperature double precision,
    mg double precision,
    kh double precision,
    ca double precision,
    ph double precision,
    oxygen double precision,
    pump double precision,
    PRIMARY KEY (aquarium_id, timestamp)
);

SELECT create_hypertable('measurements', 'timestamp', 'aquarium_id', 4, if_not_exists => TRUE);

INSERT INTO measurements(aquarium_id, timestamp, temperature, mg, kh, ca, ph, oxygen, pump)
VALUES
    ('aq1', @t1, 24.0, 1300, 7.2, 420, 8.10, 7.8, 60),
    ('aq1', @t2, 25.0, 1310, 7.3, 421, 8.30, 7.7, 70),
    ('aq1', @t3, 26.0, 1290, 7.1, 419, 8.20, 7.9, 80),
    ('aq1', @t4, 24.5, 1280, 7.0, 418, 8.00, 8.0, 75);
""";

        command.Parameters.AddWithValue("t1", now.AddMinutes(-120));
        command.Parameters.AddWithValue("t2", now.AddMinutes(-90));
        command.Parameters.AddWithValue("t3", now.AddMinutes(-60));
        command.Parameters.AddWithValue("t4", now.AddMinutes(-30));

        await command.ExecuteNonQueryAsync();
    }

    private static async Task RefreshContinuousAggregateAsync(
        string connectionString,
        string viewName,
        DateTimeOffset from,
        DateTimeOffset to)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "CALL refresh_continuous_aggregate(@view::regclass, @from, @to);";
        command.Parameters.AddWithValue("view", viewName);
        command.Parameters.AddWithValue("from", from);
        command.Parameters.AddWithValue("to", to);

        await command.ExecuteNonQueryAsync();
    }

    private static async Task<PostgreSqlContainer?> StartTimescaleContainerOrNullAsync()
    {
        try
        {
            var container = new PostgreSqlBuilder()
                .WithImage("timescale/timescaledb:2.17.2-pg16")
                .WithDatabase("postgres")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .Build();

            await container.StartAsync();
            return container;
        }
        catch (Exception ex) when (IsDockerUnavailable(ex))
        {
            return null;
        }
    }

    private static bool IsDockerUnavailable(Exception ex)
    {
        if (ex is ArgumentException && ex.Message.Contains("Docker", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (ex.InnerException is null)
        {
            return false;
        }

        return IsDockerUnavailable(ex.InnerException);
    }

    private sealed class TestMeasurementDbContextFactory : IMeasurementDbContextFactory
    {
        private readonly string _connectionString;

        public TestMeasurementDbContextFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public Task<MeasurementDbContext> CreateAsync(string aquariumId, CancellationToken cancellationToken)
        {
            var options = new DbContextOptionsBuilder<MeasurementDbContext>()
                .UseNpgsql(_connectionString)
                .Options;

            return Task.FromResult(new MeasurementDbContext(options));
        }
    }
}
