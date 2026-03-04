namespace Infrastructure.Persistence.Aggregates;

public sealed class SqlIdentifierValidator : ISqlIdentifierValidator
{
    public string EnsureValidAquariumId(string aquariumId)
    {
        if (string.IsNullOrWhiteSpace(aquariumId))
        {
            throw new ArgumentException("AquariumId must be provided.", nameof(aquariumId));
        }

        return aquariumId.Trim();
    }

    public string BuildMeasurementTableName(string aquariumId)
    {
        EnsureValidAquariumId(aquariumId);
        return "measurements";
    }

    public string BuildAggregateViewName(string aquariumId, string viewSuffix)
    {
        var validAquariumId = EnsureValidAquariumId(aquariumId);
        var normalizedAquariumId = NormalizeForSqlIdentifier(validAquariumId);
        return $"measurement_{normalizedAquariumId}_{viewSuffix}";
    }

    private static string NormalizeForSqlIdentifier(string value)
    {
        Span<char> buffer = stackalloc char[value.Length];
        var index = 0;

        foreach (var c in value)
        {
            if (char.IsLetterOrDigit(c) || c == '_')
            {
                buffer[index++] = char.ToLowerInvariant(c);
            }
            else
            {
                buffer[index++] = '_';
            }
        }

        return new string(buffer.Slice(0, index));
    }
}
