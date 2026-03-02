namespace Infrastructure.Persistence;

public interface ITenantConnectionStringResolver
{
    string Resolve(string aquariumId);
}
