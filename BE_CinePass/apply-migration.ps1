# PowerShell script để apply EF Core Migration lên database
# Usage: .\apply-migration.ps1

Write-Host "Applying migrations to database..." -ForegroundColor Green

# Chuyển đến thư mục API project
Set-Location "BE_CinePass\BE_CinePass.API"

# Chạy lệnh update database
dotnet ef database update --startup-project . --project .

if ($LASTEXITCODE -eq 0) {
    Write-Host "Database updated successfully!" -ForegroundColor Green
} else {
    Write-Host "Database update failed!" -ForegroundColor Red
}

# Quay lại thư mục root
Set-Location ..\..

