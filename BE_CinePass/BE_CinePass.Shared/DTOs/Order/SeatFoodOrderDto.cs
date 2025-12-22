using System.ComponentModel.DataAnnotations;

namespace BE_CinePass.Shared.DTOs.Order;

/// <summary>
/// DTO để tạo đơn hàng đồ ăn/nước uống từ ghế ngồi trong rạp qua QR code
/// Khách hàng quét mã QR trên ghế để order đồ ăn/nước uống trực tiếp đến ghế
/// </summary>
public class SeatFoodOrderCreateDto
{
    /// <summary>
    /// Mã QR ordering của ghế (6 ký tự, được in trên ghế)
    /// </summary>
    [Required(ErrorMessage = "Mã QR ghế là bắt buộc")]
    [MaxLength(20)]
    public string SeatQrCode { get; set; } = string.Empty;

    /// <summary>
    /// Danh sách sản phẩm (đồ ăn, nước uống) cần order
    /// </summary>
    [Required(ErrorMessage = "Cần có ít nhất 1 sản phẩm")]
    [MinLength(1, ErrorMessage = "Cần có ít nhất 1 sản phẩm")]
    public List<SeatFoodOrderItemDto> Items { get; set; } = new();

    /// <summary>
    /// Ghi chú đặc biệt (ít đá, không đường, v.v.)
    /// </summary>
    [MaxLength(500)]
    public string? Note { get; set; }

    /// <summary>
    /// User ID (nếu đã đăng nhập)
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Phương thức thanh toán: MOMO, CASH, CARD
    /// </summary>
    [MaxLength(20)]
    public string PaymentMethod { get; set; } = "MOMO";
}

/// <summary>
/// Chi tiết sản phẩm trong đơn hàng đồ ăn
/// </summary>
public class SeatFoodOrderItemDto
{
    /// <summary>
    /// ID sản phẩm
    /// </summary>
    [Required]
    public Guid ProductId { get; set; }

    /// <summary>
    /// Số lượng
    /// </summary>
    [Required]
    [Range(1, 99, ErrorMessage = "Số lượng phải từ 1-99")]
    public int Quantity { get; set; }

    /// <summary>
    /// Ghi chú riêng cho sản phẩm này (ví dụ: ít đá, không đường)
    /// </summary>
    [MaxLength(200)]
    public string? ItemNote { get; set; }
}

/// <summary>
/// Response khi tạo đơn hàng đồ ăn từ ghế thành công
/// </summary>
public class SeatFoodOrderResponseDto
{
    /// <summary>
    /// ID đơn hàng
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// Mã đơn hàng để theo dõi
    /// </summary>
    public string OrderCode { get; set; } = string.Empty;

    /// <summary>
    /// Trạng thái đơn hàng
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Tổng tiền
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Thời gian giao hàng dự kiến (phút)
    /// </summary>
    public int EstimatedDeliveryMinutes { get; set; }

    /// <summary>
    /// Thông tin ghế để giao hàng
    /// </summary>
    public SeatDeliveryInfoDto SeatInfo { get; set; } = null!;

    /// <summary>
    /// Thông tin phim đang chiếu (để xác nhận đúng suất chiếu)
    /// </summary>
    public ShowingMovieInfoDto ShowingMovie { get; set; } = null!;

    /// <summary>
    /// Chi tiết sản phẩm đã order
    /// </summary>
    public List<SeatFoodOrderItemDetailDto> Items { get; set; } = new();

    /// <summary>
    /// Thông tin thanh toán (nếu đã thanh toán)
    /// </summary>
    public SeatFoodPaymentInfoDto? PaymentInfo { get; set; }

    /// <summary>
    /// Thời gian đặt hàng
    /// </summary>
    public DateTime OrderTime { get; set; }

    /// <summary>
    /// Tin nhắn hiển thị cho khách hàng
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Thông tin ghế để giao hàng
/// </summary>
public class SeatDeliveryInfoDto
{
    /// <summary>
    /// Mã ghế (VD: A5, B10)
    /// </summary>
    public string SeatCode { get; set; } = string.Empty;

    /// <summary>
    /// Hàng ghế
    /// </summary>
    public string SeatRow { get; set; } = string.Empty;

    /// <summary>
    /// Số ghế
    /// </summary>
    public int SeatNumber { get; set; }

    /// <summary>
    /// Tên phòng chiếu
    /// </summary>
    public string ScreenName { get; set; } = string.Empty;

