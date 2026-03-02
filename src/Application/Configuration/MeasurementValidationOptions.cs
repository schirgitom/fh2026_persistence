namespace Application.Configuration;

public sealed class MeasurementValidationOptions
{
    public const string SectionName = "MeasurementValidation";

    public int FutureToleranceSeconds { get; set; } = 30;
}
