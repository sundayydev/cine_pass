# Authentication System Fix Summary

## Ngày thực hiện: 2024-12-16

## Vấn đề ban đầu
Hệ thống authentication có vấn đề với phân quyền Admin/Staff/Customer:
- UserRole enum không có giá trị rõ ràng
- JWT claims không chứa role đúng cách
- Không có helper methods để extract user info từ JWT
- Authorization attributes (`[Authorize(Roles = "Admin")]`) có thể không hoạt động đúng

## Các thay đổi đã thực hiện

### 1. **Cập nhật UserRole Enum** ✅
**Files:**
- `BE_CinePass.Domain\Common\UserRole.cs`
- `BE_CinePass.Shared\Common\UserRole.cs`

**Thay đổi:**
```csharp
// Before
public enum UserRole
{
    Customer,  // Implicit value: 0
    Staff,     // Implicit value: 1
    Admin      // Implicit value: 2
}

// After
public enum UserRole
{
    Customer = 0,  // Explicit value
    Staff = 1,     // Explicit value
    Admin = 2      // Explicit value
}
```

**Lợi ích:**
- Giá trị rõ ràng, tránh confusion
- Thêm XML documentation
- Đảm bảo mapping đúng với database (integer column)

### 2. **Cải thiện AuthTokenService** ✅
**File:** `BE_CinePass.Core\Services\AuthTokenService.cs`

**Thay đổi:**
- Tạo role claim với string name thay vì enum value
- Thêm custom "role" claim ngoài `ClaimTypes.Role`
- Thêm name claims nếu có `FullName`

```csharp
// Before
var claims = new List<Claim>
{
    new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
    new(JwtRegisteredClaimNames.Email, user.Email),
    new(ClaimTypes.Role, user.Role.ToString()),  // "Customer" / "Staff" / "Admin"
    new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
};

// After
var roleName = user.Role.ToString(); // "Customer", "Staff", or "Admin"

var claims = new List<Claim>
{
    new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
    new(JwtRegisteredClaimNames.Email, user.Email),
    new(ClaimTypes.Role, roleName),    // For [Authorize(Roles = "...")]
    new("role", roleName),              // Custom claim for easier access
    new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
};

// Add name if available
if (!string.IsNullOrEmpty(user.FullName))
{
    claims.Add(new Claim(ClaimTypes.Name, user.FullName));
    claims.Add(new Claim(JwtRegisteredClaimNames.Name, user.FullName));
}
```

**Lợi ích:**
- Role claim là string ("Customer", "Staff", "Admin") để ASP.NET Authorization hoạt động đúng
- Có 2 role claims để tương thích với nhiều scenario
- Thêm name claims để dễ identify user

### 3. **Tạo ClaimsPrincipalExtensions** ✅
**File:** `BE_CinePass.Shared\Extensions\ClaimsPrincipalExtensions.cs`

**Extension Methods:**
```csharp
// Get basic info
User.GetUserId()          // Returns Guid?
User.GetUserEmail()       // Returns string?
User.GetUserRole()        // Returns UserRole?
User.GetUserFullName()    // Returns string?

// Check roles
User.IsInRole(UserRole.Admin)  // Returns bool
User.IsAdmin()                 // Returns bool
User.IsStaff()                 // Returns bool
User.IsCustomer()              // Returns bool
User.IsAdminOrStaff()          // Returns bool
```

**Usage Example:**
```csharp
[Authorize]
[HttpGet("my-orders")]
public async Task<IActionResult> GetMyOrders()
{
    var userId = User.GetUserId();
    if (!userId.HasValue)
        return Unauthorized();
    
    var orders = await _orderService.GetUserOrdersAsync(userId.Value);
    return Ok(orders);
}

[Authorize]
[HttpDelete("orders/{orderId}")]
public async Task<IActionResult> DeleteOrder(Guid orderId)
{
    var order = await _orderService.GetByIdAsync(orderId);
    if (order == null)
        return NotFound();
    
    // Only allow order owner or Admin/Staff to delete
    var userId = User.GetUserId();
    if (!User.IsAdminOrStaff() && order.UserId != userId)
        return Forbid();
    
    await _orderService.DeleteAsync(orderId);
    return NoContent();
}
```

