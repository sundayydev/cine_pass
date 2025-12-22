using BE_CinePass.Core.Configurations;
using BE_CinePass.Core.Repositories;
using BE_CinePass.Domain.Common;
using BE_CinePass.Domain.Models;
using BE_CinePass.Shared.DTOs.Order;
using Microsoft.EntityFrameworkCore;
using NanoidDotNet;

namespace BE_CinePass.Core.Services;

/// <summary>
/// Service xử lý order đồ ăn/nước uống từ ghế ngồi trong rạp qua QR code
/// </summary>
public class SeatFoodOrderService
{
    private readonly ApplicationDbContext _context;
    private readonly SeatRepository _seatRepository;
    private readonly ProductRepository _productRepository;
    private readonly OrderRepository _orderRepository;
    private readonly OrderProductRepository _orderProductRepository;
    private readonly MomoPaymentService _momoPaymentService;
    private readonly PaymentTransactionRepository _paymentTransactionRepository;

    // Thời gian tối thiểu còn lại của suất chiếu để có thể order đồ ăn (phút)
    private const int MinRemainingMinutesToOrder = 15;
    // Thời gian giao hàng dự kiến (phút)
    private const int DefaultDeliveryMinutes = 10;

    public SeatFoodOrderService(
        ApplicationDbContext context,
        SeatRepository seatRepository,
        ProductRepository productRepository,
        OrderRepository orderRepository,
        OrderProductRepository orderProductRepository,
        MomoPaymentService momoPaymentService,
        PaymentTransactionRepository paymentTransactionRepository)
    {
        _context = context;
        _seatRepository = seatRepository;
        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _orderProductRepository = orderProductRepository;
        _momoPaymentService = momoPaymentService;
        _paymentTransactionRepository = paymentTransactionRepository;
    }

