using Acme.Center.Platform.Publishing.Domain.Model.Events;
using Acme.Center.Platform.Shared.Application.Internal.EventHandlers;
using Microsoft.Extensions.Logging;

namespace Acme.Center.Platform.Publishing.Application.Internal.EventHandlers;

/// <summary>
///     Event handler for <see cref="CategoryCreatedEvent" />.
/// </summary>
public class CategoryCreatedEventHandler(ILogger<CategoryCreatedEventHandler> logger) : IEventHandler<CategoryCreatedEvent>
{
    /// <inheritdoc />
    public Task Handle(CategoryCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation("Created Category: {Name}", domainEvent.Name);
        return Task.CompletedTask;
    }
}