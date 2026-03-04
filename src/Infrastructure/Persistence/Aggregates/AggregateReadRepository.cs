using Application.Abstractions.Persistence;
using Dapper;
using Domain.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Aggregates;

public sealed class AggregateReadRepository : IAggregateReadRepository
{
    private readonly IMeasurementDbContextFactory _dbContextFactory;
    private readonly ISqlIdentifierValidator _identifierValidator;
    private readonly IAggregateResolutionStrategyProvider _strategyProvider;
    private readonly ContinuousAggregateSqlBuilder _sqlBuilder;
    private readonly IContinuousAggregateService _continuousAggregateService;

    public AggregateReadRepository(
        IMeasurementDbContextFactory dbContextFactory,
        ISqlIdentifierValidator identifierValidator,
        IAggregateResolutionStrategyProvider strategyProvider,
        ContinuousAggregateSqlBuilder sqlBuilder,
        IContinuousAggregateService continuousAggregateService)
    {
        _dbContextFactory = dbContextFactory;
        _identifierValidator = identifierValidator;
        _strategyProvider = strategyProvider;
        _sqlBuilder = sqlBuilder;
        _continuousAggregateService = continuousAggregateService;
    }

    public async Task<IReadOnlyList<AggregateResult>> GetAggregatesAsync(
        string aquariumId,
        AggregateResolution resolution,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        var safeAquariumId = _identifierValidator.EnsureValidAquariumId(aquariumId);
        var strategy = _strategyProvider.Get(resolution);
        var viewName = _identifierValidator.BuildAggregateViewName(safeAquariumId, strategy.ViewSuffix);

        await _continuousAggregateService.EnsureAggregatesAsync(safeAquariumId, cancellationToken);

        await using var dbContext = await _dbContextFactory.CreateAsync(safeAquariumId, cancellationToken);
        var connection = dbContext.Database.GetDbConnection();

        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        var querySql = _sqlBuilder.BuildReadSql(viewName, strategy.IncludeStandardDeviation);
        var rows = await connection.QueryAsync<AggregateRow>(new CommandDefinition(
            querySql,
            new { From = from, To = to },
            cancellationToken: cancellationToken));

        return rows.Select(row => new AggregateResult(
                DateTime.SpecifyKind(row.Bucket, DateTimeKind.Utc),
                ToDouble(row.AvgTemperature),
                ToDouble(row.MinTemperature),
                ToDouble(row.MaxTemperature),
                row.StdDevTemperature,
                ToDouble(row.AvgPh),
                ToDouble(row.MinPh),
                ToDouble(row.MaxPh),
                row.StdDevPh,
                ToDouble(row.AvgOxygen),
                ToDouble(row.MinOxygen),
                ToDouble(row.MaxOxygen),
                ToDouble(row.AvgMg),
                ToDouble(row.AvgKh),
                ToDouble(row.AvgCa),
                ToDouble(row.AvgPump),
                ToDouble(row.MaxPump)))
            .ToList();
    }

    private static double? ToDouble(decimal? value) => value.HasValue ? (double)value.Value : null;

    // Use property-based materialization so Dapper does not require an exact ctor signature.
    private sealed class AggregateRow
    {
        public DateTime Bucket { get; set; }
        public decimal? AvgTemperature { get; set; }
        public decimal? MinTemperature { get; set; }
        public decimal? MaxTemperature { get; set; }
        public double? StdDevTemperature { get; set; }
        public decimal? AvgPh { get; set; }
        public decimal? MinPh { get; set; }
        public decimal? MaxPh { get; set; }
        public double? StdDevPh { get; set; }
        public decimal? AvgOxygen { get; set; }
        public decimal? MinOxygen { get; set; }
        public decimal? MaxOxygen { get; set; }
        public decimal? AvgMg { get; set; }
        public decimal? AvgKh { get; set; }
        public decimal? AvgCa { get; set; }
        public decimal? AvgPump { get; set; }
        public decimal? MaxPump { get; set; }
    }
}
