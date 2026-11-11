using Acme.Center.Platform.Iam.Domain.Model.Events;
using Acme.Center.Platform.Shared.Application.Internal.EventHandlers;
using Microsoft.Extensions.Logging;

namespace Acme.Center.Platform.Profiles.Application.Internal.EventHandlers;

/// <summary>
///     Event handler that responds to <see cref="UserRegisteredEvent" /> in the Profiles bounded context.
///     Enables asynchronous cross-context synchronization without direct coupling.
/// </summary>
public class UserRegisteredEventHandler(ILogger<UserRegisteredEventHandler> logger) : IEventHandler<UserRegisteredEvent>
{
    /// <inheritdoc />
    public Task Handle(UserRegisteredEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing UserRegisteredEvent for User ID {UserId} ({Username}) in Profiles context",
            domainEvent.UserId.Value, domainEvent.Username);

        return Task.CompletedTask;
    }
}
