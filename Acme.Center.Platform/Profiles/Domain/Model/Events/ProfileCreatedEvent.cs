using Acme.Center.Platform.Profiles.Domain.Model.ValueObjects;
using Acme.Center.Platform.Shared.Domain.Model.Events;

namespace Acme.Center.Platform.Profiles.Domain.Model.Events;

/// <summary>
///     Domain event raised when a new profile is created.
/// </summary>
/// <param name="ProfileId">The strongly-typed identifier of the created profile.</param>
/// <param name="Email">The email address of the created profile.</param>
public record ProfileCreatedEvent(ProfileId ProfileId, EmailAddress Email) : IEvent;
