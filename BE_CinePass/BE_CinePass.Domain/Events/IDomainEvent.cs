namespace BE_CinePass.Domain.Events;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}