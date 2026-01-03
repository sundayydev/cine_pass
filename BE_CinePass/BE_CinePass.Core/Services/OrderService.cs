using BE_CinePass.Core.Repositories;
using BE_CinePass.Core.Configurations;
using BE_CinePass.Domain.Common;
using BE_CinePass.Domain.Events;
using BE_CinePass.Domain.Models;
using BE_CinePass.Shared.DTOs.Order;
using Microsoft.EntityFrameworkCore;
using BE_CinePass.Shared.DTOs.Showtime;

namespace BE_CinePass.Core.Services;

public class OrderService
{
    private readonly OrderRepository _orderRepository;
    private readonly OrderTicketRepository _orderTicketRepository;
    private readonly OrderProductRepository _orderProductRepository;
    private readonly ShowtimeRepository _showtimeRepository;
    private readonly SeatRepository _seatRepository;
    private readonly ProductRepository _productRepository;
    private readonly SeatTypeRepository _seatTypeRepository;
    private readonly UserRepository _userRepository;
    private readonly PaymentTransactionRepository _paymentTransactionRepository;
    private readonly ETicketService _eTicketService;
    private readonly ApplicationDbContext _context;
    private readonly UserVoucherService _userVoucherService;
    private readonly MemberPointService _memberPointService;
    private readonly PointHistoryService _pointHistoryService;
    private readonly IEventBus _eventBus;
    
    public OrderService(
        OrderRepository orderRepository,
        OrderTicketRepository orderTicketRepository,
        OrderProductRepository orderProductRepository,
        ShowtimeRepository showtimeRepository,
        SeatRepository seatRepository,
        ProductRepository productRepository,
        SeatTypeRepository seatTypeRepository,
        UserRepository userRepository,
        PaymentTransactionRepository paymentTransactionRepository,
        ETicketService eTicketService,
        ApplicationDbContext context,
        UserVoucherService userVoucherService,
        MemberPointService memberPointService,
        PointHistoryService pointHistoryService,
        IEventBus eventBus)
    {
        _orderRepository = orderRepository;
        _orderTicketRepository = orderTicketRepository;
        _orderProductRepository = orderProductRepository;
        _showtimeRepository = showtimeRepository;
        _seatRepository = seatRepository;
        _productRepository = productRepository;
        _seatTypeRepository = seatTypeRepository;
        _userRepository = userRepository;
        _paymentTransactionRepository = paymentTransactionRepository;
        _eTicketService = eTicketService;
        _context = context;
        _userVoucherService = userVoucherService;
        _memberPointService = memberPointService;
        _pointHistoryService = pointHistoryService;
        _eventBus = eventBus;
    }


