using Domain.Aggregates;

namespace Infrastructure.Persistence.Aggregates;

public interface IAggregateResolutionStrategy
{
    AggregateResolution Resolution { get; }
    string ViewSuffix { get; }
    string BucketInterval { get; }
    string PolicyStartOffset { get; }
    string PolicyEndOffset { get; }
    string PolicyScheduleInterval { get; }
    bool IncludeStandardDeviation { get; }
}
