namespace Infrastructure.Configuration;

public sealed class TenantDatabaseOptions
{
    public const string SectionName = "TenantDatabase";

    public string ConnectionStringTemplate { get; set; } = string.Empty;
}