    /// <summary>
    /// Kiểm tra thông tin ghế từ mã QR và xác định có suất chiếu đang diễn ra không
    /// </summary>
    public async Task<SeatInfoResponseDto> GetSeatInfoAsync(string seatQrCode, CancellationToken cancellationToken = default)
    {
        var response = new SeatInfoResponseDto
        {
            MinRemainingMinutesToOrder = MinRemainingMinutesToOrder
        };

        // Tìm ghế theo mã QR ordering
        var seat = await _context.Seats
            .Include(s => s.Screen)
                .ThenInclude(sc => sc.Cinema)
            .FirstOrDefaultAsync(s => s.QrOrderingCode == seatQrCode && s.IsActive, cancellationToken);

        if (seat == null)
        {
            response.IsValid = false;
            response.ErrorMessage = "Mã QR không hợp lệ hoặc ghế không tồn tại";
            response.CanOrderFood = false;
            return response;
        }

        // Lấy thông tin ghế
        response.SeatInfo = new SeatDeliveryInfoDto
        {
            SeatCode = seat.SeatCode,
            SeatRow = seat.SeatRow,
            SeatNumber = seat.SeatNumber,
            ScreenName = seat.Screen?.Name ?? "N/A",
            CinemaName = seat.Screen?.Cinema?.Name ?? "N/A"
        };

        // Tìm suất chiếu đang diễn ra tại phòng chiếu này
        var now = DateTime.UtcNow;
        var currentShowtime = await _context.Showtimes
            .Include(s => s.Movie)
            .Where(s => s.ScreenId == seat.ScreenId
                && s.StartTime <= now
                && s.EndTime > now
                && s.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        if (currentShowtime == null)
        {
            response.IsValid = true;
            response.ErrorMessage = "Hiện không có suất chiếu nào đang diễn ra tại phòng này";
            response.CanOrderFood = false;
            return response;
        }

        // Kiểm tra xem ghế này có được đặt cho suất chiếu này không
        var hasBooking = await _context.OrderTickets
            .Include(ot => ot.Order)
            .AnyAsync(ot => ot.SeatId == seat.Id
                && ot.ShowtimeId == currentShowtime.Id
                && ot.Order.Status == OrderStatus.Confirmed,
                cancellationToken);

        if (!hasBooking)
        {
            response.IsValid = true;
            response.ErrorMessage = "Ghế này không có ai đặt cho suất chiếu hiện tại";
            response.CanOrderFood = false;
            return response;
        }

        // Tính thời gian còn lại
        var remainingMinutes = (int)(currentShowtime.EndTime - now).TotalMinutes;

        response.ShowingMovie = new ShowingMovieInfoDto
        {
            ShowtimeId = currentShowtime.Id,
            MovieTitle = currentShowtime.Movie?.Title ?? "N/A",
            StartTime = currentShowtime.StartTime,
            EndTime = currentShowtime.EndTime,
            RemainingMinutes = remainingMinutes
        };

        // Kiểm tra có đủ thời gian để order không
        if (remainingMinutes < MinRemainingMinutesToOrder)
        {
            response.IsValid = true;
            response.ErrorMessage = $"Phim sắp kết thúc (còn {remainingMinutes} phút), không thể order đồ ăn";
            response.CanOrderFood = false;
            return response;
        }

        response.IsValid = true;
        response.CanOrderFood = true;
        return response;
    }

    /// <summary>
    /// Tạo đơn hàng đồ ăn/nước uống từ ghế ngồi
    /// </summary>
    public async Task<SeatFoodOrderResponseDto> CreateOrderAsync(SeatFoodOrderCreateDto dto, CancellationToken cancellationToken = default)
    {
        // Kiểm tra thông tin ghế
        var seatInfo = await GetSeatInfoAsync(dto.SeatQrCode, cancellationToken);

        if (!seatInfo.IsValid)
            throw new InvalidOperationException(seatInfo.ErrorMessage ?? "Mã QR không hợp lệ");

        if (!seatInfo.CanOrderFood)
            throw new InvalidOperationException(seatInfo.ErrorMessage ?? "Không thể order đồ ăn lúc này");

        if (seatInfo.ShowingMovie == null)
            throw new InvalidOperationException("Không tìm thấy thông tin suất chiếu");

        // Lấy ghế
        var seat = await _context.Seats
            .Include(s => s.Screen)
                .ThenInclude(sc => sc.Cinema)
            .FirstOrDefaultAsync(s => s.QrOrderingCode == dto.SeatQrCode, cancellationToken);

        if (seat == null)
            throw new InvalidOperationException("Không tìm thấy ghế");

        // Validate và tính tổng tiền
        decimal totalAmount = 0;
        var orderItems = new List<(Product product, int quantity, string? note)>();

        foreach (var item in dto.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken);
            if (product == null)
                throw new InvalidOperationException($"Sản phẩm không tồn tại: {item.ProductId}");

            if (!product.IsActive)
                throw new InvalidOperationException($"Sản phẩm '{product.Name}' hiện không khả dụng");

            totalAmount += product.Price * item.Quantity;
            orderItems.Add((product, item.Quantity, item.ItemNote));
        }

        // Tạo mã đơn hàng
        var orderCode = $"SF-{Nanoid.Generate(size: 8).ToUpper()}";

        // Tạo đơn hàng
        var order = new Order
        {
            UserId = dto.UserId,
            Status = OrderStatus.Pending,
            PaymentMethod = dto.PaymentMethod,
            TotalAmount = totalAmount,
            CreatedAt = DateTime.UtcNow,
            ExpireAt = DateTime.UtcNow.AddMinutes(30), // 30 phút để thanh toán
            Note = $"[SEAT-ORDER] Ghế: {seat.SeatCode} | Phòng: {seat.Screen?.Name} | {dto.Note}"
        };

        await _orderRepository.AddAsync(order, cancellationToken);

        // Thêm sản phẩm vào đơn hàng
        foreach (var (product, quantity, note) in orderItems)
        {
            var orderProduct = new OrderProduct
            {
                OrderId = order.Id,
                ProductId = product.Id,
                Quantity = quantity,
                UnitPrice = product.Price
            };
            await _orderProductRepository.AddAsync(orderProduct, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Tạo response
        var response = new SeatFoodOrderResponseDto
        {
            OrderId = order.Id,
            OrderCode = orderCode,
            Status = order.Status.ToString(),
            TotalAmount = totalAmount,
            EstimatedDeliveryMinutes = DefaultDeliveryMinutes,
            SeatInfo = seatInfo.SeatInfo!,
            ShowingMovie = seatInfo.ShowingMovie,
            OrderTime = order.CreatedAt,
            Message = "Đơn hàng đã được tạo thành công. Vui lòng thanh toán để xác nhận.",
            Items = orderItems.Select(oi => new SeatFoodOrderItemDetailDto
            {
                ProductId = oi.product.Id,
                ProductName = oi.product.Name,
                ImageUrl = oi.product.ImageUrl,
                Quantity = oi.quantity,
                UnitPrice = oi.product.Price,
                SubTotal = oi.product.Price * oi.quantity,
                ItemNote = oi.note
            }).ToList()
        };

        // Xử lý thanh toán theo phương thức
        if (dto.PaymentMethod.ToUpper() == "MOMO")
        {
            var momoResponse = await _momoPaymentService.CreatePaymentAsync(
                order.Id,
                totalAmount,
                $"Order đồ ăn - Ghế {seat.SeatCode}",
                $"seat:{seat.Id}|showtime:{seatInfo.ShowingMovie.ShowtimeId}"
            );

            if (momoResponse.ResultCode == 0)
            {
                response.PaymentInfo = new SeatFoodPaymentInfoDto
                {
                    PaymentMethod = "MOMO",
                    PaymentStatus = "Pending",
                    PayUrl = momoResponse.PayUrl,
                    Deeplink = momoResponse.Deeplink,
                    QrCodeUrl = momoResponse.QrCodeUrl,
                    TransactionId = momoResponse.RequestId
                };

                // Lưu transaction
                var transaction = new PaymentTransaction
                {
                    OrderId = order.Id,
                    ProviderTransId = momoResponse.RequestId,
                    Amount = totalAmount,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };
                await _paymentTransactionRepository.AddAsync(transaction, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                response.Message = "Vui lòng quét mã QR hoặc mở link để thanh toán qua MoMo";
            }
            else
            {
                throw new InvalidOperationException($"Không thể tạo thanh toán MoMo: {momoResponse.Message}");
            }
        }
        else if (dto.PaymentMethod.ToUpper() == "CASH")
        {
            response.PaymentInfo = new SeatFoodPaymentInfoDto
            {
                PaymentMethod = "CASH",
                PaymentStatus = "Pending"
            };
            response.Message = "Đơn hàng đã được tạo. Nhân viên sẽ đến để thu tiền và giao hàng.";
        }

        return response;
    }

    /// <summary>
    /// Xác nhận thanh toán tiền mặt và xác nhận đơn hàng
    /// </summary>
    public async Task<SeatFoodOrderResponseDto> ConfirmCashPaymentAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .Include(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order == null)
            throw new InvalidOperationException("Không tìm thấy đơn hàng");

        if (order.Status != OrderStatus.Pending)
            throw new InvalidOperationException("Đơn hàng không ở trạng thái chờ thanh toán");

        // Cập nhật trạng thái
        order.Status = OrderStatus.Confirmed;
        order.ExpireAt = null;

        // Tạo transaction cho thanh toán tiền mặt
        var transaction = new PaymentTransaction
        {
            OrderId = order.Id,
            ProviderTransId = $"CASH-SF-{orderId}-{DateTime.UtcNow:yyyyMMddHHmmss}",
            Amount = order.TotalAmount,
            Status = "Success",
            CreatedAt = DateTime.UtcNow
        };
        await _paymentTransactionRepository.AddAsync(transaction, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new SeatFoodOrderResponseDto
        {
            OrderId = order.Id,
            OrderCode = $"SF-{order.Id.ToString()[..8].ToUpper()}",
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            OrderTime = order.CreatedAt,
            Message = "Đơn hàng đã được xác nhận thanh toán thành công",
            Items = order.OrderProducts.Select(op => new SeatFoodOrderItemDetailDto
            {
                ProductId = op.ProductId,
                ProductName = op.Product?.Name ?? "N/A",
                ImageUrl = op.Product?.ImageUrl,
                Quantity = op.Quantity,
                UnitPrice = op.UnitPrice,
                SubTotal = op.UnitPrice * op.Quantity
            }).ToList(),
            PaymentInfo = new SeatFoodPaymentInfoDto
            {
                PaymentMethod = "CASH",
                PaymentStatus = "Completed",
                TransactionId = transaction.Id.ToString()
            }
        };
    }

    /// <summary>
    /// Lấy trạng thái đơn hàng đồ ăn
    /// </summary>
    public async Task<SeatFoodOrderStatusDto?> GetOrderStatusAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order == null)
            return null;

        var statusDescriptions = new Dictionary<OrderStatus, string>
        {
            { OrderStatus.Pending, "Đang chờ thanh toán" },
            { OrderStatus.Confirmed, "Đã xác nhận - Đang chuẩn bị" },
            { OrderStatus.Cancelled, "Đã hủy" }
        };

        return new SeatFoodOrderStatusDto
        {
            OrderId = order.Id,
            OrderCode = $"SF-{order.Id.ToString()[..8].ToUpper()}",
            Status = order.Status.ToString(),
            StatusDescription = statusDescriptions.GetValueOrDefault(order.Status, "Không xác định"),
            EstimatedMinutesRemaining = order.Status == OrderStatus.Confirmed ? DefaultDeliveryMinutes : null,
            LastUpdated = order.CreatedAt
        };
    }

    /// <summary>
    /// Hủy đơn hàng đồ ăn (chỉ khi chưa thanh toán)
    /// </summary>
    public async Task<bool> CancelOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order == null)
            return false;

