using Domain.Aggregates;

namespace Infrastructure.Persistence.Aggregates;

public sealed class FiveMinuteAggregateResolutionStrategy : IAggregateResolutionStrategy
{
    public AggregateResolution Resolution => AggregateResolution.FiveMinutes;
    public string ViewSuffix => "5m";
    public string BucketInterval => "5 minutes";
    public string PolicyStartOffset => "1 day";
    public string PolicyEndOffset => "5 minutes";
    public string PolicyScheduleInterval => "5 minutes";
    public bool IncludeStandardDeviation => false;
}
