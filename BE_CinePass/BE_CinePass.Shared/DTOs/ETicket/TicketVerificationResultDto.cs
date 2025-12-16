namespace BE_CinePass.Shared.DTOs.ETicket;

public class TicketVerificationResultDto
{
    public bool IsValid { get; set; }
    public string Status { get; set; } = string.Empty; // "Valid", "Invalid", "AlreadyUsed"
    public string Message { get; set; } = string.Empty;
    public ETicketDetailDto? TicketDetail { get; set; }
}
