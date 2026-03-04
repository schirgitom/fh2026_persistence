using Application.Abstractions.Cqrs;
using Application.Abstractions.Persistence;
using Application.Abstractions.Services;
using Application.Aggregates;
using Application.Measurements;
using Infrastructure.Configuration;
using Infrastructure.Messaging;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Aggregates;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TenantDatabaseOptions>(configuration.GetSection(TenantDatabaseOptions.SectionName));
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));

        services.AddSingleton<ITenantConnectionStringResolver, TenantConnectionStringResolver>();
        services.AddSingleton<IMeasurementDbContextFactory, MeasurementDbContextFactory>();

        services.AddScoped<IMeasurementDbContextScope, MeasurementDbContextScope>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<IMeasurementRepository, MeasurementRepository>();
        services.AddScoped<IMeasurementReadRepository, MeasurementRepository>();
        services.AddScoped<IAggregateReadRepository, AggregateReadRepository>();
        services.AddScoped<IAggregateQueryService, AggregateQueryService>();
        services.AddScoped<IContinuousAggregateService, ContinuousAggregateService>();
        services.AddSingleton<ISqlIdentifierValidator, SqlIdentifierValidator>();
        services.AddSingleton<ContinuousAggregateSqlBuilder>();
        services.AddSingleton<IAggregateResolutionStrategy, FiveMinuteAggregateResolutionStrategy>();
        services.AddSingleton<IAggregateResolutionStrategy, OneHourAggregateResolutionStrategy>();
        services.AddSingleton<IAggregateResolutionStrategy, OneDayAggregateResolutionStrategy>();
        services.AddSingleton<IAggregateResolutionStrategyProvider, AggregateResolutionStrategyProvider>();
        services.AddScoped<ICommandHandler<IngestMeasurementCommand>, IngestMeasurementCommandHandler>();

        services.AddHostedService<RabbitMqMeasurementConsumer>();

        return services;
    }
}
