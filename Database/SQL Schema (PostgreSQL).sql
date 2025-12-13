-- Kích hoạt extension để sinh UUID tự động
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- 1. QUẢN LÝ NGƯỜI DÙNG
-- Tách riêng bảng users để quản lý xác thực
CREATE TABLE users (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    email text UNIQUE NOT NULL,
    phone text,
    password_hash text NOT NULL,
    full_name text,
    role text CHECK (role IN ('customer', 'staff', 'admin')) DEFAULT 'customer',
    created_at timestamptz DEFAULT now(),
    updated_at timestamptz DEFAULT now()
);

-- 2. HẠ TẦNG RẠP CHIẾU (Cinema -> Screen -> Seat)
CREATE TABLE cinemas (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    name text NOT NULL,
    address text,
    city text, -- Thêm city để lọc theo khu vực
    is_active boolean DEFAULT true
);

CREATE TABLE screens (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    cinema_id uuid REFERENCES cinemas(id) ON DELETE CASCADE,
    name text NOT NULL, -- Ví dụ: "Rạp 01", "IMAX Room"
    total_seats int DEFAULT 0,
    seat_map_layout jsonb -- Lưu cấu trúc hiển thị (Grid) cho Frontend vẽ ghế
);

-- Loại ghế (VIP, Thường, Đôi) - Tách ra để dễ quản lý giá
CREATE TABLE seat_types (
    code text PRIMARY KEY, -- 'STD', 'VIP', 'SWEETBOX'
    name text,
    surcharge_rate numeric(5,2) DEFAULT 1.0 -- Hệ số giá (VD: VIP = 1.2)
);

CREATE TABLE seats (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    screen_id uuid REFERENCES screens(id) ON DELETE CASCADE,
    seat_row text NOT NULL, -- "A", "B"
    seat_number int NOT NULL, -- 1, 2
    seat_code text NOT NULL, -- "A1", "B2" (Dùng để hiển thị vé)
    seat_type_code text REFERENCES seat_types(code),
    is_active boolean DEFAULT true,
    UNIQUE(screen_id, seat_code) -- Đảm bảo 1 phòng không có 2 ghế A1
);

-- 3. PHIM VÀ LỊCH CHIẾU
CREATE TABLE movies (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    title text NOT NULL,
    slug text UNIQUE, -- Dùng cho URL SEO friendly
    duration_minutes int NOT NULL,
    description text,
    poster_url text,
    trailer_url text,
    release_date date,
    status text CHECK (status IN ('coming_soon', 'showing', 'ended')),
    created_at timestamptz DEFAULT now()
);

CREATE TABLE showtimes (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    movie_id uuid REFERENCES movies(id),
    screen_id uuid REFERENCES screens(id),
    start_time timestamptz NOT NULL,
    end_time timestamptz NOT NULL, -- Nên tính toán: start_time + movie.duration + cleaning_time
    base_price numeric(10,2) NOT NULL, -- Giá gốc cho xuất chiếu này
    is_active boolean DEFAULT true,
    CHECK (end_time > start_time)
);

-- Index để tìm lịch chiếu nhanh
CREATE INDEX idx_showtimes_movie ON showtimes(movie_id);
CREATE INDEX idx_showtimes_date ON showtimes(start_time);

-- 4. SẢN PHẨM BẮP NƯỚC (CONCESSIONS)
CREATE TABLE products (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    name text NOT NULL, -- "Combo Bắp Nước Lớn"
    description text,
    price numeric(10,2) NOT NULL,
    image_url text,
    category text CHECK (category IN ('food', 'drink', 'combo')),
    is_active boolean DEFAULT true
);

-- 5. ĐƠN HÀNG (CORE)
-- Gộp ticket và combo vào chung một Order cha để quản lý thanh toán dễ hơn
CREATE TABLE orders (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id uuid REFERENCES users(id),
    total_amount numeric(12,2) DEFAULT 0,
    status text CHECK (status IN ('pending', 'confirmed', 'cancelled', 'refunded')),
    payment_method text, -- 'momo', 'zalopay', 'card'
    created_at timestamptz DEFAULT now(),
    expire_at timestamptz -- Dùng để giữ ghế trong 10-15p, quá giờ cronjob sẽ hủy
);

-- Chi tiết vé trong đơn hàng
CREATE TABLE order_tickets (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id uuid REFERENCES orders(id) ON DELETE CASCADE,
    showtime_id uuid REFERENCES showtimes(id),
    seat_id uuid REFERENCES seats(id),
    price numeric(10,2) NOT NULL, -- Giá tại thời điểm mua (Base price * Seat Type Surcharge)
    -- Ràng buộc quan trọng: 1 ghế trong 1 suất chiếu chỉ được bán 1 lần
    UNIQUE(showtime_id, seat_id) 
);

-- Chi tiết bắp nước trong đơn hàng
CREATE TABLE order_products (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id uuid REFERENCES orders(id) ON DELETE CASCADE,
    product_id uuid REFERENCES products(id),
    quantity int DEFAULT 1,
    unit_price numeric(10,2) NOT NULL
);

-- 6. VÉ ĐIỆN TỬ (Sau khi thanh toán thành công mới sinh ra bảng này)
CREATE TABLE e_tickets (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    order_ticket_id uuid REFERENCES order_tickets(id), -- Link ngược về chi tiết đơn hàng
    ticket_code text UNIQUE, -- Mã hiển thị cho user (VD: 8 ký tự random)
    qr_data text, -- Dữ liệu để tạo QR Code
    is_used boolean DEFAULT false, -- Đã check-in chưa
    used_at timestamptz
);

-- 7. THANH TOÁN LOG
CREATE TABLE payment_transactions (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id uuid REFERENCES orders(id),
    provider_trans_id text, -- Mã giao dịch từ MoMo/ZaloPay
    amount numeric(12,2),
    status text, -- 'success', 'fail'
    response_json jsonb, -- Lưu toàn bộ log trả về từ cổng thanh toán để debug
    created_at timestamptz DEFAULT now()
);