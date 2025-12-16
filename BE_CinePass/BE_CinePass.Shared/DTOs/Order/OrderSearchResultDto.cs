namespace BE_CinePass.Shared.DTOs.Order;

public class OrderSearchResultDto
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? PaymentMethod { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TicketCount { get; set; }
    public string MovieTitles { get; set; } = string.Empty;
}
