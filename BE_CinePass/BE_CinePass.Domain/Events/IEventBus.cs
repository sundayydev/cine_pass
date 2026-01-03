namespace BE_CinePass.Domain.Events;

public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) 
        where TEvent : IDomainEvent;
    
    void Subscribe<TEvent>(Func<TEvent, Task> handler) 
        where TEvent : IDomainEvent;
}