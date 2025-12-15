using BE_CinePass.Domain.Common;
using BE_CinePass.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BE_CinePass.Core.Configurations;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets
    public DbSet<User> Users { get; set; }
    public DbSet<Cinema> Cinemas { get; set; }
    public DbSet<Screen> Screens { get; set; }
    public DbSet<SeatType> SeatTypes { get; set; }
    public DbSet<Seat> Seats { get; set; }
    public DbSet<Movie> Movies { get; set; }
    public DbSet<Showtime> Showtimes { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderTicket> OrderTickets { get; set; }
    public DbSet<OrderProduct> OrderProducts { get; set; }
    public DbSet<ETicket> ETickets { get; set; }
    public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
    public DbSet<MemberPoint> MemberPoints { get; set; }
    public DbSet<Actor> Actors { get; set; }
    public DbSet<MovieActor> MovieActors { get; set; }
    public DbSet<MovieReview> MovieReviews { get; set; }
    public DbSet<Reward> Rewards { get; set; }
    public DbSet<PointHistory> PointHistories { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Role)
                .HasConversion(
                    v => v.ToString().ToLower(),
                    v => Enum.Parse<UserRole>(v, true))
                .HasMaxLength(20);
        });

        // Configure Cinema
        modelBuilder.Entity<Cinema>(entity =>
        {
            entity.HasMany(c => c.Screens)
                .WithOne(s => s.Cinema)
                .HasForeignKey(s => s.CinemaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Screen
        modelBuilder.Entity<Screen>(entity =>
        {
            entity.HasMany(s => s.Seats)
                .WithOne(seat => seat.Screen)
                .HasForeignKey(seat => seat.ScreenId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(s => s.Showtimes)
                .WithOne(st => st.Screen)
                .HasForeignKey(st => st.ScreenId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.SeatMapLayout)
                .HasColumnType("jsonb");
        });

        // Configure SeatType
        modelBuilder.Entity<SeatType>(entity =>
        {
            entity.HasKey(e => e.Code);
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.SurchargeRate)
                .HasColumnType("numeric(5,2)");
        });

        // Configure Seat
        modelBuilder.Entity<Seat>(entity =>
        {
            entity.HasIndex(e => new { e.ScreenId, e.SeatCode })
                .IsUnique();

            entity.HasOne(s => s.SeatType)
                .WithMany(st => st.Seats)
                .HasForeignKey(s => s.SeatTypeCode)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure Movie
        modelBuilder.Entity<Movie>(entity =>
        {
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.Property(e => e.Status)
                .HasConversion(
                    v => v.ToString().ToLower().Replace("ComingSoon", "coming_soon"),
                    v => Enum.Parse<MovieStatus>(v.Replace("coming_soon", "ComingSoon"), true))
                .HasMaxLength(20);
            entity.Property(e => e.ReleaseDate)
                .HasColumnType("date");
            entity.Property(e => e.Category)
                .HasConversion(
                    v => v.ToString().ToLower().Replace("Movie", "movie"),
                    v => Enum.Parse<MovieCategory>(v.Replace("movie", "Movie"), true))
                .HasMaxLength(20);
        });

        // Configure Showtime
        modelBuilder.Entity<Showtime>(entity =>
        {
            entity.HasIndex(e => e.MovieId);
            entity.HasIndex(e => e.StartTime);

            entity.HasOne(st => st.Movie)
                .WithMany(m => m.Showtimes)
                .HasForeignKey(st => st.MovieId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Product
        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(e => e.Category)
                .HasConversion(
                    v => v.ToString().ToLower(),
                    v => Enum.Parse<ProductCategory>(v, true))
                .HasMaxLength(20);
            entity.Property(e => e.Price)
                .HasColumnType("numeric(10,2)");
        });

        // Configure Order
        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(e => e.Status)
                .HasConversion(
                    v => v.ToString().ToLower(),
                    v => Enum.Parse<OrderStatus>(v, true))
                .HasMaxLength(20);
            entity.Property(e => e.TotalAmount)
                .HasColumnType("numeric(12,2)");

            entity.HasMany(o => o.OrderTickets)
                .WithOne(ot => ot.Order)
                .HasForeignKey(ot => ot.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(o => o.OrderProducts)
                .WithOne(op => op.Order)
                .HasForeignKey(op => op.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure OrderTicket
        modelBuilder.Entity<OrderTicket>(entity =>
        {
            entity.HasIndex(e => new { e.ShowtimeId, e.SeatId })
                .IsUnique();

            entity.Property(e => e.Price)
                .HasColumnType("numeric(10,2)");

            entity.HasOne(ot => ot.Showtime)
                .WithMany(st => st.OrderTickets)
                .HasForeignKey(ot => ot.ShowtimeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ot => ot.Seat)
                .WithMany(s => s.OrderTickets)
                .HasForeignKey(ot => ot.SeatId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure OrderProduct
        modelBuilder.Entity<OrderProduct>(entity =>
        {
            entity.Property(e => e.UnitPrice)
                .HasColumnType("numeric(10,2)");
        });

        // Configure ETicket
        modelBuilder.Entity<ETicket>(entity =>
        {
            entity.HasIndex(e => e.TicketCode).IsUnique();
            entity.HasIndex(e => e.OrderTicketId);

            entity.HasOne(et => et.OrderTicket)
                .WithMany(ot => ot.ETickets)
                .HasForeignKey(et => et.OrderTicketId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure PaymentTransaction
        modelBuilder.Entity<PaymentTransaction>(entity =>
        {
            entity.Property(e => e.Amount)
                .HasColumnType("numeric(12,2)");
            entity.Property(e => e.ResponseJson)
                .HasColumnType("jsonb");
        });
    }
}