    public async Task<List<OrderResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // Include User để lấy thông tin khách hàng
        var orders = await _context.Orders
            .Include(o => o.User)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
        return orders.Select(MapToResponseDto).ToList();
    }

    public async Task<OrderResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken);
        return order == null ? null : MapToResponseDto(order);
    }

    public async Task<OrderDetailDto?> GetDetailByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetWithDetailsAsync(id, cancellationToken);
        return order == null ? null : MapToDetailDto(order);
    }

    public async Task<List<OrderResponseDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetByUserIdAsync(userId, cancellationToken);
        return orders.Select(MapToResponseDto).ToList();
    }

    public async Task<List<OrderResponseDto>> GetByStatusAsync(Shared.Common.OrderStatus status, CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetByStatusAsync((OrderStatus)status, cancellationToken);
        return orders.Select(MapToResponseDto).ToList();
    }

    public async Task<OrderResponseDto> CreateAsync(OrderCreateDto dto, CancellationToken cancellationToken = default)
    {
        var order = new Order
        {
            UserId = dto.UserId,
            Status = OrderStatus.Pending,
            PaymentMethod = dto.PaymentMethod,
            Note = dto.Note, // Ghi chú từ FE: SEAT-ORDER hoặc PRO-ORDER
            CreatedAt = DateTime.UtcNow,
            ExpireAt = DateTime.UtcNow.AddMinutes(15) // 15 minutes to complete payment
        };

        // Add order first to generate Id
        await _orderRepository.AddAsync(order, cancellationToken);

        decimal totalAmount = 0;

        // Process tickets
        foreach (var ticketItem in dto.Tickets)
        {
            var showtime = await _showtimeRepository.GetByIdAsync(ticketItem.ShowtimeId, cancellationToken);
            if (showtime == null)
                throw new InvalidOperationException($"Showtime with id {ticketItem.ShowtimeId} not found");

            var seat = await _seatRepository.GetByIdAsync(ticketItem.SeatId, cancellationToken);
            if (seat == null)
                throw new InvalidOperationException($"Seat with id {ticketItem.SeatId} not found");

            // Check if seat is available
            if (!await _seatRepository.IsSeatAvailableAsync(ticketItem.SeatId, ticketItem.ShowtimeId, cancellationToken))
                throw new InvalidOperationException($"Seat {seat.SeatCode} is already booked for this showtime");

            // Calculate price with seat type surcharge
            decimal ticketPrice = showtime.BasePrice;
            if (!string.IsNullOrEmpty(seat.SeatTypeCode))
            {
                var seatType = await _seatTypeRepository.GetByCodeAsync(seat.SeatTypeCode, cancellationToken);
                if (seatType != null)
                    ticketPrice *= seatType.SurchargeRate;
            }

            var orderTicket = new OrderTicket
            {
                OrderId = order.Id,
                ShowtimeId = ticketItem.ShowtimeId,
                SeatId = ticketItem.SeatId,
                Price = ticketPrice
            };

            await _orderTicketRepository.AddAsync(orderTicket, cancellationToken);
            totalAmount += ticketPrice;
        }

        // Process products
        foreach (var productItem in dto.Products)
        {
            var product = await _productRepository.GetByIdAsync(productItem.ProductId, cancellationToken);
            if (product == null)
                throw new InvalidOperationException($"Product with id {productItem.ProductId} not found");

            if (!product.IsActive)
                throw new InvalidOperationException($"Product {product.Name} is not active");

            var orderProduct = new OrderProduct
            {
                OrderId = order.Id,
                ProductId = productItem.ProductId,
                Quantity = productItem.Quantity,
                UnitPrice = product.Price
            };

            await _orderProductRepository.AddAsync(orderProduct, cancellationToken);
            totalAmount += product.Price * productItem.Quantity;
        }

        // Tính tổng tiền order
        order.TotalAmount = totalAmount;
        order.FinalAmount = totalAmount; // Mặc định = TotalAmount, sẽ update khi apply voucher
        order.DiscountAmount = 0;
        order.UserVoucherId = null;

        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponseDto(order);
    }

    public async Task<OrderResponseDto?> UpdateAsync(Guid id, OrderUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken);
        if (order == null)
            return null;

        if (dto.Status.HasValue)
            order.Status = (OrderStatus)dto.Status.Value;

        if (dto.PaymentMethod != null)
            order.PaymentMethod = dto.PaymentMethod;

        _orderRepository.Update(order);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponseDto(order);
    }

    public async Task<bool> ConfirmOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
            return false;

        if (order.Status != OrderStatus.Pending)
            throw new InvalidOperationException("Only pending orders can be confirmed");

        // Xác nhận order sau khi thanh toán thành công
        order.Status = Domain.Common.OrderStatus.Confirmed;
        order.ExpireAt = DateTime.UtcNow; // Thời điểm thanh toán thành công
        
        // Đánh dấu voucher đã sử dụng
        if (order.UserVoucherId.HasValue)
        {
            await _userVoucherService.MarkAsUsedAsync(order.UserVoucherId.Value, orderId, cancellationToken);
        }
        
        // Tích điểm cho user
        if (order.UserId.HasValue)
        {
            var (basePoints, bonusPoints, totalPoints) = await _memberPointService
                .AddPointsFromOrderAsync(order.UserId.Value, order.FinalAmount, cancellationToken);
            
            await _pointHistoryService.CreateAsync(new PointHistory
            {
                UserId = order.UserId.Value,
                Points = totalPoints,
                Type = PointHistoryType.Purchase,
                OrderId = orderId,
                Description = $"Tích {basePoints} điểm (+ {bonusPoints} điểm thưởng) từ đơn hàng",
                ExpiresAt = DateTime.UtcNow.AddMonths(12)
            }, cancellationToken);
        }

        _orderRepository.Update(order);
        await _context.SaveChangesAsync(cancellationToken);

        // TẠO E-TICKETS CHO MỖI ORDER TICKET
        // Lấy danh sách OrderTickets của order này
        var orderTickets = await _context.OrderTickets
            .Where(ot => ot.OrderId == orderId)
            .ToListAsync(cancellationToken);

        foreach (var orderTicket in orderTickets)
        {
            try
            {
                // Kiểm tra xem ETicket đã tồn tại chưa
                var existingTickets = await _context.ETickets
                    .Where(e => e.OrderTicketId == orderTicket.Id)
                    .AnyAsync(cancellationToken);

                if (!existingTickets)
                {
                    await _eTicketService.GenerateETicketAsync(orderTicket.Id, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                // Log error nhưng không throw để không ảnh hưởng confirm order
                Console.WriteLine($"Error generating e-ticket for orderTicket {orderTicket.Id}: {ex.Message}");
            }
        }
        
        if (order.UserId.HasValue)
        {
            await _eventBus.PublishAsync(new OrderFailedEvent
            {
                OrderId = order.Id,
                UserId = order.UserId.Value,
                Reason = "Order cancelled by user"
            });
        }
        
        return true;
    }

    public async Task<bool> CancelOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
            return false;

        if (order.Status == OrderStatus.Confirmed)
            throw new InvalidOperationException("Các đơn hàng đã được xác nhận không thể hủy trực tiếp. Vui lòng sử dụng chức năng hoàn tiền.");

        order.Status = OrderStatus.Cancelled;

        _orderRepository.Update(order);
        await _context.SaveChangesAsync(cancellationToken);
        
        if (order.UserId.HasValue)
        {
            await _eventBus.PublishAsync(new OrderConfirmedEvent
            {
                OrderId = order.Id,
                UserId = order.UserId.Value,
                OrderCode = $"ORD-{order.Id.ToString()[..8]}",
                TotalAmount = order.FinalAmount,
                TicketCount = order.OrderTickets?.Count ?? 0
            });
        }
        
        return true;
    }

    public async Task<List<OrderResponseDto>> GetExpiredOrdersAsync(CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetExpiredOrdersAsync(cancellationToken);
        return orders.Select(MapToResponseDto).ToList();
    }

    // Staff Support Methods
    public async Task<List<OrderSearchResultDto>> SearchByPhoneAsync(string phone, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByPhoneAsync(phone, cancellationToken);
        if (user == null)
            return new List<OrderSearchResultDto>();

        var orders = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderTickets)
                .ThenInclude(ot => ot.Showtime)
                    .ThenInclude(s => s.Movie)
            .Where(o => o.UserId == user.Id && o.Status == OrderStatus.Confirmed)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        return orders.Select(o => new OrderSearchResultDto
        {
            Id = o.Id,
            CustomerName = o.User?.FullName ?? "N/A",
            CustomerPhone = o.User?.Phone ?? phone,
            CustomerEmail = o.User?.Email ?? "N/A",
            TotalAmount = o.TotalAmount,
            Status = o.Status.ToString(),
            PaymentMethod = o.PaymentMethod,
            CreatedAt = o.CreatedAt,
            TicketCount = o.OrderTickets.Count,
            MovieTitles = string.Join(", ", o.OrderTickets
                .Select(ot => ot.Showtime?.Movie?.Title ?? "Unknown")
                .Distinct())
        }).ToList();
    }

    public async Task<OrderDetailDto?> GetForPrintAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetWithDetailsAsync(orderId, cancellationToken);

        if (order == null)
            return null;

        if (order.Status != OrderStatus.Confirmed)
            throw new InvalidOperationException("Only confirmed orders can be printed");

        return MapToDetailDto(order);
    }

    /// <summary>
    /// Tạo đơn hàng POS (Point of Sale) và thanh toán tiền mặt ngay lập tức
    /// </summary>
    public async Task<PosOrderResponseDto> CreatePosOrderAsync(PosOrderCreateDto dto, CancellationToken cancellationToken = default)
    {
        // Validate cash received is sufficient
        decimal totalAmount = 0;

        // Calculate total first to validate cash received
        foreach (var ticketItem in dto.Tickets)
        {
            var showtime = await _showtimeRepository.GetByIdAsync(ticketItem.ShowtimeId, cancellationToken);
            if (showtime == null)
                throw new InvalidOperationException($"Showtime with id {ticketItem.ShowtimeId} not found");

            var seat = await _seatRepository.GetByIdAsync(ticketItem.SeatId, cancellationToken);
            if (seat == null)
                throw new InvalidOperationException($"Seat with id {ticketItem.SeatId} not found");

            // Check if seat is available
            if (!await _seatRepository.IsSeatAvailableAsync(ticketItem.SeatId, ticketItem.ShowtimeId, cancellationToken))
                throw new InvalidOperationException($"Seat {seat.SeatCode} is already booked for this showtime");

            decimal ticketPrice = showtime.BasePrice;
            if (!string.IsNullOrEmpty(seat.SeatTypeCode))
            {
                var seatType = await _seatTypeRepository.GetByCodeAsync(seat.SeatTypeCode, cancellationToken);
                if (seatType != null)
                    ticketPrice *= seatType.SurchargeRate;
            }

            totalAmount += ticketPrice;
        }

        foreach (var productItem in dto.Products)
        {
            var product = await _productRepository.GetByIdAsync(productItem.ProductId, cancellationToken);
            if (product == null)
                throw new InvalidOperationException($"Product with id {productItem.ProductId} not found");

            if (!product.IsActive)
                throw new InvalidOperationException($"Product {product.Name} is not active");

            totalAmount += product.Price * productItem.Quantity;
        }

        // Validate cash received
        if (dto.CashReceived < totalAmount)
            throw new InvalidOperationException($"Số tiền khách đưa ({dto.CashReceived:N0} VND) không đủ để thanh toán ({totalAmount:N0} VND)");

        // Create the order with Confirmed status (bypassing Pending)
        var order = new Order
        {
            UserId = null, // POS orders are guest orders
            Status = OrderStatus.Confirmed,
            PaymentMethod = "CASH",
            CreatedAt = DateTime.UtcNow,
            ExpireAt = DateTime.UtcNow // Thời điểm thanh toán thành công (POS)
        };

        // Process tickets
        var orderTickets = new List<OrderTicket>();
        foreach (var ticketItem in dto.Tickets)
        {
            var showtime = await _showtimeRepository.GetByIdAsync(ticketItem.ShowtimeId, cancellationToken);
            var seat = await _seatRepository.GetByIdAsync(ticketItem.SeatId, cancellationToken);

            decimal ticketPrice = showtime!.BasePrice;
            if (!string.IsNullOrEmpty(seat!.SeatTypeCode))
            {
                var seatType = await _seatTypeRepository.GetByCodeAsync(seat.SeatTypeCode, cancellationToken);
                if (seatType != null)
                    ticketPrice *= seatType.SurchargeRate;
            }

            var orderTicket = new OrderTicket
            {
                OrderId = order.Id,
                ShowtimeId = ticketItem.ShowtimeId,
                SeatId = ticketItem.SeatId,
                Price = ticketPrice
            };

            orderTickets.Add(orderTicket);
            await _orderTicketRepository.AddAsync(orderTicket, cancellationToken);
        }

        // Process products
        foreach (var productItem in dto.Products)
        {
            var product = await _productRepository.GetByIdAsync(productItem.ProductId, cancellationToken);

            var orderProduct = new OrderProduct
            {
                OrderId = order.Id,
                ProductId = productItem.ProductId,
                Quantity = productItem.Quantity,
                UnitPrice = product!.Price
            };

            await _orderProductRepository.AddAsync(orderProduct, cancellationToken);
        }

        order.TotalAmount = totalAmount;

        // Add order to database
        await _orderRepository.AddAsync(order, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Create payment transaction for cash payment
        var paymentTransaction = new PaymentTransaction
        {
            OrderId = order.Id,
            ProviderTransId = $"CASH-{order.Id}-{DateTime.UtcNow:yyyyMMddHHmmss}",
            Amount = totalAmount,
            Status = "Success",
            ResponseJson = System.Text.Json.JsonDocument.Parse($"{{\"cashReceived\":{dto.CashReceived},\"change\":{dto.CashReceived - totalAmount},\"staffNote\":\"{dto.StaffNote ?? ""}\",\"customerName\":\"{dto.CustomerName}\",\"customerPhone\":\"{dto.CustomerPhone}\"}}"),
            CreatedAt = DateTime.UtcNow
        };

        await _paymentTransactionRepository.AddAsync(paymentTransaction, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Generate e-tickets for all order tickets
        foreach (var orderTicket in orderTickets)
        {
            await _eTicketService.GenerateETicketAsync(orderTicket.Id, cancellationToken);
        }
        
        await _eventBus.PublishAsync(new PaymentSuccessEvent
        {
            OrderId = order.Id,
            UserId = null, // Guest order
            Amount = totalAmount,
            PaymentMethod = "CASH"
        });

        // Get the full order details with all related data for printing
        var orderDetail = await _orderRepository.GetWithDetailsAsync(order.Id, cancellationToken);
        if (orderDetail == null)
            throw new InvalidOperationException("Failed to retrieve order details after creation");

        // Build response for POS printing
        var response = new PosOrderResponseDto
        {
            OrderDetail = MapToDetailDto(orderDetail),
            PaymentInfo = new PosPaymentInfo
            {
                TotalAmount = totalAmount,
                CashReceived = dto.CashReceived,
                ChangeAmount = dto.CashReceived - totalAmount,
                PaymentMethod = "CASH",
                PaymentTime = DateTime.UtcNow,
                TransactionId = paymentTransaction.Id
            },
            CustomerInfo = new PosCustomerInfo
            {
                Name = dto.CustomerName,
                Phone = dto.CustomerPhone,
                Email = dto.CustomerEmail
            }
        };

        return response;
    }
    
    /// <summary>
    /// Áp dụng voucher vào order
    /// </summary>
    public async Task<OrderResponseDto> ApplyVoucherAsync(
        Guid orderId, 
        Guid userVoucherId, 
        CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
            throw new InvalidOperationException("Order không tồn tại");
            
        if (order.Status != OrderStatus.Pending)
            throw new InvalidOperationException("Chỉ có thể áp dụng voucher cho order đang chờ thanh toán");
            
        // Validate voucher
        var (isValid, errorMessage) = await _userVoucherService
            .ValidateVoucherUsageAsync(userVoucherId, order.TotalAmount, cancellationToken);
            
        if (!isValid)
            throw new InvalidOperationException(errorMessage ?? "Voucher không hợp lệ");
            
        // Get voucher để tính discount
        var userVoucher = await _context.UserVouchers
            .Include(uv => uv.Voucher)
            .FirstOrDefaultAsync(uv => uv.Id == userVoucherId, cancellationToken);
            
        if (userVoucher == null)
            throw new InvalidOperationException("User voucher không tồn tại");
            
        // Tính discount
        var discount = _userVoucherService.CalculateDiscount(userVoucher, order.TotalAmount);
        
        // Update order
        order.UserVoucherId = userVoucherId;
        order.DiscountAmount = discount;
        order.FinalAmount = order.TotalAmount - discount;
        
        _orderRepository.Update(order);
        await _context.SaveChangesAsync(cancellationToken);
        
        return MapToResponseDto(order);
    }
    
    /// <summary>
    /// Xóa voucher khỏi order
    /// </summary>
    public async Task<OrderResponseDto> RemoveVoucherAsync(
        Guid orderId, 
        CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
            throw new InvalidOperationException("Order không tồn tại");
            
        if (order.Status != OrderStatus.Pending)
            throw new InvalidOperationException("Chỉ có thể xóa voucher khỏi order đang chờ thanh toán");
            
        order.UserVoucherId = null;
        order.DiscountAmount = 0;
        order.FinalAmount = order.TotalAmount;
        
        _orderRepository.Update(order);
        await _context.SaveChangesAsync(cancellationToken);
        
        return MapToResponseDto(order);
    }

    private static OrderResponseDto MapToResponseDto(Order order)
    {
        return new OrderResponseDto
        {
            Id = order.Id,
            UserId = order.UserId,
            CustomerName = order.User?.FullName,
            CustomerPhone = order.User?.Phone,
            CustomerEmail = order.User?.Email,
            TotalAmount = order.TotalAmount,
            UserVoucherId = order.UserVoucherId,
            DiscountAmount = order.DiscountAmount,
            FinalAmount = order.FinalAmount,
            Status = order.Status.ToString(),
            PaymentMethod = order.PaymentMethod,
            Note = order.Note,
            CreatedAt = order.CreatedAt,
            ExpireAt = order.ExpireAt
        };
    }

    private static OrderDetailDto MapToDetailDto(Order order)
    {
        return new OrderDetailDto
        {
            Id = order.Id,
            UserId = order.UserId,
            User = order.User != null ? new Shared.DTOs.User.UserResponseDto
            {
                Id = order.User.Id,
                Email = order.User.Email,
                Phone = order.User.Phone,
                FullName = order.User.FullName,
                Role = (Shared.Common.UserRole)order.User.Role,
                CreatedAt = order.User.CreatedAt,
                UpdatedAt = order.User.UpdatedAt
            } : null,
            TotalAmount = order.TotalAmount,
            Status = order.Status.ToString(),
            PaymentMethod = order.PaymentMethod,
            CreatedAt = order.CreatedAt,
            ExpireAt = order.ExpireAt,
            Tickets = order.OrderTickets.Select(ot => new OrderTicketDetailDto
            {
                Id = ot.Id,
                ShowtimeId = ot.ShowtimeId,
                SeatId = ot.SeatId,
                Price = ot.Price,
                Showtime = ot.Showtime != null ? new ShowtimeDetailDto
                {
                    Id = ot.Showtime.Id,
                    StartTime = ot.Showtime.StartTime,
                    EndTime = ot.Showtime.EndTime,
                    BasePrice = ot.Showtime.BasePrice,
                    IsActive = ot.Showtime.IsActive,
                    Movie = ot.Showtime.Movie != null ? new Shared.DTOs.Movie.MovieResponseDto
                    {
                        Id = ot.Showtime.Movie.Id,
                        Title = ot.Showtime.Movie.Title,
                        Slug = ot.Showtime.Movie.Slug,
                        DurationMinutes = ot.Showtime.Movie.DurationMinutes,
                        Description = ot.Showtime.Movie.Description,
                        PosterUrl = ot.Showtime.Movie.PosterUrl,
                        TrailerUrl = ot.Showtime.Movie.TrailerUrl,
                        ReleaseDate = ot.Showtime.Movie.ReleaseDate,
                        Status = ot.Showtime.Movie.Status.ToString(),
                        CreatedAt = ot.Showtime.Movie.CreatedAt
                    } : null!,
                    Screen = ot.Showtime.Screen != null ? new Shared.DTOs.Screen.ScreenResponseDto
                    {
                        Id = ot.Showtime.Screen.Id,
                        CinemaId = ot.Showtime.Screen.CinemaId,
                        CinemaName = ot.Showtime.Screen.Cinema?.Name,
                        Name = ot.Showtime.Screen.Name,
                        TotalSeats = ot.Showtime.Screen.TotalSeats,
                        SeatMapLayout = ot.Showtime.Screen.SeatMapLayout?.RootElement.GetRawText()
                    } : null!
                } : null,
                Seat = ot.Seat != null ? new Shared.DTOs.Seat.SeatResponseDto
                {
                    Id = ot.Seat.Id,
                    ScreenId = ot.Seat.ScreenId,
                    SeatRow = ot.Seat.SeatRow,
                    SeatNumber = ot.Seat.SeatNumber,
                    SeatCode = ot.Seat.SeatCode,
                    SeatTypeCode = ot.Seat.SeatTypeCode,
                    IsActive = ot.Seat.IsActive
                } : null,
                ETicket = ot.ETickets.FirstOrDefault() != null ? new Shared.DTOs.ETicket.ETicketResponseDto
                {
                    Id = ot.ETickets.First().Id,
                    OrderTicketId = ot.ETickets.First().OrderTicketId,
                    TicketCode = ot.ETickets.First().TicketCode,
                    QrData = ot.ETickets.First().QrData,
                    IsUsed = ot.ETickets.First().IsUsed,
                    UsedAt = ot.ETickets.First().UsedAt
                } : null
            }).ToList(),
            Products = order.OrderProducts.Select(op => new OrderProductDetailDto
            {
                Id = op.Id,
                ProductId = op.ProductId,
                Quantity = op.Quantity,
                UnitPrice = op.UnitPrice,
                Product = op.Product != null ? new Shared.DTOs.Product.ProductResponseDto
                {
                    Id = op.Product.Id,
                    Name = op.Product.Name,
                    Description = op.Product.Description,
                    Price = op.Product.Price,
                    ImageUrl = op.Product.ImageUrl,
                    Category = op.Product.Category.ToString(),
                    IsActive = op.Product.IsActive
                } : null
            }).ToList()
        };
    }
}

