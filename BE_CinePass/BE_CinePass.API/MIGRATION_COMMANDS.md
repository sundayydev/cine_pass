# Entity Framework Core Migration Commands

## 1. Tạo Migration đầu tiên (Initial Migration)

**Quan trọng**: `ApplicationDbContext` nằm trong `BE_CinePass.Core` nhưng migrations sẽ được tạo trong `BE_CinePass.API` (đã cấu hình trong `AppDbContextFactory` và `Program.cs`).

```bash
dotnet ef migrations add InitialCreate --project BE_CinePass.API --startup-project BE_CinePass.API
```

Hoặc nếu đang ở thư mục root:
```bash
cd BE_CinePass/BE_CinePass.API
dotnet ef migrations add InitialCreate
```

**Lưu ý**: Phải chỉ định `--project BE_CinePass.API` vì migrations assembly đã được set là `BE_CinePass.API`.

## 2. Tạo Migration mới (khi có thay đổi models)

```bash
dotnet ef migrations add MigrationName --project BE_CinePass.API --startup-project BE_CinePass.API
```

Ví dụ:
```bash
dotnet ef migrations add AddUserTable
dotnet ef migrations add UpdateOrderStatus
```

## 3. Xóa Migration cuối cùng (chưa apply)

```bash
dotnet ef migrations remove --project BE_CinePass.API --startup-project BE_CinePass.API
```

## 4. Apply Migration lên Database

```bash
dotnet ef database update --project BE_CinePass.API --startup-project BE_CinePass.API
```

## 5. Xem danh sách Migration đã apply

```bash
dotnet ef migrations list --project BE_CinePass.API --startup-project BE_CinePass.API
```

## 6. Rollback về Migration cụ thể

```bash
dotnet ef database update MigrationName --project BE_CinePass.API --startup-project BE_CinePass.API
```

Ví dụ rollback về migration trước:
```bash
dotnet ef database update InitialCreate --project BE_CinePass.API --startup-project BE_CinePass.API
```

## 7. Tạo SQL script từ Migration (không apply)

```bash
dotnet ef migrations script --project BE_CinePass.API --startup-project BE_CinePass.API --output migration.sql
```

Tạo script từ migration cụ thể đến migration khác:
```bash
dotnet ef migrations script InitialCreate LatestMigration --project BE_CinePass.API --startup-project BE_CinePass.API --output migration.sql
```

## 8. Xem SQL sẽ được execute (preview)

```bash
dotnet ef migrations script --project BE_CinePass.API --startup-project BE_CinePass.API --idempotent
```

## Lưu ý:

- **DesignTimeDbContextFactory**: Đã có `AppDbContextFactory` trong `BE_CinePass.Core/Configurations`, EF Core sẽ tự động sử dụng nó
- **Connection String**: Được đọc từ `appsettings.json` hoặc `appsettings.Development.json` trong thư mục API project
- **Migrations Assembly**: Đã được set là `BE_CinePass.API` trong cả `Program.cs` và `AppDbContextFactory`
  - `ApplicationDbContext` nằm trong `BE_CinePass.Core` nhưng migrations sẽ được tạo trong `BE_CinePass.API`
  - Điều này đảm bảo migrations và runtime configuration nhất quán
- **Auto-migrate**: Trong Development mode, migration sẽ tự động chạy khi start app (xem `Program.cs` dòng 40-55)

## Workflow thông thường:

1. Thay đổi Models trong `BE_CinePass.Domain/Models`
2. Tạo migration: `dotnet ef migrations add MigrationName`
3. Review migration files trong `Migrations/` folder
4. Apply: `dotnet ef database update`
5. Test ứng dụng

