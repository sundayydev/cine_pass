using BE_CinePass.Core.Configurations;
using BE_CinePass.Core.Repositories;
using BE_CinePass.Core.Services;
using BE_CinePass.Shared.Common;
using BE_CinePass.Shared.Settings;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;
using BE_CinePass.Core.EventHandlers;
using BE_CinePass.Core.Services.BackgroundJobs;
using BE_CinePass.Domain.Events;
using Hangfire;
using Hangfire.MemoryStorage;

// Load environment variables từ file .env
DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();
// Hangfire
builder.Services.AddHangfire(config =>
{
    config
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseMemoryStorage();
});

builder.Services.AddHangfireServer();
// =======================
// Options
// =======================
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt"));

builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("Cloudinary"));

// Cấu hình Momo từ Environment Variables
builder.Services.Configure<MomoSettings>(options =>
{
    options.PartnerCode = Environment.GetEnvironmentVariable("MOMO_PARTNER_CODE") ?? "";
    options.AccessKey = Environment.GetEnvironmentVariable("MOMO_ACCESS_KEY") ?? "";
    options.SecretKey = Environment.GetEnvironmentVariable("MOMO_SECRET_KEY") ?? "";
    options.ReturnUrl = Environment.GetEnvironmentVariable("MOMO_RETURN_URL") ?? "";
    options.IpnUrl = Environment.GetEnvironmentVariable("MOMO_IPN_URL") ?? "";
    options.RequestType = Environment.GetEnvironmentVariable("MOMO_REQUEST_TYPE") ?? "captureWallet";
    options.ApiEndpoint = Environment.GetEnvironmentVariable("MOMO_API_ENDPOINT") ?? "";
    options.QueryEndpoint = Environment.GetEnvironmentVariable("MOMO_QUERY_ENDPOINT") ?? "";
    options.PosEndpoint = Environment.GetEnvironmentVariable("MOMO_POS_ENDPOINT") ?? "";
    options.Environment = Environment.GetEnvironmentVariable("MOMO_ENVIRONMENT") ?? "Development";
});

// =======================
// Database - PostgreSQL
// =======================
var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") ?? throw new InvalidOperationException("Database connection string is missing");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql =>
    {
        npgsql.MigrationsAssembly("BE_CinePass.API");
        npgsql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
    }));

// =======================
// Redis (Refresh Token / Seat Hold)
// =======================
var redisConnection = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING") ?? throw new InvalidOperationException("Redis connection string is missing");

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnection;
    options.InstanceName = "cinepass:";
});

// =======================
// JWT Authentication
// =======================
var jwtSettings = builder.Configuration
    .GetSection("Jwt")
    .Get<JwtSettings>()
    ?? throw new InvalidOperationException("Jwt settings not found");

var signingKey = new SymmetricSecurityKey(
    Encoding.UTF8.GetBytes(jwtSettings.SecretKey));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,

        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,

        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        IssuerSigningKey = signingKey,
        ClockSkew = TimeSpan.Zero,

        RoleClaimType = System.Security.Claims.ClaimTypes.Role
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception is SecurityTokenExpiredException)
            {
                context.Response.Headers.Add("Token-Expired", "true");
            }
            return Task.CompletedTask;
        }
    };
});

// =======================
// Authorization
// =======================
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AnyAuthenticated", policy =>
        policy.RequireAuthenticatedUser());
});

// =======================
// Repositories
// =======================
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<MovieRepository>();
builder.Services.AddScoped<CinemaRepository>();
builder.Services.AddScoped<ScreenRepository>();
builder.Services.AddScoped<SeatTypeRepository>();
builder.Services.AddScoped<SeatRepository>();
builder.Services.AddScoped<ShowtimeRepository>();
builder.Services.AddScoped<ProductRepository>();
builder.Services.AddScoped<OrderRepository>();
builder.Services.AddScoped<OrderTicketRepository>();
builder.Services.AddScoped<OrderProductRepository>();
builder.Services.AddScoped<ETicketRepository>();
builder.Services.AddScoped<PaymentTransactionRepository>();
builder.Services.AddScoped<MemberPointRepository>();
builder.Services.AddScoped<ActorRepository>();
builder.Services.AddScoped<MovieActorRepository>();
builder.Services.AddScoped<MovieReviewRepository>();
builder.Services.AddScoped<NotificationRepository>();
builder.Services.AddScoped<NotificationSettingsRepository>();
builder.Services.AddScoped<DeviceTokenRepository>();

// =======================
// Services
// =======================
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AuthTokenService>();
builder.Services.AddScoped<MovieService>();
builder.Services.AddScoped<CinemaService>();
builder.Services.AddScoped<ScreenService>();
builder.Services.AddScoped<SeatTypeService>();
builder.Services.AddScoped<SeatService>();
builder.Services.AddScoped<ShowtimeService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<ETicketService>();
builder.Services.AddScoped<PaymentTransactionService>();

