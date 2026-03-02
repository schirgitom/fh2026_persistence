using Application.Abstractions.Persistence;
using Application.Configuration;
using Application.Contracts;
using Application.Measurements;
using Domain;
using Domain.Measurements;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace UnitTests.Application;

public sealed class IngestMeasurementCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithValidMeasurement_PersistsMeasurement()
    {
        var repository = new CapturingMeasurementRepository();
        var unitOfWork = new CapturingUnitOfWork();
        var handler = CreateHandler(repository, unitOfWork, futureToleranceSeconds: 30);

        var now = DateTimeOffset.UtcNow;
        var command = new IngestMeasurementCommand(
            new MeasurementDto(
                "aq-42",
                now,
                new[]
                {
                    new MetricValueDto(MetricType.Temperature, 24m, "C"),
                    new MetricValueDto(MetricType.Ph, 7.1m, "pH"),
                    new MetricValueDto(MetricType.Oxygen, 8m, "mg/l")
                }));

        await handler.HandleAsync(command, CancellationToken.None);

        Assert.NotNull(repository.LastMeasurement);
        Assert.Equal("aq-42", repository.LastMeasurement!.AquariumId);
        Assert.Equal(24m, repository.LastMeasurement.Temperature);
        Assert.Equal("aq-42", unitOfWork.LastAquariumId);
        Assert.True(unitOfWork.Executed);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidMeasurement_ThrowsAndDoesNotPersist()
    {
        var repository = new CapturingMeasurementRepository();
        var unitOfWork = new CapturingUnitOfWork();
        var handler = CreateHandler(repository, unitOfWork, futureToleranceSeconds: 0);
        var futureTimestamp = DateTimeOffset.UtcNow.AddSeconds(10);

        var command = new IngestMeasurementCommand(
            new MeasurementDto(
                "aq-42",
                futureTimestamp,
                new[]
                {
                    new MetricValueDto(MetricType.Temperature, 24m, "C"),
                    new MetricValueDto(MetricType.Ph, 7.1m, "pH"),
                    new MetricValueDto(MetricType.Oxygen, 8m, "mg/l")
                }));

        await Assert.ThrowsAsync<DomainValidationException>(() => handler.HandleAsync(command, CancellationToken.None));
        Assert.Null(repository.LastMeasurement);
        Assert.False(unitOfWork.Executed);
    }

    private static IngestMeasurementCommandHandler CreateHandler(
        IMeasurementRepository repository,
        IUnitOfWork unitOfWork,
        int futureToleranceSeconds)
    {
        return new IngestMeasurementCommandHandler(
            repository,
            unitOfWork,
            Options.Create(new MeasurementValidationOptions { FutureToleranceSeconds = futureToleranceSeconds }),
            NullLogger<IngestMeasurementCommandHandler>.Instance);
    }

    private sealed class CapturingMeasurementRepository : IMeasurementRepository
    {
        public Measurement? LastMeasurement { get; private set; }

        public Task UpsertAsync(Measurement measurement, CancellationToken cancellationToken)
        {
            LastMeasurement = measurement;
            return Task.CompletedTask;
        }
    }

    private sealed class CapturingUnitOfWork : IUnitOfWork
    {
        public string? LastAquariumId { get; private set; }
        public bool Executed { get; private set; }

        public async Task ExecuteAsync(
            string aquariumId,
            Func<CancellationToken, Task> operation,
            CancellationToken cancellationToken)
        {
            LastAquariumId = aquariumId;
            Executed = true;
            await operation(cancellationToken);
        }
    }
}
