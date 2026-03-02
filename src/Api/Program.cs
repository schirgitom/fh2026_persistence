using Api.GraphQl;
using Api.Middleware;
using Application.Configuration;
using Infrastructure;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, _, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .WriteTo.Console()
            .WriteTo.File(
                path: "logs/log-.txt",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30);
    });

    builder.Services.Configure<MeasurementValidationOptions>(
        builder.Configuration.GetSection(MeasurementValidationOptions.SectionName));

    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services
        .AddGraphQLServer()
        .AddQueryType<MeasurementQueries>();

    var app = builder.Build();

    app.UseMiddleware<GlobalExceptionMiddleware>();
    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseSerilogRequestLogging();

    app.MapGraphQL("/graphql");
    app.MapGet("/", () => Results.Ok("Aquarium Persistence API"));

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application startup failed");
}
finally
{
    await Log.CloseAndFlushAsync();
}
