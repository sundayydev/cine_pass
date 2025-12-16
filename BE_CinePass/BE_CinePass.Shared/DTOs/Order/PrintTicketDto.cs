namespace BE_CinePass.Shared.DTOs.Order;

public class PrintTicketDto
{
    public string? StaffNote { get; set; }
    public string PrintReason { get; set; } = "Customer request";
}