        if (order.Status != OrderStatus.Pending)
            throw new InvalidOperationException("Chỉ có thể hủy đơn hàng chưa thanh toán");

        order.Status = OrderStatus.Cancelled;
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <summary>
    /// Lấy danh sách đơn hàng đồ ăn theo ghế trong ngày
    /// </summary>
    public async Task<List<SeatFoodOrderStatusDto>> GetOrdersBySeatAsync(string seatQrCode, CancellationToken cancellationToken = default)
    {
        var seat = await _seatRepository.GetByQrCodeAsync(seatQrCode, cancellationToken);
        if (seat == null)
            return new List<SeatFoodOrderStatusDto>();

        var today = DateTime.UtcNow.Date;
        var orders = await _context.Orders
            .Where(o => o.Note != null && o.Note.Contains($"Ghế: {seat.SeatCode}")
                && o.CreatedAt >= today)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        var statusDescriptions = new Dictionary<OrderStatus, string>
        {
            { OrderStatus.Pending, "Đang chờ thanh toán" },
            { OrderStatus.Confirmed, "Đã xác nhận - Đang chuẩn bị" },
            { OrderStatus.Cancelled, "Đã hủy" }
        };

        return orders.Select(o => new SeatFoodOrderStatusDto
        {
            OrderId = o.Id,
            OrderCode = $"SF-{o.Id.ToString()[..8].ToUpper()}",
            Status = o.Status.ToString(),
            StatusDescription = statusDescriptions.GetValueOrDefault(o.Status, "Không xác định"),
            EstimatedMinutesRemaining = o.Status == OrderStatus.Confirmed ? DefaultDeliveryMinutes : null,
            LastUpdated = o.CreatedAt
        }).ToList();
    }
}
