namespace BE_CinePass.Domain.Events;

public class SystemMaintenanceEvent : IDomainEvent
{
    public DateTime MaintenanceStart { get; set; }
    public DateTime MaintenanceEnd { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
}