1. UsersController (/api/users)
- GET / - Lấy danh sách tất cả users
- GET /{id} - Lấy user theo ID
- GET /email/{email} - Lấy user theo email
- GET /role/{role} - Lấy users theo role
- POST / - Tạo user mới
- PUT /{id} - Cập nhật user
- DELETE /{id} - Xóa user
- POST /login - Đăng nhập
2. MoviesController (/api/movies)
- GET / - Lấy danh sách phim
- GET /{id} - Lấy phim theo ID
- GET /slug/{slug} - Lấy phim theo slug
- GET /now-showing - Phim đang chiếu
- GET /coming-soon - Phim sắp chiếu
- GET /search?searchTerm=... - Tìm kiếm phim
- POST / - Tạo phim mới
- PUT /{id} - Cập nhật phim
- DELETE /{id} - Xóa phim
3. CinemasController (/api/cinemas)
- GET / - Lấy danh sách rạp
- GET /active - Rạp đang hoạt động
- GET /city/{city} - Rạp theo thành phố
- GET /{id} - Lấy rạp theo ID
- POST / - Tạo rạp mới
- PUT /{id} - Cập nhật rạp
- DELETE /{id} - Xóa rạp
4. ScreensController (/api/screens)
- GET /cinema/{cinemaId} - Màn hình theo rạp
- GET /{id} - Lấy màn hình theo ID
- POST / - Tạo màn hình mới
- PUT /{id} - Cập nhật màn hình
- DELETE /{id} - Xóa màn hình
5. SeatTypesController (/api/seattypes)
- GET / - Lấy danh sách loại ghế
- GET /{code} - Lấy loại ghế theo mã
- POST / - Tạo loại ghế mới
- PUT /{code} - Cập nhật loại ghế
- DELETE /{code} - Xóa loại ghế
6. SeatsController (/api/seats)
- GET /screen/{screenId} - Ghế theo màn hình
- GET /screen/{screenId}/active - Ghế đang hoạt động
- GET /{id} - Lấy ghế theo ID
- GET /{seatId}/available?showtimeId=... - Kiểm tra - ghế còn trống
- POST / - Tạo ghế mới
- PUT /{id} - Cập nhật ghế
- DELETE /{id} - Xóa ghế
7. ShowtimesController (/api/showtimes)
- GET /movie/{movieId} - Lịch chiếu theo phim
- GET /date/{date} - Lịch chiếu theo ngày
- GET /movie/{movieId}/date/{date} - Lịch chiếu theo phim và ngày
- GET /{id} - Lấy lịch chiếu theo ID
- POST / - Tạo lịch chiếu mới
- PUT /{id} - Cập nhật lịch chiếu
- DELETE /{id} - Xóa lịch chiếu
8. ProductsController (/api/products)
- GET / - Lấy danh sách sản phẩm
- GET /active - Sản phẩm đang hoạt động
- GET /category/{category} - Sản phẩm theo danh mục
- GET /search?searchTerm=... - Tìm kiếm sản phẩm
- GET /{id} - Lấy sản phẩm theo ID
- POST / - Tạo sản phẩm mới
- PUT /{id} - Cập nhật sản phẩm
- DELETE /{id} - Xóa sản phẩm
9. OrdersController (/api/orders)
- GET /{id} - Lấy đơn hàng theo ID
- GET /{id}/detail - Lấy chi tiết đơn hàng
- GET /user/{userId} - Đơn hàng theo user
- GET /status/{status} - Đơn hàng theo trạng thái
- GET /expired - Đơn hàng hết hạn
- POST / - Tạo đơn hàng mới
- PUT /{id} - Cập nhật đơn hàng
- POST /{id}/confirm - Xác nhận đơn hàng
- POST /{id}/cancel - Hủy đơn hàng
10. ETicketsController (/api/etickets)
- GET /{id} - Lấy vé điện tử theo ID
- GET /code/{ticketCode} - Lấy vé theo mã
- GET /code/{ticketCode}/detail - Chi tiết vé theo mã
- GET /order-ticket/{orderTicketId} - Vé theo order ticket
- POST /generate/{orderTicketId} - Tạo vé điện tử
- GET /validate/{ticketCode} - Xác thực vé
- POST /use/{ticketCode} - Sử dụng vé (check-in)
11. PaymentTransactionsController (/api/paymenttransactions)
- GET /{id} - Lấy giao dịch theo ID
- GET /order/{orderId} - Giao dịch theo đơn hàng
- GET /provider/{providerTransId} - Giao dịch theo mã provider
- GET /successful - Giao dịch thành công
- GET /failed - Giao dịch thất bại
- POST / - Tạo giao dịch mới
- PUT /{id}/status - Cập nhật trạng thái