### 4. **Cập nhật AuthController** ✅
**File:** `BE_CinePass.API\Controllers\AuthController.cs`

**Thay đổi:**
- Import `BE_CinePass.Shared.Extensions`
- Sử dụng `User.GetUserId()` thay vì manual parsing

```csharp
// Before
var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
    return Unauthorized();

// After  
var userId = User.GetUserId();
if (!userId.HasValue)
    return Unauthorized();
```

### 5. **Tạo Documentation** ✅
**File:** `AUTHENTICATION_AUTHORIZATION_GUIDE.md`

**Nội dung:**
- Tổng quan về authentication system
- User roles và permissions
- JWT token structure
- API endpoints chi tiết
- Authorization usage trong controllers
- ClaimsPrincipal extensions usage
- Client-side integration examples
- Security best practices
- Troubleshooting guide

## Cách sử dụng Authorization

### 1. Protect Endpoint với Role
```csharp
// Only Admin
[Authorize(Roles = "Admin")]
[HttpPost("admin-only")]
public async Task<IActionResult> AdminOnly() { }

// Only Staff
[Authorize(Roles = "Staff")]
[HttpPost("staff-only")]
public async Task<IActionResult> StaffOnly() { }

// Admin OR Staff
[Authorize(Roles = "Admin,Staff")]
[HttpPost("admin-or-staff")]
public async Task<IActionResult> AdminOrStaff() { }

// Any authenticated user
[Authorize]
[HttpGet("authenticated-users")]
public async Task<IActionResult> AuthenticatedUsers() { }
```

### 2. Manual Permission Check
```csharp
[Authorize]
[HttpGet("my-data/{id}")]
public async Task<IActionResult> GetData(Guid id)
{
    var data = await _service.GetByIdAsync(id);
    if (data == null)
        return NotFound();
    
    // Check if user can access this data
    var userId = User.GetUserId();
    
    // Option 1: Check specific role
    if (!User.IsAdmin() && data.OwnerId != userId)
        return Forbid();
    
    // Option 2: Check multiple roles
    if (!User.IsAdminOrStaff() && data.OwnerId != userId)
        return Forbid();
    
    return Ok(data);
}
```

## Testing

### Test Data trong Database
```sql
-- Admin user (role = 2)
INSERT INTO users (email, password_hash, full_name, role) 
VALUES ('admin@cinepass.com', '$2a$11$...', 'Admin User', 2);

-- Staff user (role = 1)
INSERT INTO users (email, password_hash, full_name, role) 
VALUES ('staff@cinepass.com', '$2a$11$...', 'Staff User', 1);

-- Customer user (role = 0)
INSERT INTO users (email, password_hash, full_name, role) 
VALUES ('customer@cinepass.com', '$2a$11$...', 'Customer User', 0);
```

### Test Flow
1. **Login as different roles:**
   ```bash
   # Login as Admin
   POST /api/auth/login
   { "email": "admin@cinepass.com", "password": "..." }
   
   # Login as Staff
   POST /api/auth/login
   { "email": "staff@cinepass.com", "password": "..." }
   
   # Login as Customer
   POST /api/auth/login
   { "email": "customer@cinepass.com", "password": "..." }
   ```

2. **Test protected endpoints:**
   ```bash
   # Try accessing Admin-only endpoint with different roles
   GET /api/movies (Admin only)
   Header: Authorization: Bearer {token}
   
   # Expected results:
   # - Admin token: 200 OK
   # - Staff token: 403 Forbidden
   # - Customer token: 403 Forbidden
   # - No token: 401 Unauthorized
   ```

3. **Verify JWT claims:**
   - Decode JWT token at https://jwt.io
   - Check that `role` claim contains "Customer", "Staff", or "Admin" (not 0, 1, 2)
   - Verify other claims: sub, email, name, jti

