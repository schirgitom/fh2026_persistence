namespace Infrastructure.Persistence;

public interface IMeasurementDbContextScope
{
    MeasurementDbContext? Current { get; }
    void Set(MeasurementDbContext context);
    void Clear();
}
