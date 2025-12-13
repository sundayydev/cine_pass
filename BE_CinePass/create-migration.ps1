# PowerShell script để tạo EF Core Migration
# Usage: .\create-migration.ps1 -MigrationName "InitialCreate"

param(
    [Parameter(Mandatory=$true)]
    [string]$MigrationName
)

Write-Host "Creating migration: $MigrationName" -ForegroundColor Green

# Chuyển đến thư mục API project
Set-Location "BE_CinePass\BE_CinePass.API"

# Chạy lệnh migration
dotnet ef migrations add $MigrationName --startup-project . --project .

if ($LASTEXITCODE -eq 0) {
    Write-Host "Migration created successfully!" -ForegroundColor Green
    Write-Host "Next step: Run 'dotnet ef database update' to apply migration" -ForegroundColor Yellow
} else {
    Write-Host "Migration creation failed!" -ForegroundColor Red
}

# Quay lại thư mục root
Set-Location ..\..

