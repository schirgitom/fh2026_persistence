namespace Application.Abstractions.Cqrs;

public interface ICommandHandler<in TCommand>
{
    Task HandleAsync(TCommand command, CancellationToken cancellationToken);
}
