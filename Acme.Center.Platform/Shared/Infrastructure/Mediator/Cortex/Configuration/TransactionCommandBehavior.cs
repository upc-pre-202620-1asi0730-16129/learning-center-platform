using Acme.Center.Platform.Shared.Domain.Repositories;
using Cortex.Mediator.Commands;
using Microsoft.Extensions.Logging;

namespace Acme.Center.Platform.Shared.Infrastructure.Mediator.Cortex.Configuration;

/// <summary>
///     Pipeline behavior that automatically commits the unit of work after a command executes successfully.
/// </summary>
public class TransactionCommandBehavior<TCommand>(
    IUnitOfWork unitOfWork,
    ILogger<TransactionCommandBehavior<TCommand>> logger)
    : ICommandPipelineBehavior<TCommand> where TCommand : ICommand
{
    public async Task Handle(
        TCommand command,
        CommandHandlerDelegate next,
        CancellationToken ct)
    {
        await next();
        logger.LogDebug("Committing unit of work for {CommandType}", typeof(TCommand).Name);
        await unitOfWork.CompleteAsync(ct);
    }
}
