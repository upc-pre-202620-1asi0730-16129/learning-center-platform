using Acme.Center.Platform.Publishing.Domain.Model.ValueObjects;
using Acme.Center.Platform.Shared.Domain.Model.Events;

namespace Acme.Center.Platform.Publishing.Domain.Model.Events;

/// <summary>
///     Domain event raised when a tutorial is published and unlocked.
/// </summary>
/// <param name="TutorialId">The identifier of the published tutorial.</param>
public record TutorialPublishedEvent(TutorialId TutorialId) : IEvent;
