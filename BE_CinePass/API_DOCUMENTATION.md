# 📚 CinePass API Documentation

> **Base URL:** `/api`
> **Version:** 1.0
> **Ngày cập nhật:** 2025-12-22

---

## 📑 Mục lục

1. [🎭 Actors API](#1-actors-api)
2. [🔐 Auth API](#2-auth-api)
3. [🎬 Cinemas API](#3-cinemas-api)
4. [🎫 E-Tickets API](#4-e-tickets-api)
5. [💳 Momo Payment API](#5-momo-payment-api)
6. [🎭 Movie Actors API](#6-movie-actors-api)
7. [🎥 Movies API](#7-movies-api)
8. [📦 Orders API](#8-orders-api)
9. [💰 Payment Transactions API](#9-payment-transactions-api)
10. [🍿 Products API](#10-products-api)
11. [🖥️ Screens API](#11-screens-api)
12. [🪑 Seat Types API](#12-seat-types-api)
13. [💺 Seats API](#13-seats-api)
14. [🕐 Showtimes API](#14-showtimes-api)
15. [👨‍💼 Staff API](#15-staff-api)
16. [📤 Upload API](#16-upload-api)
17. [👥 Users API](#17-users-api)

---

## 1. 🎭 Actors API

**Base Route:** `/api/actors`

Quản lý thông tin diễn viên trong hệ thống.

| Method | Endpoint | Mô tả | Auth |
|--------|----------|-------|------|
| `GET` | `/` | Lấy danh sách tất cả diễn viên | ❌ |
| `GET` | `/{id}` | Lấy thông tin diễn viên theo ID | ❌ |
| `GET` | `/slug/{slug}` | Lấy thông tin diễn viên theo slug | ❌ |
| `GET` | `/search?searchTerm={term}` | Tìm kiếm diễn viên | ❌ |
| `POST` | `/` | Tạo diễn viên mới | ❌ |
| `PUT` | `/{id}` | Cập nhật thông tin diễn viên | ❌ |
| `DELETE` | `/{id}` | Xóa diễn viên | ❌ |

### Request/Response Examples

#### GET /api/actors
```json
// Response
{
  "success": true,
  "message": "",
  "data": [
    {
      "id": "guid",
      "name": "string",
      "slug": "string",
      "bio": "string",
      "photoUrl": "string"
    }
  ]
}
```

#### POST /api/actors
```json
// Request Body (ActorCreateDto)
{
  "name": "string",
  "bio": "string",
  "photoUrl": "string"
}
```

---

## 2. 🔐 Auth API

**Base Route:** `/api/auth`

Quản lý xác thực và phân quyền người dùng.

| Method | Endpoint | Mô tả | Auth |
|--------|----------|-------|------|
| `POST` | `/register` | Đăng ký tài khoản mới | ❌ |
| `POST` | `/login` | Đăng nhập | ❌ |
| `POST` | `/refresh` | Làm mới access token từ refresh token | ❌ |
| `POST` | `/logout` | Đăng xuất và thu hồi refresh token | ❌ |
| `GET` | `/me` | Lấy thông tin người dùng hiện tại (Profile, Points) | ✅ |
| `GET` | `/test-auth` | Test endpoint để verify JWT authentication | ✅ |
| `GET` | `/user/{id}` | Lấy thông tin người dùng theo ID (internal) | ❌ |

### Request/Response Examples

#### POST /api/auth/register
```json
// Request Body (UserCreateDto)
{
  "email": "string",
  "password": "string",
  "fullName": "string",
  "phoneNumber": "string"
}

// Response
{
  "success": true,
  "message": "Đăng ký thành công",
  "data": {
    "accessToken": "string",
    "refreshToken": "string",
    "expiresAt": "datetime"
  }
}
```

#### POST /api/auth/login
```json
// Request Body (UserLoginDto)
{
  "email": "string",
  "password": "string"
}

// Response
{
  "success": true,
  "message": "Đăng nhập thành công",
  "data": {
    "accessToken": "string",
    "refreshToken": "string",
    "expiresAt": "datetime"
  }
}
```

#### POST /api/auth/refresh
```json
// Request Body (RefreshTokenRequestDto)
{
  "refreshToken": "string"
}
```

#### GET /api/auth/me (🔒 Require Auth)
```json
// Response (AuthMeResponseDto)
{
  "success": true,
  "data": {
    "profile": {
      "id": "guid",
      "email": "string",
      "fullName": "string",
      "role": "Customer|Staff|Admin"
    },
    "points": {
      "points": 0,
      "tier": "string"
    }
  }
}
```

---

## 3. 🎬 Cinemas API

**Base Route:** `/api/cinemas`

Quản lý thông tin rạp chiếu phim.

| Method | Endpoint | Mô tả | Auth |
|--------|----------|-------|------|
| `GET` | `/` | Lấy danh sách tất cả rạp chiếu phim | ❌ |
| `GET` | `/active` | Lấy danh sách rạp đang hoạt động | ❌ |
| `GET` | `/city/{city}` | Lấy danh sách rạp theo thành phố | ❌ |
| `GET` | `/{id}` | Lấy thông tin rạp theo ID | ❌ |
| `GET` | `/{cinemaId}/movies` | Lấy danh sách phim đang chiếu tại rạp | ❌ |
| `GET` | `/{cinemaId}/detail` | Lấy thông tin chi tiết rạp kèm phim đang chiếu | ❌ |
| `GET` | `/with-movies` | Lấy danh sách tất cả rạp kèm phim đang chiếu | ❌ |
| `GET` | `/{cinemaId}/movies-with-showtimes` | Lấy phim kèm lịch chiếu tại rạp | ❌ |
| `GET` | `/{cinemaId}/movies-with-showtimes/by-date?date={date}` | Lấy phim kèm lịch chiếu theo ngày | ❌ |
| `GET` | `/brands` | Lấy danh sách các brand rạp | ❌ |
| `POST` | `/` | Tạo rạp chiếu phim mới | ❌ |
| `PUT` | `/{id}` | Cập nhật thông tin rạp | ❌ |
| `DELETE` | `/{id}` | Xóa rạp chiếu phim | ❌ |

### Request/Response Examples

#### GET /api/cinemas/{cinemaId}/movies-with-showtimes
```json
// Response (CinemaMoviesWithShowtimesResponseDto)
{
  "success": true,
  "data": {
    "cinema": { "id": "guid", "name": "string" },
    "movies": [
      {
        "movie": { "id": "guid", "title": "string" },
        "showtimes": [
          { "id": "guid", "startTime": "datetime", "basePrice": 100000 }
        ]
      }
    ]
  }
}
```

#### POST /api/cinemas
```json
// Request Body (CinemaCreateDto)
{
  "name": "string",
  "address": "string",
  "city": "string",
  "phoneNumber": "string",
  "isActive": true
}
```

---

## 4. 🎫 E-Tickets API

**Base Route:** `/api/etickets`

Quản lý vé điện tử.

| Method | Endpoint | Mô tả | Auth |
|--------|----------|-------|------|
| `GET` | `/{id}` | Lấy thông tin vé theo ID | ❌ |
| `GET` | `/code/{ticketCode}` | Lấy thông tin vé theo mã vé | ❌ |
| `GET` | `/code/{ticketCode}/detail` | Lấy chi tiết vé theo mã vé | ❌ |
| `GET` | `/order-ticket/{orderTicketId}` | Lấy danh sách vé theo order ticket ID | ❌ |
| `GET` | `/validate/{ticketCode}` | Xác thực vé điện tử | ❌ |
| `POST` | `/generate/{orderTicketId}` | Tạo vé điện tử (sau thanh toán thành công) | ❌ |
| `POST` | `/use/{ticketCode}` | Sử dụng vé (check-in) | ❌ |
| `POST` | `/checkin` | Check-in vé bằng mã QR | ❌ |

### Request/Response Examples

#### POST /api/etickets/checkin
```json
// Request Body (VerifyTicketDto)
{
  "qrData": "string"
}

// Response (TicketVerificationResultDto)
{
  "success": true,
  "data": {
    "isValid": true,
    "status": "Valid|Used|Expired|NotFound",
    "message": "string",
    "ticketDetail": {
      "ticketCode": "string",
      "movieTitle": "string",
      "cinemaName": "string",
      "showtime": "datetime",
      "seatCode": "string"
    }
  }
}
```

---

## 5. 💳 Momo Payment API

**Base Route:** `/api/momopayment`

Xử lý thanh toán qua ví Momo.

| Method | Endpoint | Mô tả | Auth |
|--------|----------|-------|------|
| `POST` | `/create` | Tạo giao dịch thanh toán Momo | ✅ Customer |
| `GET` | `/callback` | Callback từ Momo (Return URL) | ❌ |
| `POST` | `/ipn` | IPN (Instant Payment Notification) từ Momo | ❌ |
| `GET` | `/query/{orderId}?requestId={requestId}` | Truy vấn trạng thái giao dịch | ✅ |
| `GET` | `/config` | Lấy thông tin cấu hình Momo (debug) | ✅ Admin |

### Request/Response Examples

#### POST /api/momopayment/create (🔒 Role: Customer)
```json
// Request Body (CreateMomoPaymentRequest)
{
  "orderId": "guid",
  "amount": 150000,
  "orderInfo": "Thanh toán đơn hàng #123",
  "extraData": ""
}

// Response (CreateMomoPaymentResponse)
{
  "success": true,
  "message": "Tạo thanh toán thành công",
  "payUrl": "https://momo.vn/...",
  "deeplink": "momo://app?...",
  "qrCodeUrl": "https://...",
  "orderId": "string",
  "requestId": "string",
  "resultCode": 0
}
```

---

## 6. 🎭 Movie Actors API

**Base Route:** `/api/movieactors`

Quản lý mối quan hệ giữa phim và diễn viên.

| Method | Endpoint | Mô tả | Auth |
|--------|----------|-------|------|
| `GET` | `/` | Lấy danh sách tất cả mối quan hệ phim-diễn viên | ❌ |
| `GET` | `/{id}` | Lấy thông tin mối quan hệ theo ID | ❌ |
| `GET` | `/movie/{movieId}` | Lấy danh sách diễn viên của một phim | ❌ |
| `GET` | `/actor/{actorId}` | Lấy danh sách phim của một diễn viên | ❌ |
| `POST` | `/` | Thêm diễn viên vào phim | ❌ |
| `DELETE` | `/{id}` | Xóa diễn viên khỏi phim | ❌ |

### Request/Response Examples

#### POST /api/movieactors
```json
// Request Body (MovieActorCreateDto)
{
  "movieId": "guid",
  "actorId": "guid",
  "characterName": "string",
  "order": 1
}
```

---

## 7. 🎥 Movies API

**Base Route:** `/api/movies`

Quản lý thông tin phim.

| Method | Endpoint | Mô tả | Auth |
|--------|----------|-------|------|
| `GET` | `/` | Lấy danh sách tất cả phim | ❌ |
| `GET` | `/{id}` | Lấy thông tin chi tiết phim theo ID | ❌ |
| `GET` | `/slug/{slug}` | Lấy thông tin chi tiết phim theo slug | ❌ |
| `GET` | `/now-showing` | Lấy danh sách phim đang chiếu | ❌ |
| `GET` | `/coming-soon` | Lấy danh sách phim sắp chiếu | ❌ |
| `GET` | `/search?searchTerm={term}` | Tìm kiếm phim | ❌ |
| `GET` | `/{id}/cinemas?date={date}` | Lấy danh sách rạp có lịch chiếu phim | ❌ |
| `POST` | `/` | Tạo phim mới | ❌ |
| `PUT` | `/{id}` | Cập nhật thông tin phim | ❌ |
| `DELETE` | `/{id}` | Xóa phim | ❌ |

### Request/Response Examples

#### GET /api/movies/{id}
```json
// Response (MovieDetailResponseDto)
{
  "success": true,
  "data": {
    "id": "guid",
    "title": "string",
    "slug": "string",
    "description": "string",
    "duration": 120,
    "releaseDate": "2025-01-01",
    "posterUrl": "string",
    "trailerUrl": "string",
    "genre": "string",
    "director": "string",
    "rating": 8.5,
    "actors": [
      { "id": "guid", "name": "string", "characterName": "string" }
    ]
  }
}
```

#### POST /api/movies
```json
// Request Body (MovieCreateDto)
{
  "title": "string",
  "description": "string",
  "duration": 120,
  "releaseDate": "2025-01-01",
  "posterUrl": "string",
  "trailerUrl": "string",
  "genre": "string",
  "director": "string"
}
```

#### GET /api/movies/{id}/cinemas?date={date}
```json
// Response (MovieCinemasWithShowtimesResponseDto)
{
  "success": true,
  "data": {
    "cinemas": [
      {
        "cinema": { "id": "guid", "name": "string" },
        "showtimes": [
          { "id": "guid", "startTime": "datetime", "screenName": "string" }
        ]
      }
    ]
  }
}
```

---

## 8. 📦 Orders API

**Base Route:** `/api/orders`

Quản lý đơn hàng.

| Method | Endpoint | Mô tả | Auth |
|--------|----------|-------|------|
| `GET` | `/` | Lấy danh sách tất cả đơn hàng | ❌ |
| `GET` | `/{id}` | Lấy thông tin đơn hàng theo ID | ❌ |
| `GET` | `/{id}/detail` | Lấy chi tiết đơn hàng (tickets + products) | ❌ |
| `GET` | `/user/{userId}` | Lấy danh sách đơn hàng theo người dùng | ❌ |
| `GET` | `/status/{status}` | Lấy danh sách đơn hàng theo trạng thái | ❌ |
| `GET` | `/expired` | Lấy danh sách đơn hàng đã hết hạn | ❌ |
| `POST` | `/` | Tạo đơn hàng mới | ❌ |
| `POST` | `/{id}/confirm` | Xác nhận đơn hàng (sau thanh toán) | ❌ |
| `POST` | `/{id}/cancel` | Hủy đơn hàng | ❌ |
| `PUT` | `/{id}` | Cập nhật đơn hàng | ❌ |

### Order Status Enum
- `Pending` - Đang chờ thanh toán
- `Confirmed` - Đã xác nhận
- `Cancelled` - Đã hủy
- `Expired` - Đã hết hạn

### Request/Response Examples

#### POST /api/orders
```json
// Request Body (OrderCreateDto)
{
  "userId": "guid",
  "showtimeId": "guid",
  "tickets": [
    { "seatId": "guid", "price": 100000 }
  ],
  "products": [
    { "productId": "guid", "quantity": 2 }
  ]
}

// Response (OrderResponseDto)
{
  "success": true,
  "message": "Tạo đơn hàng thành công",
  "data": {
    "id": "guid",
    "userId": "guid",
    "totalAmount": 250000,
    "status": "Pending",
    "createdAt": "datetime"
  }
}
```

#### GET /api/orders/{id}/detail
```json
// Response (OrderDetailDto)
{
  "success": true,
  "data": {
    "id": "guid",
    "userId": "guid",
    "totalAmount": 250000,
    "status": "Confirmed",
    "tickets": [
      {
        "seatCode": "A1",
        "price": 100000,
        "showtime": { "startTime": "datetime", "movieTitle": "string" },
        "eTicketCode": "string"
      }
    ],
    "products": [
      { "productName": "Popcorn", "quantity": 2, "price": 50000 }
    ],
    "cinema": {
      "name": "string",
      "address": "string"
    }
  }
}
```

---

## 9. 💰 Payment Transactions API

**Base Route:** `/api/paymenttransactions`

Quản lý giao dịch thanh toán.

| Method | Endpoint | Mô tả | Auth |
|--------|----------|-------|------|
| `GET` | `/{id}` | Lấy thông tin giao dịch theo ID | ❌ |
| `GET` | `/order/{orderId}` | Lấy danh sách giao dịch theo đơn hàng | ❌ |
| `GET` | `/provider/{providerTransId}` | Lấy giao dịch theo mã nhà cung cấp | ❌ |
| `GET` | `/successful` | Lấy danh sách giao dịch thành công | ❌ |
| `GET` | `/failed` | Lấy danh sách giao dịch thất bại | ❌ |
| `POST` | `/` | Tạo giao dịch thanh toán mới | ❌ |
| `PUT` | `/{id}/status` | Cập nhật trạng thái giao dịch | ❌ |

### Transaction Status
- `Pending` - Đang xử lý
- `Completed` - Hoàn thành
- `Failed` - Thất bại

---

## 10. 🍿 Products API

**Base Route:** `/api/products`

Quản lý sản phẩm (đồ ăn, nước uống, combo).

| Method | Endpoint | Mô tả | Auth |
|--------|----------|-------|------|
| `GET` | `/` | Lấy danh sách tất cả sản phẩm | ❌ |
| `GET` | `/active` | Lấy danh sách sản phẩm đang hoạt động | ❌ |
| `GET` | `/category/{category}` | Lấy danh sách sản phẩm theo danh mục | ❌ |
| `GET` | `/search?searchTerm={term}` | Tìm kiếm sản phẩm | ❌ |
| `GET` | `/{id}` | Lấy thông tin sản phẩm theo ID | ❌ |
| `POST` | `/` | Tạo sản phẩm mới | ❌ |
| `PUT` | `/{id}` | Cập nhật thông tin sản phẩm | ❌ |
| `DELETE` | `/{id}` | Xóa sản phẩm | ❌ |

### Product Category Enum
- `0` - Food (Đồ ăn)
- `1` - Beverage (Nước uống)
- `2` - Combo

### Request/Response Examples

#### POST /api/products
```json
// Request Body (ProductCreateDto)
{
  "name": "Popcorn Large",
  "description": "Bắp rang bơ size lớn",
  "price": 65000,
  "imageUrl": "string",
  "category": 0,
  "isActive": true
}
```

---

## 11. 🖥️ Screens API

**Base Route:** `/api/screens`

Quản lý phòng chiếu (màn hình).

| Method | Endpoint | Mô tả | Auth |
|--------|----------|-------|------|
| `GET` | `/cinema/{cinemaId}` | Lấy danh sách phòng chiếu theo rạp | ❌ |
| `GET` | `/{id}` | Lấy thông tin phòng chiếu theo ID | ❌ |
| `POST` | `/` | Tạo phòng chiếu mới | ❌ |
| `POST` | `/admin/screens/{id}/generate-seats` | Sinh ghế tự động cho phòng chiếu | ❌ |
| `PUT` | `/{id}` | Cập nhật thông tin phòng chiếu | ❌ |
| `DELETE` | `/{id}` | Xóa phòng chiếu | ❌ |

### Request/Response Examples

#### POST /api/screens
```json
// Request Body (ScreenCreateDto)
{
  "cinemaId": "guid",
  "name": "Screen 1",
  "capacity": 120,
  "screenType": "2D"
}
```

#### POST /api/screens/admin/screens/{id}/generate-seats
```json
// Request Body (GenerateSeatsDto)
{
  "rows": 10,
  "seatsPerRow": 12,
  "seatTypeCode": "STANDARD"
}
```

---

## 12. 🪑 Seat Types API

**Base Route:** `/api/seattypes`

Quản lý loại ghế.

| Method | Endpoint | Mô tả | Auth |
|--------|----------|-------|------|
| `GET` | `/` | Lấy danh sách tất cả loại ghế | ❌ |
| `GET` | `/{code}` | Lấy thông tin loại ghế theo mã | ❌ |
| `POST` | `/` | Tạo loại ghế mới | ❌ |
| `PUT` | `/{code}` | Cập nhật thông tin loại ghế | ❌ |
| `DELETE` | `/{code}` | Xóa loại ghế | ❌ |

### Request/Response Examples

#### POST /api/seattypes
```json
// Request Body (SeatTypeCreateDto)
{
  "code": "VIP",
  "name": "Ghế VIP",
  "priceMultiplier": 1.5,
  "color": "#FFD700"
}
```

---

## 13. 💺 Seats API

**Base Route:** `/api/seats`

Quản lý ghế ngồi trong phòng chiếu.

| Method | Endpoint | Mô tả | Auth |
|--------|----------|-------|------|
| `GET` | `/screen/{screenId}` | Lấy danh sách ghế theo phòng chiếu | ❌ |
| `GET` | `/screen/{screenId}/active` | Lấy danh sách ghế đang hoạt động | ❌ |
| `GET` | `/{id}` | Lấy thông tin ghế theo ID | ❌ |
| `GET` | `/{seatId}/available?showtimeId={showtimeId}` | Kiểm tra ghế có còn trống | ❌ |
| `POST` | `/` | Tạo ghế mới | ❌ |
| `POST` | `/generate` | Tự động tạo ghế theo cấu hình (sẽ xóa ghế cũ) | ❌ |
| `PUT` | `/{id}` | Cập nhật thông tin ghế | ❌ |
| `DELETE` | `/{id}` | Xóa ghế | ❌ |

### SeatResponseDto

```json
{
  "id": "guid",
  "screenId": "guid",
  "seatRow": "A",
  "seatNumber": 1,
  "seatCode": "A1",
  "seatTypeCode": "VIP",
  "qrOrderingCode": "A7X2K9",  // Mã 6 ký tự ngẫu nhiên (A-Z, 0-9) dùng cho QR đặt vé
  "isActive": true
}
```

> **📌 Lưu ý:** `qrOrderingCode` là mã 6 ký tự alphanumeric ngẫu nhiên được tạo tự động khi generate seats. Mã này được sử dụng để hiển thị QR code tại quầy bán vé.

### Request/Response Examples

#### GET /api/seats/screen/{screenId}
```json
// Response
{
  "success": true,
  "data": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "screenId": "guid",
      "seatRow": "A",
      "seatNumber": 1,
      "seatCode": "A1",
      "seatTypeCode": "STANDARD",
      "qrOrderingCode": "B3M8N1",
      "isActive": true
    }
  ]
}
```

#### POST /api/seats
```json
// Request Body (SeatCreateDto)
{
  "screenId": "guid",
  "seatRow": "A",
  "seatNumber": 1,
  "seatCode": "A1",
  "seatTypeCode": "STANDARD",
  "isActive": true
}

// Response
{
  "success": true,
  "message": "Tạo ghế thành công",
  "data": {
    "id": "guid",
    "screenId": "guid",
    "seatRow": "A",
    "seatNumber": 1,
    "seatCode": "A1",
    "seatTypeCode": "STANDARD",
    "qrOrderingCode": "Q5P4R7",
    "isActive": true
  }
}
```

#### POST /api/seats/generate
```json
// Request Body (SeatGenerateDto)
{
  "screenId": "guid",
  "rows": 10,
  "seatsPerRow": 12,
  "defaultSeatTypeCode": "STANDARD"
}

// Response - Trả về danh sách ghế đã tạo
{
  "success": true,
  "message": "Tạo 120 ghế thành công",
  "data": [
    {
      "id": "guid",
      "screenId": "guid",
      "seatRow": "A",
      "seatNumber": 1,
      "seatCode": "A1",
      "seatTypeCode": "STANDARD",
      "qrOrderingCode": "K9T3V2",
      "isActive": true
    }
    // ... 119 ghế khác
  ]
}
```

> ⚠️ **Cảnh báo:** API `/generate` sẽ **XÓA TẤT CẢ** ghế cũ của phòng chiếu trước khi tạo ghế mới.

#### PUT /api/seats/{id}
```json
// Request Body (SeatUpdateDto)
{
  "seatTypeCode": "VIP",
  "isActive": false
}
```

#### GET /api/seats/{seatId}/available?showtimeId={showtimeId}
```json
// Response
{
  "success": true,
  "data": true  // true = ghế còn trống, false = đã đặt
}
```

---


## 14. 🕐 Showtimes API

**Base Route:** `/api/showtimes`

Quản lý lịch chiếu phim.

| Method | Endpoint | Mô tả | Auth |
|--------|----------|-------|------|
| `GET` | `/` | Lấy tất cả lịch chiếu | ❌ |
| `GET` | `/movie/{movieId}` | Lấy danh sách lịch chiếu theo phim | ❌ |
| `GET` | `/date/{date}` | Lấy danh sách lịch chiếu theo ngày | ❌ |
| `GET` | `/movie/{movieId}/date/{date}` | Lấy lịch chiếu theo phim và ngày | ❌ |
| `GET` | `/{id}` | Lấy thông tin lịch chiếu theo ID | ❌ |
| `GET` | `/{id}/seats` | Lấy sơ đồ ghế và trạng thái ghế | ❌ |
| `POST` | `/` | Tạo lịch chiếu mới | ❌ |
| `PUT` | `/{id}` | Cập nhật thông tin lịch chiếu | ❌ |
| `PUT` | `/admin/showtimes/{id}/price` | Cập nhật giá vé cơ bản | ❌ |
| `DELETE` | `/{id}` | Xóa lịch chiếu | ❌ |

### Request/Response Examples

#### POST /api/showtimes
```json
// Request Body (ShowtimeCreateDto)
{
  "movieId": "guid",
  "screenId": "guid",
  "startTime": "2025-01-01T19:00:00",
  "basePrice": 100000
}
```

#### GET /api/showtimes/{id}/seats
```json
// Response (ShowtimeSeatsResponseDto)
{
  "success": true,
  "data": {
    "showtimeId": "guid",
    "movieTitle": "string",
    "screenName": "string",
    "startTime": "datetime",
    "basePrice": 100000,
    "rows": [
      {
        "rowLabel": "A",
        "seats": [
          {
            "id": "guid",
            "seatCode": "A1",
            "seatType": "STANDARD",
            "price": 100000,
            "status": "Available|Booked|Selected"
          }
        ]
      }
    ]
  }
}
```

#### PUT /api/showtimes/admin/showtimes/{id}/price
```json
// Request Body (ShowtimeUpdatePriceDto)
{
  "basePrice": 120000
}
```

---

## 15. 👨‍💼 Staff API

**Base Route:** `/api/staff`

API dành cho nhân viên quầy vé.

| Method | Endpoint | Mô tả | Auth |
|--------|----------|-------|------|
| `GET` | `/orders/search?phone={phone}` | Tìm kiếm đơn hàng theo SĐT khách hàng | ❌* |
| `GET` | `/orders/{id}` | Lấy chi tiết đơn hàng (cho nhân viên) | ❌* |
| `POST` | `/orders/{id}/print` | In lại vé cho khách hàng | ❌* |
| `POST` | `/tickets/verify` | Quét QR Code vé để verify và check-in | ❌* |
| `POST` | `/orders/pos-create` | Tạo đơn hàng POS và thanh toán tiền mặt | ❌* |

> **Note:** API này nên được bảo vệ bằng `[Authorize(Roles = "Staff,Admin")]` khi enable authentication.

### Request/Response Examples

#### GET /api/staff/orders/search?phone={phone}
```json
// Response (List<OrderSearchResultDto>)
{
  "success": true,
  "message": "Tìm thấy 2 đơn hàng",
  "data": [
    {
      "orderId": "guid",
      "movieTitle": "string",
      "showtime": "datetime",
      "seats": ["A1", "A2"],
      "totalAmount": 200000,
      "status": "Confirmed"
    }
  ]
}
```

#### POST /api/staff/orders/{id}/print
```json
// Request Body (PrintTicketDto)
{
  "printReason": "Khách mất điện thoại",
  "staffNote": "Đã xác minh CMND"
}
```

#### POST /api/staff/tickets/verify
```json
// Request Body (VerifyTicketDto)
{
  "qrData": "string"
}

// Response (TicketVerificationResultDto)
{
  "success": true,
  "data": {
    "isValid": true,
    "status": "Valid",
    "message": "Vé hợp lệ",
    "ticketDetail": {
      "ticketCode": "string",
      "movieTitle": "string",
      "showtime": "datetime",
      "seatCode": "A1"
    }
  }
}
```

#### POST /api/staff/orders/pos-create
```json
// Request Body (PosOrderCreateDto)
{
  "customerName": "Nguyễn Văn A",
  "customerPhone": "0901234567",
  "showtimeId": "guid",
  "seatIds": ["guid", "guid"],
  "products": [
    { "productId": "guid", "quantity": 2 }
  ],
  "cashReceived": 500000
}

// Response (PosOrderResponseDto)
{
  "success": true,
  "message": "Đơn hàng đã được tạo thành công. Tiền thừa: 150,000 VND",
  "data": {
    "orderDetail": { ... },
    "paymentInfo": {
      "totalAmount": 350000,
      "cashReceived": 500000,
      "changeAmount": 150000
    },
    "printData": { ... }
  }
}
```

---

## 16. 📤 Upload API

**Base Route:** `/api/upload`

Quản lý upload hình ảnh lên Cloudinary.

| Method | Endpoint | Mô tả | Auth |
|--------|----------|-------|------|
| `POST` | `/image?folder={folder}` | Upload một hình ảnh | ✅ Admin,Staff |
| `POST` | `/images?folder={folder}` | Upload nhiều hình ảnh | ✅ Admin,Staff |
| `POST` | `/entity/{entityType}?entityId={entityId}` | Upload hình ảnh cho entity | ✅ Admin,Staff |
| `GET` | `/transform/{publicId}?width={w}&height={h}&crop={crop}` | Lấy URL hình ảnh đã transform | ❌ |
| `DELETE` | `/image/{publicId}` | Xóa hình ảnh | ✅ Admin,Staff |

### Entity Types
- `movie`
- `actor`
- `cinema`
- `product`
- `banner`
- `user`

### Crop Modes
- `fill`, `fit`, `scale`, `crop`, `thumb`, `pad`, `limit`, `mfit`, `lfill`

### Request/Response Examples

#### POST /api/upload/image (🔒 Role: Admin, Staff)
```http
Content-Type: multipart/form-data

file: [binary image data]
folder: cinepass/movies (optional)
```

```json
// Response (UploadImageResponseDto)
{
  "publicId": "cinepass/movies/abc123",
  "url": "https://res.cloudinary.com/...",
  "secureUrl": "https://res.cloudinary.com/...",
  "width": 800,
  "height": 600,
  "format": "jpg"
}
```

#### POST /api/upload/entity/movie (🔒 Role: Admin, Staff)
```http
Content-Type: multipart/form-data

file: [binary image data]
entityId: optional-movie-id
```

---

## 17. 👥 Users API

**Base Route:** `/api/users`

Quản lý người dùng.

| Method | Endpoint | Mô tả | Auth |
|--------|----------|-------|------|
| `GET` | `/` | Lấy danh sách tất cả người dùng | ❌ |
| `GET` | `/{id}` | Lấy thông tin người dùng theo ID | ❌ |
| `GET` | `/email/{email}` | Lấy thông tin người dùng theo email | ❌ |
| `GET` | `/role/{role}` | Lấy danh sách người dùng theo vai trò | ❌ |
| `POST` | `/` | Tạo người dùng mới | ❌ |
| `PUT` | `/{id}` | Cập nhật thông tin người dùng | ❌ |
| `DELETE` | `/{id}` | Xóa người dùng | ❌ |

### User Role Enum
- `Customer` (0)
- `Staff` (1)
- `Admin` (2)

### Request/Response Examples

#### POST /api/users
```json
// Request Body (UserCreateDto)
{
  "email": "user@example.com",
  "password": "SecurePassword123!",
  "fullName": "Nguyễn Văn A",
  "phoneNumber": "0901234567",
  "role": 0
}
```

#### PUT /api/users/{id}
```json
// Request Body (UserUpdateDto)
{
  "fullName": "Nguyễn Văn B",
  "phoneNumber": "0909876543",
  "avatarUrl": "https://..."
}
```

---

## 📋 Response Format

Tất cả API đều trả về response theo format thống nhất:

```json
{
  "success": true,
  "message": "Thông báo",
  "data": { /* Dữ liệu trả về */ },
  "errors": [ /* Danh sách lỗi (nếu có) */ ]
}
```

### HTTP Status Codes

| Code | Mô tả |
|------|-------|
| `200` | OK - Thành công |
| `201` | Created - Tạo mới thành công |
| `204` | No Content - Xóa thành công |
| `400` | Bad Request - Dữ liệu không hợp lệ |
| `401` | Unauthorized - Chưa đăng nhập |
| `403` | Forbidden - Không có quyền |
| `404` | Not Found - Không tìm thấy |
| `499` | Client Closed Request - Request bị hủy |
| `500` | Internal Server Error - Lỗi server |

---

## 🔐 Authentication

API sử dụng **JWT Bearer Token** cho xác thực.

### Header
```http
Authorization: Bearer <access_token>
```

### Token Structure
- **Access Token**: Có thời hạn ngắn, dùng để xác thực request
- **Refresh Token**: Có thời hạn dài, dùng để lấy access token mới

### Roles
- `Customer` - Khách hàng
- `Staff` - Nhân viên
- `Admin` - Quản trị viên

---

## 📝 Notes

1. **GUID Format**: Tất cả ID đều sử dụng định dạng GUID (e.g., `"12345678-1234-1234-1234-123456789012"`)

2. **DateTime Format**: Sử dụng ISO 8601 format (e.g., `"2025-01-01T19:00:00"`)

3. **Currency**: Tất cả giá tiền đều tính bằng VND (đồng Việt Nam)

4. **Pagination**: Một số API có thể hỗ trợ pagination trong tương lai

5. **Rate Limiting**: Có thể áp dụng rate limiting cho một số API

---

> **Liên hệ hỗ trợ:** Vui lòng liên hệ đội ngũ Backend nếu có câu hỏi hoặc cần hỗ trợ thêm.
