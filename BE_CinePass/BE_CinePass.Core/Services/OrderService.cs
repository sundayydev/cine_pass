using BE_CinePass.Core.Repositories;
using BE_CinePass.Core.Configurations;
using BE_CinePass.Domain.Common;
using BE_CinePass.Domain.Models;
using BE_CinePass.Shared.Common;
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
    private readonly ApplicationDbContext _context;

    public OrderService(
        OrderRepository orderRepository,
        OrderTicketRepository orderTicketRepository,
        OrderProductRepository orderProductRepository,
        ShowtimeRepository showtimeRepository,
        SeatRepository seatRepository,
        ProductRepository productRepository,
        SeatTypeRepository seatTypeRepository,
        ApplicationDbContext context)
    {
        _orderRepository = orderRepository;
        _orderTicketRepository = orderTicketRepository;
        _orderProductRepository = orderProductRepository;
        _showtimeRepository = showtimeRepository;
        _seatRepository = seatRepository;
        _productRepository = productRepository;
        _seatTypeRepository = seatTypeRepository;
        _context = context;
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
        var orders = await _orderRepository.GetByStatusAsync((Domain.Common.OrderStatus)status, cancellationToken);
        return orders.Select(MapToResponseDto).ToList();
    }

    public async Task<OrderResponseDto> CreateAsync(OrderCreateDto dto, CancellationToken cancellationToken = default)
    {
        var order = new Order
        {
            UserId = dto.UserId,
            Status = Domain.Common.OrderStatus.Pending,
            PaymentMethod = dto.PaymentMethod,
            CreatedAt = DateTime.UtcNow,
            ExpireAt = DateTime.UtcNow.AddMinutes(15) // 15 minutes to complete payment
        };

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

        order.TotalAmount = totalAmount;

        await _orderRepository.AddAsync(order, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponseDto(order);
    }

    public async Task<OrderResponseDto?> UpdateAsync(Guid id, OrderUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken);
        if (order == null)
            return null;

        if (dto.Status.HasValue)
            order.Status = (Domain.Common.OrderStatus)dto.Status.Value;

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

        if (order.Status != Domain.Common.OrderStatus.Pending)
            throw new InvalidOperationException("Only pending orders can be confirmed");

        order.Status = Domain.Common.OrderStatus.Confirmed;
        order.ExpireAt = null;

        _orderRepository.Update(order);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> CancelOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
            return false;

        if (order.Status == Domain.Common.OrderStatus.Confirmed)
            throw new InvalidOperationException("Các đơn hàng đã được xác nhận không thể hủy trực tiếp. Vui lòng sử dụng chức năng hoàn tiền.");

        order.Status = Domain.Common.OrderStatus.Cancelled;

        _orderRepository.Update(order);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<List<OrderResponseDto>> GetExpiredOrdersAsync(CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetExpiredOrdersAsync(cancellationToken);
        return orders.Select(MapToResponseDto).ToList();
    }

    private static OrderResponseDto MapToResponseDto(Order order)
    {
        return new OrderResponseDto
        {
            Id = order.Id,
            UserId = order.UserId,
            TotalAmount = order.TotalAmount,
            Status = order.Status.ToString(),
            PaymentMethod = order.PaymentMethod,
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

