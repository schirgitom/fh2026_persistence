namespace Infrastructure.Persistence.Aggregates;

public interface ISqlIdentifierValidator
{
    string EnsureValidAquariumId(string aquariumId);
    string BuildMeasurementTableName(string aquariumId);
    string BuildAggregateViewName(string aquariumId, string viewSuffix);
}
