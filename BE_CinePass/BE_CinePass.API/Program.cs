using BE_CinePass.Core.Configurations;
using BE_CinePass.Core.Repositories;
using BE_CinePass.Core.Services;
using BE_CinePass.Shared.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add Controllers
builder.Services.AddControllers();

// Options
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

// Add PostgreSQL + EF Core
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.MigrationsAssembly("BE_CinePass.API");
        npgsqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
    }));

// Redis for refresh tokens
var redisConnection = builder.Configuration.GetConnectionString("Redis");
if (string.IsNullOrWhiteSpace(redisConnection))
{
    throw new InvalidOperationException("ConnectionStrings:Redis is not configured.");
}

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnection;
    options.InstanceName = "cinepass:";
});

// Authentication + JWT
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey))
{
    throw new InvalidOperationException("Jwt:SecretKey is not configured.");
}

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey));

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

// Repositories
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

// Services
builder.Services.AddScoped<UserService>();
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
builder.Services.AddScoped<AuthTokenService>();
builder.Services.AddScoped<MemberPointService>();
builder.Services.AddScoped<ActorService>();
builder.Services.AddScoped<MovieActorService>();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "CinePass API",
        Description = "API hệ thống đặt vé xem phim",
        Contact = new OpenApiContact
        {
            Name = "CinePass Support",
            Email = "support@cinepass.com"
        }
    });

    // XML docs
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }

    // JWT
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    options.CustomSchemaIds(type => type.FullName);
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("CinePassCors", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Auto Migrate
// using (var scope = app.Services.CreateScope())
// {
//     var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//     await db.Database.MigrateAsync();
// }

// Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CinePass API v1");
        c.RoutePrefix = "swagger"; // UI at /swagger
        c.DisplayRequestDuration();
    });
}

app.UseHttpsRedirection();
app.UseCors("CinePassCors");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