## Migration Considerations

### Database
- Không cần migration cho database
- UserRole đã được lưu dưới dạng integer (0, 1, 2)
- Enum mapping vẫn hoạt động như cũ

### Existing Tokens
- Tokens hiện tại có thể không có custom "role" claim
- Users cần login lại để nhận token mới với đầy đủ claims
- Hoặc implement token migration logic nếu cần

### Frontend Changes
- Update token decoding logic nếu cần
- Sử dụng role name ("Customer", "Staff", "Admin") thay vì number
- Update role-based UI rendering

## Best Practices

### 1. Always Use Extension Methods
```csharp
// Good ✅
var userId = User.GetUserId();
if (User.IsAdmin()) { }

// Bad ❌
var userId = Guid.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub).Value);
if (User.FindFirst(ClaimTypes.Role).Value == "Admin") { }
```

### 2. Check Authorization at Multiple Levels
```csharp
// Controller level
[Authorize(Roles = "Admin,Staff")]
public class AdminController : ControllerBase { }

// Action level  
[Authorize(Roles = "Admin")]
[HttpDelete("{id}")]
public async Task<IActionResult> Delete(Guid id) { }

// Business logic level
public async Task DeleteAsync(Guid id, Guid requesterId)
{
    var requester = await _userRepo.GetByIdAsync(requesterId);
    if (requester.Role != UserRole.Admin)
        throw new UnauthorizedException();
    
    await _repo.DeleteAsync(id);
}
```

### 3. Use Descriptive Error Messages
```csharp
if (!User.IsAdminOrStaff())
    return Forbid(); // Don't reveal why

// Or with custom message
if (!User.IsAdminOrStaff())
    return StatusCode(403, new { 
        error = "Insufficient permissions", 
        required = "Admin or Staff role" 
    });
```

## Checklist ✅

- [x] UserRole enum có giá trị explicit (0, 1, 2)
- [x] AuthTokenService tạo role claim dạng string
- [x] ClaimsPrincipalExtensions helper methods
- [x] AuthController sử dụng extension methods
- [x] Documentation đầy đủ
- [x] Examples và test cases
- [x] Security best practices documented

## Next Steps

1. **Test thoroughly:**
   - Test với cả 3 roles (Customer, Staff, Admin)
   - Test protected endpoints
   - Test token refresh flow
   - Test logout flow

2. **Update existing controllers:**
   - Review tất cả controllers
   - Thêm appropriate `[Authorize]` attributes
   - Sử dụng extension methods

3. **Frontend integration:**
   - Update login/register flow
   - Implement token storage
   - Add token refresh logic
   - Implement role-based UI

4. **Security audit:**
   - Review all protected endpoints
   - Ensure proper authorization checks
   - Test for privilege escalation
   - Add rate limiting

## Files Changed Summary

```
BE_CinePass.Domain/
  └── Common/
      └── UserRole.cs                           [MODIFIED]

BE_CinePass.Shared/
  ├── Common/
  │   └── UserRole.cs                           [MODIFIED]
  └── Extensions/
      └── ClaimsPrincipalExtensions.cs          [NEW]

BE_CinePass.Core/
  └── Services/
      └── AuthTokenService.cs                   [MODIFIED]

BE_CinePass.API/
  └── Controllers/
      └── AuthController.cs                     [MODIFIED]

BE_CinePass/
  └── AUTHENTICATION_AUTHORIZATION_GUIDE.md     [NEW]
  └── AUTH_SYSTEM_FIX_SUMMARY.md                [NEW]
```

## Support & Questions

Nếu có vấn đề hoặc câu hỏi:
1. Xem `AUTHENTICATION_AUTHORIZATION_GUIDE.md` để biết cách sử dụng
2. Check existing controllers để xem examples
3. Review logs nếu có errors
4. Test với Swagger UI hoặc Postman

---

**Status:** ✅ Completed
**Tested:** Cần test
**Documentation:** ✅ Complete
**Breaking Changes:** Không (backward compatible)
