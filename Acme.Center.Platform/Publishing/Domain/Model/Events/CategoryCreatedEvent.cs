using Acme.Center.Platform.Shared.Domain.Model.Events;

namespace Acme.Center.Platform.Publishing.Domain.Model.Events;

public class CategoryCreatedEvent(string name) : IEvent
{
    public string Name { get; } = name;
}