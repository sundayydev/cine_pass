using Microsoft.AspNetCore.SignalR;

namespace BE_CinePass.API.Hubs;

/// <summary>
/// SignalR Hub for real-time seat reservation management
/// </summary>
public class SeatReservationHub : Hub
{
    private readonly ILogger<SeatReservationHub> _logger;

    public SeatReservationHub(ILogger<SeatReservationHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Join a showtime group to receive real-time updates
    /// </summary>
    public async Task JoinShowtime(string showtimeId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"showtime_{showtimeId}");
        _logger.LogInformation("Client {ConnectionId} joined showtime {ShowtimeId}", Context.ConnectionId, showtimeId);
    }

    /// <summary>
    /// Leave a showtime group
    /// </summary>
    public async Task LeaveShowtime(string showtimeId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"showtime_{showtimeId}");
        _logger.LogInformation("Client {ConnectionId} left showtime {ShowtimeId}", Context.ConnectionId, showtimeId);
    }

    /// <summary>
    /// Hold seats temporarily (called when user selects seats)
    /// </summary>
    public async Task HoldSeats(string showtimeId, List<string> seatIds, string userId)
    {
        // Notify all clients in the showtime group about the seat hold
        await Clients.OthersInGroup($"showtime_{showtimeId}")
            .SendAsync("SeatsHeld", new { SeatIds = seatIds, UserId = userId });

        _logger.LogInformation("User {UserId} held seats {SeatIds} in showtime {ShowtimeId}",
            userId, string.Join(",", seatIds), showtimeId);
    }

    /// <summary>
    /// Release held seats (called when user deselects or times out)
    /// </summary>
    public async Task ReleaseSeats(string showtimeId, List<string> seatIds)
    {
        // Notify all clients in the showtime group about the seat release
        await Clients.OthersInGroup($"showtime_{showtimeId}")
            .SendAsync("SeatsReleased", new { SeatIds = seatIds });

        _logger.LogInformation("Seats {SeatIds} released in showtime {ShowtimeId}",
            string.Join(",", seatIds), showtimeId);
    }

    /// <summary>
    /// Confirm seat purchase (called after successful payment)
    /// </summary>
    public async Task ConfirmSeats(string showtimeId, List<string> seatIds)
    {
        // Notify all clients in the showtime group about the confirmed purchase
        await Clients.Group($"showtime_{showtimeId}")
            .SendAsync("SeatsConfirmed", new { SeatIds = seatIds });

        _logger.LogInformation("Seats {SeatIds} confirmed in showtime {ShowtimeId}",
            string.Join(",", seatIds), showtimeId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client {ConnectionId} disconnected", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}
