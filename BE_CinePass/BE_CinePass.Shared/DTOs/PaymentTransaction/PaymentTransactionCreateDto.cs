using System.ComponentModel.DataAnnotations;

namespace BE_CinePass.Shared.DTOs.PaymentTransaction;

public class PaymentTransactionCreateDto
{
    [Required]
    public Guid OrderId { get; set; }

    [MaxLength(255)]
    public string? ProviderTransId { get; set; }

    [Range(0.01, 999999999.99)]
    public decimal? Amount { get; set; }

    [MaxLength(50)]
    public string? Status { get; set; }

    public string? ResponseJson { get; set; } // JSON string
}

