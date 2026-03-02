namespace Infrastructure.Persistence;

public sealed class MeasurementEntity
{
    public string AquariumId { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
    public decimal? Temperature { get; set; }
    public decimal? Mg { get; set; }
    public decimal? Kh { get; set; }
    public decimal? Ca { get; set; }
    public decimal? Ph { get; set; }
    public decimal? Oxygen { get; set; }
    public decimal? Pump { get; set; }
}
