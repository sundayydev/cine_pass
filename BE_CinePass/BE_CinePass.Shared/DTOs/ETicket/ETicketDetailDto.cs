using BE_CinePass.Shared.DTOs.Order;

namespace BE_CinePass.Shared.DTOs.ETicket;

public class ETicketDetailDto
{
    public Guid Id { get; set; }
    public string TicketCode { get; set; } = string.Empty;
    public string? QrData { get; set; }
    public bool IsUsed { get; set; }
    public DateTime? UsedAt { get; set; }
    public OrderTicketDetailDto OrderTicket { get; set; } = null!;
}

