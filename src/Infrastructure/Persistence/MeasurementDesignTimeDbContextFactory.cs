using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Persistence;

public sealed class MeasurementDesignTimeDbContextFactory : IDesignTimeDbContextFactory<MeasurementDbContext>
{
    public MeasurementDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("MIGRATIONS_CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=aquarium_migrations;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<MeasurementDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new MeasurementDbContext(options);
    }
}
