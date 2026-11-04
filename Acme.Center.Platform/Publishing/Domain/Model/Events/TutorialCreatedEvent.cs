using Acme.Center.Platform.Publishing.Domain.Model.ValueObjects;
using Acme.Center.Platform.Shared.Domain.Model.Events;

namespace Acme.Center.Platform.Publishing.Domain.Model.Events;

/// <summary>
///     Domain event raised when a new tutorial is created.
/// </summary>
/// <param name="TutorialId">The identifier of the created tutorial.</param>
/// <param name="Title">The title of the created tutorial.</param>
/// <param name="CategoryId">The category identifier associated with the tutorial.</param>
public record TutorialCreatedEvent(TutorialId TutorialId, string Title, CategoryId CategoryId) : IEvent;
