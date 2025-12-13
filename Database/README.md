Phần A: Hạ tầng (Cinema, Screen, Seat)

- Vấn đề cũ: Bạn dùng seat_map (JSON) trong bảng screens nhưng lại có bảng seats riêng lẻ, và bảng seat_bookings để giữ chỗ.

- Cải tiến:

    seats: Mình giữ bảng này làm dữ liệu vật lý (Physical Data). Mỗi cái ghế là một dòng record.

    UNIQUE(screen_id, seat_code): Dòng này cực quan trọng. Nó ngăn chặn việc database có 2 ghế "A1" trong cùng một phòng.

    seat_types: Tách riêng ra để quản lý giá linh hoạt. Ví dụ ghế VIP nhân hệ số 1.2, ghế đôi nhân 2.0. Không nên hardcode giá vào bảng ghế.

Phần B: Phim & Lịch chiếu (Movies & Showtimes)

- Vấn đề cũ: Thiếu end_time và validation.

- Cải tiến:

    showtimes: Mình thêm ràng buộc CHECK (end_time > start_time).

    Index: Thêm Index cho start_time và movie_id. Tại sao? Vì câu query phổ biến nhất của app đặt vé là "Lấy danh sách phim chiếu ngày hôm nay". Nếu không có index, khi dữ liệu lớn app sẽ rất chậm.

Phần C: Quy trình Đặt vé (Booking Flow) - Phần quan trọng nhất

Đây là thay đổi lớn nhất so với bản gốc của bạn. Thay vì tách lẻ tẻ seat_bookings, orders, tickets, combo_orders, mình gom lại theo tư duy E-commerce (Thương mại điện tử):

- Bảng orders (Giỏ hàng tổng):

    Đại diện cho một "phiên giao dịch".

    Có expire_at: Khi user chọn ghế, bạn tạo 1 Order với status 'pending' và set thời gian hết hạn là 10 phút. Nếu không thanh toán kịp, job background sẽ xóa order này => ghế nhả ra cho người khác.

- Bảng order_tickets (Chi tiết vé):

    Lưu dòng: Mua vé phim A, ghế B1.

    CONSTRAINT UNIQUE(showtime_id, seat_id): Đây là khóa chống trùng vé.

    Trong database cũ của bạn, bạn dùng bảng seat_bookings để check status. Cách đó dễ bị lỗi "Race Condition" (2 người cùng bấm mua 1 lúc). Với ràng buộc Unique này ngay trong bảng order_tickets, nếu 2 người cùng bấm mua ghế A1, Database sẽ chỉ cho phép 1 người insert thành công, người kia sẽ bị lỗi ngay lập tức => An toàn tuyệt đối.

- Bảng order_products (Chi tiết bắp nước):

    Thay vì tách bảng combo_orders riêng, hãy coi bắp nước là một "Item" trong giỏ hàng. Điều này giúp tính tổng tiền (total_amount) dễ dàng hơn và xuất hóa đơn gộp (1 hóa đơn cho cả vé và bắp).

Phần D: Vé điện tử (E-Ticket)

Mình tách e_tickets ra khỏi orders.

Lý do: orders là dữ liệu giao dịch (tiền nong). e_tickets là dữ liệu vận hành (check-in).

Chỉ khi thanh toán thành công (orders.status = 'confirmed') thì backend mới trigger tạo record trong e_tickets.

Trường ticket_code nên là chuỗi ngắn (VD: X8K9L2) để nhân viên có thể nhập tay nếu máy quét QR hỏng.