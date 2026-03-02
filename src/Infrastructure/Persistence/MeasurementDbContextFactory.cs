using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence;

public sealed class MeasurementDbContextFactory : IMeasurementDbContextFactory
{
    private readonly ITenantConnectionStringResolver _connectionStringResolver;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<MeasurementDbContextFactory> _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly HashSet<string> _initializedTenants = new(StringComparer.OrdinalIgnoreCase);

    public MeasurementDbContextFactory(
        ITenantConnectionStringResolver connectionStringResolver,
        ILoggerFactory loggerFactory,
        ILogger<MeasurementDbContextFactory> logger)
    {
        _connectionStringResolver = connectionStringResolver;
        _loggerFactory = loggerFactory;
        _logger = logger;
    }

    public async Task<MeasurementDbContext> CreateAsync(string aquariumId, CancellationToken cancellationToken)
    {
        var connectionString = _connectionStringResolver.Resolve(aquariumId);
        var options = new DbContextOptionsBuilder<MeasurementDbContext>()
            .UseNpgsql(connectionString)
            .UseLoggerFactory(_loggerFactory)
            .EnableDetailedErrors()
            .Options;

        var context = new MeasurementDbContext(options);
        await EnsureTenantInitializedAsync(aquariumId, context, cancellationToken);
        return context;
    }

    private async Task EnsureTenantInitializedAsync(
        string aquariumId,
        MeasurementDbContext context,
        CancellationToken cancellationToken)
    {
        if (_initializedTenants.Contains(aquariumId))
        {
            return;
        }

        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            if (_initializedTenants.Contains(aquariumId))
            {
                return;
            }

            _logger.LogInformation("Applying EF Core migrations for aquarium {AquariumId}", aquariumId);
            await context.Database.MigrateAsync(cancellationToken);
            _initializedTenants.Add(aquariumId);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
