using Acme.Center.Platform.Publishing.Domain.Model.Events;
using Acme.Center.Platform.Shared.Application.Internal.EventHandlers;
using Microsoft.Extensions.Logging;

namespace Acme.Center.Platform.Publishing.Application.Internal.EventHandlers;

/// <summary>
///     Event handler for <see cref="TutorialCreatedEvent" />.
/// </summary>
public class TutorialCreatedEventHandler(ILogger<TutorialCreatedEventHandler> logger) : IEventHandler<TutorialCreatedEvent>
{
    /// <inheritdoc />
    public Task Handle(TutorialCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation("Tutorial created: {Title} (ID: {TutorialId}, CategoryID: {CategoryId})",
            domainEvent.Title, domainEvent.TutorialId.Value, domainEvent.CategoryId.Value);

        return Task.CompletedTask;
    }
}
