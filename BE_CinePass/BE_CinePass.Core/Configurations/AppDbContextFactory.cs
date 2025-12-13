using BE_CinePass.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System;

namespace BE_CinePass.Core.Configurations;

public class AppDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Tìm thư mục chứa appsettings.json (thư mục API project)
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "BE_CinePass.API");
        if (!Directory.Exists(basePath))
        {
            // Nếu không tìm thấy, thử tìm từ thư mục hiện tại
            basePath = Directory.GetCurrentDirectory();
            // Nếu vẫn không có appsettings.json, thử tìm trong các thư mục con
            if (!File.Exists(Path.Combine(basePath, "appsettings.json")))
            {
                var apiPath = Path.Combine(basePath, "BE_CinePass.API");
                if (Directory.Exists(apiPath))
                    basePath = apiPath;
            }
        }

        // Tạo configuration để đọc appsettings.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // Lấy connection string từ appsettings.json
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found in appsettings.json");
        }

        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
        {
            // Set migrations assembly để migrations được tạo trong BE_CinePass.API project
            npgsqlOptions.MigrationsAssembly("BE_CinePass.API");
        });

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}   