// Membership & Voucher System Services
builder.Services.AddScoped<MemberTierConfigService>();
builder.Services.AddScoped<PointHistoryService>();
builder.Services.AddScoped<VoucherService>();
builder.Services.AddScoped<UserVoucherService>();
builder.Services.AddScoped<MemberPointService>();
// Other Services
builder.Services.AddScoped<ActorService>();
builder.Services.AddScoped<MovieActorService>();
builder.Services.AddScoped<SeatHoldService>();
builder.Services.AddScoped<CloudinaryService>();
builder.Services.AddScoped<MomoPaymentService>();
builder.Services.AddScoped<SeatFoodOrderService>();
builder.Services.AddScoped<MovieReviewService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<NotificationSettingsService>();
builder.Services.AddScoped<NotificationHelperService>();
builder.Services.AddSingleton<FirebaseMessagingService>();
builder.Services.AddScoped<PushNotificationService>();

builder.Services.AddSingleton<IEventBus, InMemoryEventBus>();
// Order event handlers
builder.Services.AddScoped<IEventHandler<OrderConfirmedEvent>, OrderConfirmedEventHandler>();
builder.Services.AddScoped<IEventHandler<OrderFailedEvent>, OrderFailedEventHandler>();
builder.Services.AddScoped<IEventHandler<PaymentSuccessEvent>, PaymentSuccessEventHandler>();
builder.Services.AddScoped<IEventHandler<PaymentFailedEvent>, PaymentFailedEventHandler>();
builder.Services.AddScoped<IEventHandler<OrderRefundedEvent>, OrderRefundedEventHandler>();

// Showtime event handlers
builder.Services.AddScoped<IEventHandler<ShowtimeCreatedEvent>, ShowtimeCreatedEventHandler>();
builder.Services.AddScoped<IEventHandler<ShowtimeCancelledEvent>, ShowtimeCancelledEventHandler>();
builder.Services.AddScoped<IEventHandler<ShowtimeTimeChangedEvent>, ShowtimeTimeChangedEventHandler>();
builder.Services.AddScoped<IEventHandler<MovieReleasedEvent>, MovieReleasedEventHandler>();

// Account event handlers
builder.Services.AddScoped<IEventHandler<VoucherReceivedEvent>, VoucherReceivedEventHandler>();
builder.Services.AddScoped<IEventHandler<VoucherExpiringSoonEvent>, VoucherExpiringSoonEventHandler>();
builder.Services.AddScoped<IEventHandler<PointsEarnedEvent>, PointsEarnedEventHandler>();
builder.Services.AddScoped<IEventHandler<BirthdayVoucherEvent>, BirthdayVoucherEventHandler>();
builder.Services.AddScoped<IEventHandler<SystemMaintenanceEvent>, SystemMaintenanceEventHandler>();
builder.Services.AddScoped<IEventHandler<SecurityAlertEvent>, SecurityAlertEventHandler>();

builder.Services.AddScoped<ShowtimeReminderJob>();
builder.Services.AddScoped<NotificationCleanupJob>();
builder.Services.AddScoped<VoucherExpiryCheckJob>();
builder.Services.AddScoped<BirthdayVoucherJob>();

// HttpClient Factory cho Momo và các external APIs
builder.Services.AddHttpClient();

// Momo Payment Service
builder.Services.AddScoped<MomoPaymentService>();

// =======================
// Swagger + JWT
// =======================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CinePass API",
        Version = "v1",
        Description = "API hệ thống đặt vé xem phim"
    });

    // Cấu hình nút Authorize (Ổ khóa)
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập token vào đây (không cần chữ Bearer, chỉ cần chuỗi token)"
    });

    // Bắt buộc Swagger gửi token kèm request
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// =======================
// CORS
// =======================
builder.Services.AddCors(options =>
{
    options.AddPolicy("CinePassCors", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// =======================
// SignalR
// =======================
builder.Services.AddSignalR();

var app = builder.Build();

// =======================
// Middleware
// =======================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("CinePassCors");
app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/hangfire");
// Clean up expired notifications daily at 2 AM
RecurringJob.AddOrUpdate<NotificationCleanupJob>(
    "cleanup-expired-notifications",
    job => job.CleanupExpiredNotificationsAsync(CancellationToken.None),
    Cron.Daily(2));

// Check voucher expiry daily at 9 AM
RecurringJob.AddOrUpdate<VoucherExpiryCheckJob>(
    "check-voucher-expiry",
    job => job.CheckVoucherExpiryAsync(CancellationToken.None),
    Cron.Daily(9));

// Birthday vouchers (nếu bật)
// RecurringJob.AddOrUpdate<BirthdayVoucherJob>(
//     "send-birthday-vouchers",
//     job => job.SendBirthdayVouchersAsync(CancellationToken.None),
//     Cron.Daily(8));

app.MapControllers();
app.MapHub<BE_CinePass.API.Hubs.SeatReservationHub>(
    "/hubs/seat-reservation");

app.Run();
