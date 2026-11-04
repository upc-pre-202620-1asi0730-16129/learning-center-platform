using Acme.Center.Platform.Shared.Domain.Model.Events;

namespace Acme.Center.Platform.Shared.Domain.Model.Aggregates;

/// <summary>
///     Defines the contract for an aggregate root capable of raising domain events.
/// </summary>
public interface IAggregateRoot
{
    /// <summary>
    ///     Gets the read-only collection of domain events recorded by this aggregate root.
    /// </summary>
    IReadOnlyCollection<IEvent> DomainEvents { get; }

    /// <summary>
    ///     Clears all recorded domain events from the aggregate root.
    /// </summary>
    void ClearDomainEvents();
}
