using Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence;

public sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly IMeasurementDbContextFactory _dbContextFactory;
    private readonly IMeasurementDbContextScope _dbContextScope;
    private readonly ILogger<EfUnitOfWork> _logger;

    public EfUnitOfWork(
        IMeasurementDbContextFactory dbContextFactory,
        IMeasurementDbContextScope dbContextScope,
        ILogger<EfUnitOfWork> logger)
    {
        _dbContextFactory = dbContextFactory;
        _dbContextScope = dbContextScope;
        _logger = logger;
    }

    public async Task ExecuteAsync(
        string aquariumId,
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken)
    {
        await using var dbContext = await _dbContextFactory.CreateAsync(aquariumId, cancellationToken);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        _dbContextScope.Set(dbContext);

        try
        {
            _logger.LogInformation("Starting unit of work for aquarium {AquariumId}", aquariumId);

            await operation(cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("Unit of work committed for aquarium {AquariumId}", aquariumId);
        }
        catch (Exception ex)
        {
            await RollbackAsync(transaction, cancellationToken);
            _logger.LogError(ex, "Unit of work rolled back for aquarium {AquariumId}", aquariumId);
            throw;
        }
        finally
        {
            _dbContextScope.Clear();
        }
    }

    private static async Task RollbackAsync(IDbContextTransaction transaction, CancellationToken cancellationToken)
    {
        try
        {
            await transaction.RollbackAsync(cancellationToken);
        }
        catch
        {
            // Intentionally swallow rollback exceptions to preserve original failure.
        }
    }
}
