namespace BE_CinePass.Shared.DTOs.PaymentTransaction;

public class PaymentTransactionResponseDto
{
    public Guid Id { get; set; }
    public Guid? OrderId { get; set; }
    public string? ProviderTransId { get; set; }
    public decimal? Amount { get; set; }
    public string? Status { get; set; }
    public string? ResponseJson { get; set; }
    public DateTime CreatedAt { get; set; }
}

