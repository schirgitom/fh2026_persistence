using Domain.Aggregates;

namespace Infrastructure.Persistence.Aggregates;

public sealed class OneDayAggregateResolutionStrategy : IAggregateResolutionStrategy
{
    public AggregateResolution Resolution => AggregateResolution.OneDay;
    public string ViewSuffix => "1d";
    public string BucketInterval => "1 day";
    public string PolicyStartOffset => "30 days";
    public string PolicyEndOffset => "1 day";
    public string PolicyScheduleInterval => "1 day";
    public bool IncludeStandardDeviation => false;
}
