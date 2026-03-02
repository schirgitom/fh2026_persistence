using Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace Infrastructure.Persistence;

public sealed class TenantConnectionStringResolver : ITenantConnectionStringResolver
{
    private readonly TenantDatabaseOptions _options;

    public TenantConnectionStringResolver(IOptions<TenantDatabaseOptions> options)
    {
        _options = options.Value;
    }

    public string Resolve(string aquariumId)
    {
        if (string.IsNullOrWhiteSpace(_options.ConnectionStringTemplate))
        {
            throw new InvalidOperationException("Tenant connection string template is not configured.");
        }

        var safeAquariumId = aquariumId.Trim().ToLowerInvariant();
        foreach (var c in Path.GetInvalidFileNameChars())
        {
            safeAquariumId = safeAquariumId.Replace(c, '_');
        }

        safeAquariumId = safeAquariumId.Replace('-', '_').Replace(' ', '_');
        return _options.ConnectionStringTemplate.Replace("{aquariumId}", safeAquariumId, StringComparison.OrdinalIgnoreCase);
    }
}
