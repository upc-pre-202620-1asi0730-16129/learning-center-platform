using Acme.Center.Platform.Shared.Domain.Model.Events;
using Cortex.Mediator.Notifications;

namespace Acme.Center.Platform.Shared.Application.Internal.EventHandlers;

public interface IEventHandler<in TEvent> : INotificationHandler<TEvent> where TEvent : IEvent
{
}