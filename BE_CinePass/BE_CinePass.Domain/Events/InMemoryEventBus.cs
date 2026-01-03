using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BE_CinePass.Domain.Events;

public class InMemoryEventBus : IEventBus
{
    private readonly Dictionary<Type, List<Delegate>> _handlers = new();
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<InMemoryEventBus> _logger;

    public InMemoryEventBus(
        IServiceProvider serviceProvider,
        ILogger<InMemoryEventBus> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) 
        where TEvent : IDomainEvent
    {
        var eventType = typeof(TEvent);
        
        _logger.LogInformation(
            "Publishing event: {EventType} at {OccurredOn}", 
            eventType.Name, 
            @event.OccurredOn);

        if (_handlers.TryGetValue(eventType, out var handlers))
        {
            foreach (var handler in handlers)
            {
                try
                {
                    if (handler is Func<TEvent, Task> typedHandler)
                    {
                        await typedHandler(@event);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, 
                        "Error handling event {EventType}", 
                        eventType.Name);
                }
            }
        }

        // Also trigger IEventHandler<TEvent> implementations
        await TriggerEventHandlersAsync(@event, cancellationToken);
    }

    public void Subscribe<TEvent>(Func<TEvent, Task> handler) 
        where TEvent : IDomainEvent
    {
        var eventType = typeof(TEvent);
        
        if (!_handlers.ContainsKey(eventType))
        {
            _handlers[eventType] = new List<Delegate>();
        }

        _handlers[eventType].Add(handler);
        
        _logger.LogInformation(
            "Subscribed handler for event: {EventType}", 
            eventType.Name);
    }

    private async Task TriggerEventHandlersAsync<TEvent>(
        TEvent @event, 
        CancellationToken cancellationToken) 
        where TEvent : IDomainEvent
    {
        var handlerType = typeof(IEventHandler<>).MakeGenericType(typeof(TEvent));
        
        using var scope = _serviceProvider.CreateScope();
        var handlers = scope.ServiceProvider.GetServices(handlerType);

        foreach (var handler in handlers)
        {
            try
            {
                var handleMethod = handlerType.GetMethod("HandleAsync");
                if (handleMethod != null)
                {
                    var task = (Task?)handleMethod.Invoke(
                        handler, 
                        new object[] { @event, cancellationToken });
                    
                    if (task != null)
                    {
                        await task;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Error in event handler for {EventType}", 
                    typeof(TEvent).Name);
            }
        }
    }
}