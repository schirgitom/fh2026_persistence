using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class MeasurementDbContext : DbContext
{
    public MeasurementDbContext(DbContextOptions<MeasurementDbContext> options)
        : base(options)
    {
    }

    public DbSet<MeasurementEntity> Measurements => Set<MeasurementEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MeasurementDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
