namespace Application.Abstractions.Persistence;

public interface IUnitOfWork
{
    Task ExecuteAsync(
        string aquariumId,
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken);
}
