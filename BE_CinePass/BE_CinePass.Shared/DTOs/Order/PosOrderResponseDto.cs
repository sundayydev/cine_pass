namespace BE_CinePass.Shared.DTOs.Order;

/// <summary>
/// Response DTO for POS order creation - contains all ticket printing data
/// </summary>
public class PosOrderResponseDto
{
    /// <summary>
    /// Thông tin đơn hàng đầy đủ
    /// </summary>
    public OrderDetailDto OrderDetail { get; set; } = null!;

    /// <summary>
    /// Thông tin thanh toán
    /// </summary>
    public PosPaymentInfo PaymentInfo { get; set; } = null!;

    /// <summary>
    /// Thông tin khách hàng
    /// </summary>
    public PosCustomerInfo CustomerInfo { get; set; } = null!;
}

/// <summary>
/// Thông tin thanh toán tiền mặt
/// </summary>
public class PosPaymentInfo
{
    /// <summary>
    /// Tổng tiền
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Số tiền khách đưa
    /// </summary>
    public decimal CashReceived { get; set; }

    /// <summary>
    /// Tiền thừa trả lại
    /// </summary>
    public decimal ChangeAmount { get; set; }

    /// <summary>
    /// Phương thức thanh toán
    /// </summary>
    public string PaymentMethod { get; set; } = "CASH";

    /// <summary>
    /// Thời gian thanh toán
    /// </summary>
    public DateTime PaymentTime { get; set; }

    /// <summary>
    /// Mã giao dịch
    /// </summary>
    public Guid TransactionId { get; set; }
}

/// <summary>
/// Thông tin khách hàng
/// </summary>
public class PosCustomerInfo
{
    /// <summary>
    /// Tên khách hàng
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Số điện thoại
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Email
    /// </summary>
    public string? Email { get; set; }
}
