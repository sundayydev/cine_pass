namespace BE_CinePass.Shared.DTOs.ETicket;

public class ETicketResponseDto
{
    public Guid Id { get; set; }
    public Guid OrderTicketId { get; set; }
    public string TicketCode { get; set; } = string.Empty;
    public string? QrData { get; set; }
    public bool IsUsed { get; set; }
    public DateTime? UsedAt { get; set; }
}

