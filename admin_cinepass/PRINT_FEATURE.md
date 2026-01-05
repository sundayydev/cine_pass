# Tính Năng In Vé - CinePass Admin

## Tổng Quan

Tính năng in vé cho phép nhân viên quản lý in vé điện tử cho khách hàng trực tiếp từ trang chi tiết đơn hàng.

## Các Tính Năng Chính

### 1. **Nút In Vé**
- Xuất hiện trong header của trang chi tiết đơn hàng
- Chỉ hiển thị khi đơn hàng có trạng thái:
  - ✅ **Đã thanh toán** (Paid)
  - ✅ **Đã xác nhận** (Confirmed)
- Icon: 🖨️ (Printer)
- Màu: Primary với border

### 2. **Thông Tin Được In**

#### Header (Tiêu đề)
- Tên rạp chiếu phim
- Tiêu đề: "VÉ XEM PHIM"
- Mã đơn hàng
- Thời gian in

#### Chi Tiết Vé
Mỗi vé bao gồm:
- **Thông tin phim:**
  - Tên phim
  - Poster (nền mờ)
  
- **Thông tin địa điểm:**
  - Tên rạp chiếu
  - Tên phòng chiếu
  
- **Thông tin ghế:**
  - Mã ghế (vd: A5, B3)
  - Loại ghế (Thường/VIP/Đôi)
  
- **Thông tin suất chiếu:**
  - Ngày giờ chiếu
  - Giá vé
  
- **Thông tin vé điện tử:**
  - Mã vé (Ticket Code)
  - Mã QR để check-in
  - Trạng thái sử dụng

#### Đồ Ăn & Nước Uống
- Danh sách sản phẩm đã đặt
- Hình ảnh sản phẩm
- Số lượng và giá

#### Footer
- Hướng dẫn check-in
- Thông tin liên hệ hotline
- Lời cảm ơn
- Thông tin hệ thống

## Cách Sử Dụng

### Bước 1: Truy cập Chi Tiết Đơn Hàng
1. Vào trang **Quản lý đặt vé** (`/bookings`)
2. Click vào đơn hàng cần in vé

### Bước 2: Kiểm Tra Trạng Thái
- Đảm bảo đơn hàng có trạng thái **Đã thanh toán** hoặc **Đã xác nhận**
- Nếu chưa xác nhận, click nút "Xác nhận đơn hàng" trước

### Bước 3: In Vé
1. Click nút **"In vé"** 🖨️ ở góc trên bên phải
2. Hộp thoại in của trình duyệt sẽ xuất hiện
3. Chọn máy in và các tùy chọn in:
   - **Khổ giấy:** A4 (khuyến nghị)
   - **Hướng:** Portrait (Dọc)
   - **Lề:** Normal (1cm)
   - **Màu sắc:** Bật "Background graphics" để in màu
4. Click **Print** để in

## Tối Ưu Hóa Khi In

### Tự Động Ẩn
Khi in, hệ thống tự động ẩn:
- ❌ Navigation bar
- ❌ Buttons và controls
- ❌ Sidebar (thông tin khách hàng, thanh toán)
- ❌ Alert dialogs

### Tự Động Hiển Thị
- ✅ Header chuyên dụng cho in
- ✅ Chi tiết vé với QR code
- ✅ Danh sách sản phẩm
- ✅ Footer hướng dẫn

### Tối Ưu Layout
- **Màu sắc:** Được giữ nguyên (print-color-adjust: exact)
- **QR Code:** Luôn hiển thị rõ ràng
- **Page Break:** Tránh cắt vé giữa chừng
- **Margins:** 1cm tất cả các cạnh

## Technical Details

### CSS Print Media Queries
```css
@media print {
  /* Ẩn tất cả trừ vùng in */
  body * { visibility: hidden; }
  
  /* Hiển thị vùng in */
  .print-area, .print-area * { visibility: visible; }
  
  /* Ẩn các thành phần không cần */
  .no-print { display: none !important; }
  
  /* Tối ưu vé */
  .ticket-print {
    page-break-inside: avoid;
    break-inside: avoid;
  }
  
  /* Giữ màu sắc */
  * {
    -webkit-print-color-adjust: exact;
    print-color-adjust: exact;
  }
}
```

### Component Structure
```tsx
<div className="print-area">
  {/* Print Header */}
  <div className="hidden print:block">
    ... Cinema name, Order ID, Print time ...
  </div>
  
  {/* Tickets */}
  {tickets.map(ticket => (
    <div className="ticket-print">
      <TicketCard ticket={ticket} />
    </div>
  ))}
  
  {/* Products */}
  ... Product details ...
  
  {/* Print Footer */}
  <div className="hidden print:block">
    ... Instructions, Thank you message ...
  </div>
</div>
```

## Xử Lý Lỗi

### Không Thấy Nút In
- ✔️ Kiểm tra trạng thái đơn hàng
- ✔️ Chỉ hiển thị với trạng thái Paid/Confirmed

### In Ra Trống
- ✔️ Bật "Background graphics" trong Print Settings
- ✔️ Kiểm tra trình duyệt hỗ trợ CSS print

### QR Code Không Rõ
- ✔️ Chọn Print Quality: High
- ✔️ Đảm bảo máy in hỗ trợ độ phân giải đủ

### Màu Sắc Không In
- ✔️ Bật "Print background colors"
- ✔️ Sử dụng máy in màu

## Browser Compatibility

### Được Hỗ Trợ Tốt
- ✅ Chrome/Edge (v90+)
- ✅ Firefox (v88+)
- ✅ Safari (v14+)

### Tính Năng CSS
- ✅ `@media print`
- ✅ `print-color-adjust: exact`
- ✅ `page-break-inside: avoid`
- ✅ `visibility` control

## Tips & Best Practices

### 1. **Pre-print Checklist**
- [ ] Xác nhận đơn hàng đã thanh toán
- [ ] Kiểm tra thông tin vé chính xác
- [ ] Đảm bảo QR code hiển thị
- [ ] Test in với 1 bản trước

### 2. **Printer Settings**
- Khổ giấy: **A4**
- Màu: **Color** (nếu có màu)
- Chất lượng: **High/Best**
- Tỷ lệ: **100%** (không scale)

### 3. **Sau Khi In**
- Kiểm tra QR code có quét được
- Xác nhận tất cả thông tin rõ ràng
- Giao vé cho khách hàng

## Future Enhancements

### Planned Features
- 📄 Hỗ trợ in hàng loạt (multiple orders)
- 💾 Lưu PDF thay vì in trực tiếp
- 📧 Gửi vé qua email
- 🎨 Customizable ticket templates
- 🖨️ Thermal printer support (80mm)

### Under Consideration
- Barcode support (ngoài QR)
- Multiple language support
- Custom branding per cinema

## Support

Nếu gặp vấn đề với tính năng in vé:
1. Kiểm tra console để xem lỗi
2. Thử trình duyệt khác
3. Liên hệ team dev với:
   - Screenshot lỗi
   - Browser version
   - Printer model

---

**Phiên bản:** 1.0  
**Cập nhật:** 2025-12-19  
**Tác giả:** CinePass Development Team
