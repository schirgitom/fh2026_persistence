namespace Infrastructure.Persistence;

public sealed class MeasurementDbContextScope : IMeasurementDbContextScope
{
    private MeasurementDbContext? _current;

    public MeasurementDbContext? Current => _current;

    public void Set(MeasurementDbContext context)
    {
        _current = context;
    }

    public void Clear()
    {
        _current = null;
    }
}
