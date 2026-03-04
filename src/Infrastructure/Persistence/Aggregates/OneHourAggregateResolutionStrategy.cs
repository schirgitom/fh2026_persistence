using Domain.Aggregates;

namespace Infrastructure.Persistence.Aggregates;

public sealed class OneHourAggregateResolutionStrategy : IAggregateResolutionStrategy
{
    public AggregateResolution Resolution => AggregateResolution.OneHour;
    public string ViewSuffix => "1h";
    public string BucketInterval => "1 hour";
    public string PolicyStartOffset => "7 days";
    public string PolicyEndOffset => "1 hour";
    public string PolicyScheduleInterval => "1 hour";
    public bool IncludeStandardDeviation => true;
}