    /// <summary>
    /// Tên rạp
    /// </summary>
    public string CinemaName { get; set; } = string.Empty;
}

/// <summary>
/// Thông tin phim đang chiếu
/// </summary>
public class ShowingMovieInfoDto
{
    /// <summary>
    /// ID suất chiếu
    /// </summary>
    public Guid ShowtimeId { get; set; }

    /// <summary>
    /// Tên phim
    /// </summary>
    public string MovieTitle { get; set; } = string.Empty;

    /// <summary>
    /// Thời gian bắt đầu chiếu
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Thời gian kết thúc chiếu (dự kiến)
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// Thời gian còn lại (phút) - để biết có đủ thời gian giao không
    /// </summary>
    public int RemainingMinutes { get; set; }
}

/// <summary>
/// Chi tiết từng sản phẩm đã order
/// </summary>
public class SeatFoodOrderItemDetailDto
{
    /// <summary>
    /// ID sản phẩm
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Tên sản phẩm
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Hình ảnh sản phẩm
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Số lượng
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Đơn giá
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Thành tiền
    /// </summary>
    public decimal SubTotal { get; set; }

    /// <summary>
    /// Ghi chú
    /// </summary>
    public string? ItemNote { get; set; }
}

/// <summary>
/// Thông tin thanh toán
/// </summary>
public class SeatFoodPaymentInfoDto
{
    /// <summary>
    /// Phương thức thanh toán
    /// </summary>
    public string PaymentMethod { get; set; } = string.Empty;

    /// <summary>
    /// Trạng thái thanh toán: Pending, Completed, Failed
    /// </summary>
    public string PaymentStatus { get; set; } = string.Empty;

    /// <summary>
    /// URL thanh toán (nếu dùng MOMO)
    /// </summary>
    public string? PayUrl { get; set; }

    /// <summary>
    /// Deeplink thanh toán MOMO
    /// </summary>
    public string? Deeplink { get; set; }

    /// <summary>
    /// QR code để thanh toán
    /// </summary>
    public string? QrCodeUrl { get; set; }

    /// <summary>
    /// Mã giao dịch
    /// </summary>
    public string? TransactionId { get; set; }
}

/// <summary>
/// DTO để kiểm tra thông tin ghế trước khi order
/// </summary>
public class SeatInfoRequestDto
{
    /// <summary>
    /// Mã QR ordering của ghế
    /// </summary>
    [Required(ErrorMessage = "Mã QR ghế là bắt buộc")]
    [MaxLength(20)]
    public string SeatQrCode { get; set; } = string.Empty;
}

/// <summary>
/// Response khi kiểm tra thông tin ghế
/// </summary>
public class SeatInfoResponseDto
{
    /// <summary>
    /// Ghế có hợp lệ và đang có suất chiếu không
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Thông báo lỗi nếu không hợp lệ
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Thông tin ghế
    /// </summary>
    public SeatDeliveryInfoDto? SeatInfo { get; set; }

    /// <summary>
    /// Thông tin phim đang chiếu (nếu có)
    /// </summary>
    public ShowingMovieInfoDto? ShowingMovie { get; set; }

    /// <summary>
    /// Có thể order đồ ăn không (true nếu suất chiếu còn đủ thời gian)
    /// </summary>
    public bool CanOrderFood { get; set; }

    /// <summary>
    /// Thời gian tối thiểu còn lại để order (phút)
    /// </summary>
    public int MinRemainingMinutesToOrder { get; set; } = 15;
}

/// <summary>
/// DTO để theo dõi trạng thái đơn hàng đồ ăn
/// </summary>
public class SeatFoodOrderStatusDto
{
    /// <summary>
    /// ID đơn hàng
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// Mã đơn hàng
    /// </summary>
    public string OrderCode { get; set; } = string.Empty;

    /// <summary>
    /// Trạng thái: Pending, Preparing, Delivering, Delivered, Cancelled
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Mô tả trạng thái bằng tiếng Việt
    /// </summary>
    public string StatusDescription { get; set; } = string.Empty;

    /// <summary>
    /// Thời gian dự kiến giao (phút còn lại)
    /// </summary>
    public int? EstimatedMinutesRemaining { get; set; }

    /// <summary>
    /// Thời gian cập nhật trạng thái gần nhất
    /// </summary>
    public DateTime LastUpdated { get; set; }
}
