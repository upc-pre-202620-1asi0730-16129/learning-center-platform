using Acme.Center.Platform.Shared.Domain.Model.Events;

namespace Acme.Center.Platform.Shared.Domain.Model.Aggregates.Internal;

/// <summary>
///     Base class for aggregate roots in Domain-Driven Design.
///     Encapsulates domain event collection and management.
/// </summary>
public abstract class AggregateRoot : IAggregateRoot
{
    private readonly List<IEvent> _domainEvents = [];

    /// <inheritdoc />
    public IReadOnlyCollection<IEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    ///     Records a domain event to be dispatched when changes are committed.
    /// </summary>
    /// <param name="domainEvent">The domain event to record.</param>
    protected void AddDomainEvent(IEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <inheritdoc />
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
