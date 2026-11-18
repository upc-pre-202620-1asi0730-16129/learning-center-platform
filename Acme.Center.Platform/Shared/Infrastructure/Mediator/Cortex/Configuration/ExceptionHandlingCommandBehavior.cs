using Cortex.Mediator.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Acme.Center.Platform.Shared.Infrastructure.Mediator.Cortex.Configuration;

/// <summary>
///     Pipeline behavior that logs command execution exceptions and handles standard failure paths.
/// </summary>
public class ExceptionHandlingCommandBehavior<TCommand>(
    ILogger<ExceptionHandlingCommandBehavior<TCommand>> logger)
    : ICommandPipelineBehavior<TCommand> where TCommand : ICommand
{
    public async Task Handle(
        TCommand command,
        CommandHandlerDelegate next,
        CancellationToken ct)
    {
        try
        {
            await next();
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Command {CommandName} was cancelled.", typeof(TCommand).Name);
            throw;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database update error during command {CommandName}.", typeof(TCommand).Name);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled error during command {CommandName}.", typeof(TCommand).Name);
            throw;
        }
    }
}
