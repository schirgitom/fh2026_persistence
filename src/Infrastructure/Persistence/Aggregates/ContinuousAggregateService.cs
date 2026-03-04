using Application.Abstractions.Persistence;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Persistence.Aggregates;

public sealed class ContinuousAggregateService : IContinuousAggregateService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMeasurementDbContextScope _dbContextScope;
    private readonly ISqlIdentifierValidator _identifierValidator;
    private readonly IEnumerable<IAggregateResolutionStrategy> _strategies;
    private readonly ContinuousAggregateSqlBuilder _sqlBuilder;

    public ContinuousAggregateService(
        IUnitOfWork unitOfWork,
        IMeasurementDbContextScope dbContextScope,
        ISqlIdentifierValidator identifierValidator,
        IEnumerable<IAggregateResolutionStrategy> strategies,
        ContinuousAggregateSqlBuilder sqlBuilder)
    {
        _unitOfWork = unitOfWork;
        _dbContextScope = dbContextScope;
        _identifierValidator = identifierValidator;
        _strategies = strategies;
        _sqlBuilder = sqlBuilder;
    }

    public async Task EnsureAggregatesAsync(string aquariumId, CancellationToken cancellationToken)
    {
        var safeAquariumId = _identifierValidator.EnsureValidAquariumId(aquariumId);

        await _unitOfWork.ExecuteAsync(
            safeAquariumId,
            async token =>
            {
                var dbContext = _dbContextScope.Current
                    ?? throw new InvalidOperationException("No active DbContext in current unit of work.");

                var connection = dbContext.Database.GetDbConnection();
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync(token);
                }

                var transaction = dbContext.Database.CurrentTransaction?.GetDbTransaction()
                    ?? throw new InvalidOperationException("No active database transaction.");

                var sourceTableName = _identifierValidator.BuildMeasurementTableName(safeAquariumId);

                foreach (var strategy in _strategies)
                {
                    var viewName = _identifierValidator.BuildAggregateViewName(safeAquariumId, strategy.ViewSuffix);

                    var createViewSql = _sqlBuilder.BuildCreateViewSql(
                        sourceTableName,
                        viewName,
                        strategy.BucketInterval,
                        safeAquariumId,
                        strategy.IncludeStandardDeviation);

                    await connection.ExecuteAsync(new CommandDefinition(
                        createViewSql,
                        parameters: null,
                        transaction,
                        cancellationToken: token));

                    var policySql = _sqlBuilder.BuildPolicySql();
                    await connection.ExecuteAsync(new CommandDefinition(
                        policySql,
                        new
                        {
                            ViewName = viewName,
                            StartOffset = strategy.PolicyStartOffset,
                            EndOffset = strategy.PolicyEndOffset,
                            ScheduleInterval = strategy.PolicyScheduleInterval
                        },
                        transaction,
                        cancellationToken: token));
                }
            },
            cancellationToken);
    }
}
