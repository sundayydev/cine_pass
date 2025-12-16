-- ============================================
-- CinePass Database Sample Data
-- ============================================

-- ============================================
-- 1. USERS
-- ============================================
-- Role: 0=Customer, 1=Staff, 2=Admin
INSERT INTO users (email, phone, password_hash, full_name, role) VALUES 
('admin@cinepass.com', '0909090909', '$2a$11$hashed_password_admin', 'Nguyễn Văn Admin', 2),
('staff1@cinepass.com', '0909090901', '$2a$11$hashed_password_staff1', 'Trần Thị Staff 1', 1),
('staff2@cinepass.com', '0909090902', '$2a$11$hashed_password_staff2', 'Lê Văn Staff 2', 1),
('customer1@gmail.com', '0909090911', '$2a$11$hashed_password_cust1', 'Phạm Minh Khách 1', 0),
('customer2@gmail.com', '0909090912', '$2a$11$hashed_password_cust2', 'Hoàng Thị Khách 2', 0),
('customer3@gmail.com', '0909090913', '$2a$11$hashed_password_cust3', 'Đặng Văn Khách 3', 0),
('customer4@gmail.com', '0909090914', '$2a$11$hashed_password_cust4', 'Võ Thị Khách 4', 0),
('customer5@gmail.com', '0909090915', '$2a$11$hashed_password_cust5', 'Bùi Minh Khách 5', 0);

-- ============================================
-- 2. MOVIES
-- ============================================
INSERT INTO movies (title, slug, description, duration, release_date, poster_url, trailer_url, is_active) VALUES 
('Avatar: The Way of Water', 'avatar-the-way-of-water', 'Phần tiếp theo của bộ phim Avatar huyền thoại, khám phá vùng biển Pandora đầy kỳ thú', 192, '2024-12-16', 'https://image.tmdb.org/t/p/w500/avatar2.jpg', 'https://youtube.com/watch?v=avatar2', true),
('Avengers: Endgame', 'avengers-endgame', 'Trận chiến cuối cùng của các siêu anh hùng Avengers chống lại Thanos', 181, '2024-12-15', 'https://image.tmdb.org/t/p/w500/avengers.jpg', 'https://youtube.com/watch?v=avengers', true),
('Spider-Man: No Way Home', 'spider-man-no-way-home', 'Peter Parker đối mặt với hậu quả khi danh tính bị lộ', 148, '2024-12-14', 'https://image.tmdb.org/t/p/w500/spiderman.jpg', 'https://youtube.com/watch?v=spiderman', true),
('The Batman', 'the-batman', 'Người Dơi truy tìm kẻ giết người hàng loạt ở Gotham City', 176, '2024-12-18', 'https://image.tmdb.org/t/p/w500/batman.jpg', 'https://youtube.com/watch?v=batman', true),
('Top Gun: Maverick', 'top-gun-maverick', 'Phi công Maverick trở lại huấn luyện thế hệ phi công mới', 131, '2024-12-20', 'https://image.tmdb.org/t/p/w500/topgun.jpg', 'https://youtube.com/watch?v=topgun', true),
('Interstellar', 'interstellar', 'Hành trình vũ trụ để tìm kiếm ngôi nhà mới cho nhân loại', 169, '2024-12-17', 'https://image.tmdb.org/t/p/w500/interstellar.jpg', 'https://youtube.com/watch?v=interstellar', true),
('Inception', 'inception', 'Thế giới của những giấc mơ và thực tại đan xen', 148, '2024-12-19', 'https://image.tmdb.org/t/p/w500/inception.jpg', 'https://youtube.com/watch?v=inception', true),
('Parasite', 'parasite', 'Câu chuyện về hai gia đình thuộc hai tầng lớp xã hội khác nhau', 132, '2024-12-21', 'https://image.tmdb.org/t/p/w500/parasite.jpg', 'https://youtube.com/watch?v=parasite', true),
('Joker', 'joker', 'Câu chuyện về sự biến đổi của Arthur Fleck thành Joker', 122, '2024-12-22', 'https://image.tmdb.org/t/p/w500/joker.jpg', 'https://youtube.com/watch?v=joker', true),
('The Dark Knight', 'the-dark-knight', 'Batman đối đầu với Joker trong cuộc chiến giữa trật tự và hỗn loạn', 152, '2024-12-23', 'https://image.tmdb.org/t/p/w500/darkknight.jpg', 'https://youtube.com/watch?v=darkknight', true);

-- ============================================
-- 3. ACTORS
-- ============================================
INSERT INTO actors (name, slug, description, image_url) VALUES 
('Leonardo DiCaprio', 'leonardo-dicaprio', 'Diễn viên người Mỹ, từng đoạt giải Oscar', 'https://image.tmdb.org/t/p/w500/leonardo.jpg'),
('Scarlett Johansson', 'scarlett-johansson', 'Nữ diễn viên nổi tiếng với vai Black Widow', 'https://image.tmdb.org/t/p/w500/scarlett.jpg'),
('Robert Downey Jr.', 'robert-downey-jr', 'Diễn viên nổi tiếng với vai Iron Man', 'https://image.tmdb.org/t/p/w500/rdj.jpg'),
('Tom Holland', 'tom-holland', 'Diễn viên trẻ đóng vai Spider-Man', 'https://image.tmdb.org/t/p/w500/tomholland.jpg'),
('Christian Bale', 'christian-bale', 'Diễn viên người Anh, từng đóng vai Batman', 'https://image.tmdb.org/t/p/w500/bale.jpg'),
('Anne Hathaway', 'anne-hathaway', 'Nữ diễn viên đoạt giải Oscar', 'https://image.tmdb.org/t/p/w500/anne.jpg'),
('Sam Worthington', 'sam-worthington', 'Diễn viên chính trong Avatar', 'https://image.tmdb.org/t/p/w500/sam.jpg'),
('Zoe Saldana', 'zoe-saldana', 'Nữ diễn viên trong Avatar và Guardians of the Galaxy', 'https://image.tmdb.org/t/p/w500/zoe.jpg'),
('Joaquin Phoenix', 'joaquin-phoenix', 'Diễn viên đã đoạt Oscar cho vai Joker', 'https://image.tmdb.org/t/p/w500/joaquin.jpg'),
('Heath Ledger', 'heath-ledger', 'Diễn viên huyền thoại với vai Joker', 'https://image.tmdb.org/t/p/w500/heath.jpg');

-- ============================================
-- 4. MOVIE_ACTORS (Junction Table)
-- ============================================
-- Avatar with Sam Worthington, Zoe Saldana
INSERT INTO movie_actors (movie_id, actor_id) 
SELECT m.id, a.id FROM movies m, actors a 
WHERE m.slug = 'avatar-the-way-of-water' AND a.slug = 'sam-worthington';

INSERT INTO movie_actors (movie_id, actor_id) 
SELECT m.id, a.id FROM movies m, actors a 
WHERE m.slug = 'avatar-the-way-of-water' AND a.slug = 'zoe-saldana';

-- Avengers with Robert Downey Jr., Scarlett Johansson
INSERT INTO movie_actors (movie_id, actor_id) 
SELECT m.id, a.id FROM movies m, actors a 
WHERE m.slug = 'avengers-endgame' AND a.slug = 'robert-downey-jr';

INSERT INTO movie_actors (movie_id, actor_id) 
SELECT m.id, a.id FROM movies m, actors a 
WHERE m.slug = 'avengers-endgame' AND a.slug = 'scarlett-johansson';

-- Spider-Man with Tom Holland, Zoe Saldana
INSERT INTO movie_actors (movie_id, actor_id) 
SELECT m.id, a.id FROM movies m, actors a 
WHERE m.slug = 'spider-man-no-way-home' AND a.slug = 'tom-holland';

-- The Batman with Christian Bale
INSERT INTO movie_actors (movie_id, actor_id) 
SELECT m.id, a.id FROM movies m, actors a 
WHERE m.slug = 'the-batman' AND a.slug = 'christian-bale';

-- Inception with Leonardo DiCaprio
INSERT INTO movie_actors (movie_id, actor_id) 
SELECT m.id, a.id FROM movies m, actors a 
WHERE m.slug = 'inception' AND a.slug = 'leonardo-dicaprio';

-- Joker with Joaquin Phoenix
INSERT INTO movie_actors (movie_id, actor_id) 
SELECT m.id, a.id FROM movies m, actors a 
WHERE m.slug = 'joker' AND a.slug = 'joaquin-phoenix';

-- The Dark Knight with Christian Bale, Heath Ledger
INSERT INTO movie_actors (movie_id, actor_id) 
SELECT m.id, a.id FROM movies m, actors a 
WHERE m.slug = 'the-dark-knight' AND a.slug = 'christian-bale';

INSERT INTO movie_actors (movie_id, actor_id) 
SELECT m.id, a.id FROM movies m, actors a 
WHERE m.slug = 'the-dark-knight' AND a.slug = 'heath-ledger';

-- ============================================
-- 5. CINEMAS
-- ============================================
INSERT INTO cinemas (name, slug, description, address, city, phone, email, website, latitude, longitude, banner_url, total_screens, facilities, is_active) VALUES 
('CinePass Cinema Quận 1', 'cinepass-quan-1', 'Rạp chiếu phim hiện đại tại trung tâm Sài Gòn', '123 Nguyễn Huệ, Quận 1, TP. HCM', 'Hồ Chí Minh', '0901234567', 'quan1@cinepass.com', 'https://cinepass.com/quan1', 10.7769, 106.7007, 'https://cinepass.com/banners/quan1.jpg', 8, ARRAY['Parking', 'Free WiFi', 'Dolby Atmos', '4DX', 'IMAX'], true),
('CinePass Cinema Quận 3', 'cinepass-quan-3', 'Rạp chiếu phim cao cấp với công nghệ tiên tiến', '456 Võ Văn Tần, Quận 3, TP. HCM', 'Hồ Chí Minh', '0901234568', 'quan3@cinepass.com', 'https://cinepass.com/quan3', 10.7830, 106.6920, 'https://cinepass.com/banners/quan3.jpg', 6, ARRAY['Parking', 'Free WiFi', 'Dolby Atmos', 'Premium Lounge'], true),
('CinePass Cinema Thủ Đức', 'cinepass-thu-duc', 'Rạp chiếu phim phục vụ khu vực Thủ Đức', '789 Võ Văn Ngân, Thủ Đức, TP. HCM', 'Hồ Chí Minh', '0901234569', 'thuduc@cinepass.com', 'https://cinepass.com/thuduc', 10.8506, 106.7719, 'https://cinepass.com/banners/thuduc.jpg', 5, ARRAY['Parking', 'Free WiFi', 'Dolby Atmos'], true),
('CinePass Cinema Hà Nội', 'cinepass-ha-noi', 'Rạp chiếu phim đẳng cấp tại Hà Nội', '101 Trần Duy Hưng, Cầu Giấy, Hà Nội', 'Hà Nội', '0241234567', 'hanoi@cinepass.com', 'https://cinepass.com/hanoi', 21.0285, 105.7863, 'https://cinepass.com/banners/hanoi.jpg', 7, ARRAY['Parking', 'Free WiFi', 'Dolby Atmos', 'IMAX'], true),
('CinePass Cinema Đà Nẵng', 'cinepass-da-nang', 'Rạp chiếu phim view biển tại Đà Nẵng', '202 Trần Phú, Hải Châu, Đà Nẵng', 'Đà Nẵng', '0511234567', 'danang@cinepass.com', 'https://cinepass.com/danang', 16.0544, 108.2022, 'https://cinepass.com/banners/danang.jpg', 4, ARRAY['Parking', 'Free WiFi', 'Ocean View', 'Dolby Atmos'], true);

-- ============================================
-- 6. SEAT TYPES
-- ============================================
INSERT INTO seat_types (code, name, surcharge_rate) VALUES 
('NORMAL', 'Ghế Thường', 1.0),
('VIP', 'Ghế VIP', 1.5),
('COUPLE', 'Ghế Đôi', 2.0),
('DELUXE', 'Ghế Deluxe', 1.8);

-- ============================================
-- 7. SCREENS
-- ============================================
-- Cinema 1: 8 screens
INSERT INTO screens (cinema_id, name, total_seats, seat_map_layout) 
SELECT id, 'Phòng 1 - IMAX', 120, '{"rows": 10, "cols": 12, "vipRows": [7,8,9]}'::jsonb FROM cinemas WHERE slug = 'cinepass-quan-1' LIMIT 1;

INSERT INTO screens (cinema_id, name, total_seats, seat_map_layout) 
SELECT id, 'Phòng 2 - 4DX', 100, '{"rows": 10, "cols": 10, "vipRows": [8,9]}'::jsonb FROM cinemas WHERE slug = 'cinepass-quan-1' LIMIT 1;

INSERT INTO screens (cinema_id, name, total_seats, seat_map_layout) 
SELECT id, 'Phòng 3 - Standard', 80, '{"rows": 8, "cols": 10, "vipRows": [6,7]}'::jsonb FROM cinemas WHERE slug = 'cinepass-quan-1' LIMIT 1;

-- Cinema 2: 6 screens
INSERT INTO screens (cinema_id, name, total_seats, seat_map_layout) 
SELECT id, 'Phòng 1 - Premium', 90, '{"rows": 9, "cols": 10, "vipRows": [7,8]}'::jsonb FROM cinemas WHERE slug = 'cinepass-quan-3' LIMIT 1;

INSERT INTO screens (cinema_id, name, total_seats, seat_map_layout) 
SELECT id, 'Phòng 2 - Standard', 70, '{"rows": 7, "cols": 10, "vipRows": [5,6]}'::jsonb FROM cinemas WHERE slug = 'cinepass-quan-3' LIMIT 1;

-- Cinema 3: 5 screens
INSERT INTO screens (cinema_id, name, total_seats, seat_map_layout) 
SELECT id, 'Phòng 1 - Standard', 80, '{"rows": 8, "cols": 10, "vipRows": [6,7]}'::jsonb FROM cinemas WHERE slug = 'cinepass-thu-duc' LIMIT 1;

-- Cinema 4: 7 screens
INSERT INTO screens (cinema_id, name, total_seats, seat_map_layout) 
SELECT id, 'Phòng 1 - IMAX', 150, '{"rows": 12, "cols": 13, "vipRows": [9,10,11]}'::jsonb FROM cinemas WHERE slug = 'cinepass-ha-noi' LIMIT 1;

-- Cinema 5: 4 screens
INSERT INTO screens (cinema_id, name, total_seats, seat_map_layout) 
SELECT id, 'Phòng 1 - Ocean View', 60, '{"rows": 6, "cols": 10, "vipRows": [4,5]}'::jsonb FROM cinemas WHERE slug = 'cinepass-da-nang' LIMIT 1;

-- ============================================
-- 8. SEATS (Sample for Screen 1 only)
-- ============================================
-- Normal seats rows A-F (60 seats)
INSERT INTO seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) 
SELECT id, 'A', generate_series(1, 12), 'A' || generate_series(1, 12), 'NORMAL' 
FROM screens WHERE name = 'Phòng 1 - IMAX' LIMIT 1;

INSERT INTO seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) 
SELECT id, 'B', generate_series(1, 12), 'B' || generate_series(1, 12), 'NORMAL' 
FROM screens WHERE name = 'Phòng 1 - IMAX' LIMIT 1;

INSERT INTO seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) 
SELECT id, 'C', generate_series(1, 12), 'C' || generate_series(1, 12), 'NORMAL' 
FROM screens WHERE name = 'Phòng 1 - IMAX' LIMIT 1;

INSERT INTO seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) 
SELECT id, 'D', generate_series(1, 12), 'D' || generate_series(1, 12), 'NORMAL' 
FROM screens WHERE name = 'Phòng 1 - IMAX' LIMIT 1;

INSERT INTO seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) 
SELECT id, 'E', generate_series(1, 12), 'E' || generate_series(1, 12), 'NORMAL' 
FROM screens WHERE name = 'Phòng 1 - IMAX' LIMIT 1;

INSERT INTO seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) 
SELECT id, 'F', generate_series(1, 12), 'F' || generate_series(1, 12), 'NORMAL' 
FROM screens WHERE name = 'Phòng 1 - IMAX' LIMIT 1;

-- VIP seats rows G-I (36 seats)
INSERT INTO seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) 
SELECT id, 'G', generate_series(1, 12), 'G' || generate_series(1, 12), 'VIP' 
FROM screens WHERE name = 'Phòng 1 - IMAX' LIMIT 1;

INSERT INTO seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) 
SELECT id, 'H', generate_series(1, 12), 'H' || generate_series(1, 12), 'VIP' 
FROM screens WHERE name = 'Phòng 1 - IMAX' LIMIT 1;

INSERT INTO seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) 
SELECT id, 'I', generate_series(1, 12), 'I' || generate_series(1, 12), 'VIP' 
FROM screens WHERE name = 'Phòng 1 - IMAX' LIMIT 1;

-- Couple seats row J (12 seats = 6 couple seats)
INSERT INTO seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) 
SELECT id, 'J', generate_series(1, 12), 'J' || generate_series(1, 12), 'COUPLE' 
FROM screens WHERE name = 'Phòng 1 - IMAX' LIMIT 1;

-- ============================================
-- 9. SHOWTIMES
-- ============================================
-- Avatar showtimes
INSERT INTO showtimes (movie_id, screen_id, start_time, end_time, base_price, is_active)
SELECT m.id, s.id, '2024-12-16 09:00:00', '2024-12-16 12:12:00', 80000, true
FROM movies m, screens s 
WHERE m.slug = 'avatar-the-way-of-water' AND s.name = 'Phòng 1 - IMAX' LIMIT 1;

INSERT INTO showtimes (movie_id, screen_id, start_time, end_time, base_price, is_active)
SELECT m.id, s.id, '2024-12-16 14:00:00', '2024-12-16 17:12:00', 90000, true
FROM movies m, screens s 
WHERE m.slug = 'avatar-the-way-of-water' AND s.name = 'Phòng 1 - IMAX' LIMIT 1;

INSERT INTO showtimes (movie_id, screen_id, start_time, end_time, base_price, is_active)
SELECT m.id, s.id, '2024-12-16 19:00:00', '2024-12-16 22:12:00', 100000, true
FROM movies m, screens s 
WHERE m.slug = 'avatar-the-way-of-water' AND s.name = 'Phòng 1 - IMAX' LIMIT 1;

-- Avengers showtimes
INSERT INTO showtimes (movie_id, screen_id, start_time, end_time, base_price, is_active)
SELECT m.id, s.id, '2024-12-16 10:00:00', '2024-12-16 13:01:00', 75000, true
FROM movies m, screens s 
WHERE m.slug = 'avengers-endgame' AND s.name = 'Phòng 2 - 4DX' LIMIT 1;

INSERT INTO showtimes (movie_id, screen_id, start_time, end_time, base_price, is_active)
SELECT m.id, s.id, '2024-12-16 15:30:00', '2024-12-16 18:31:00', 85000, true
FROM movies m, screens s 
WHERE m.slug = 'avengers-endgame' AND s.name = 'Phòng 2 - 4DX' LIMIT 1;

-- Spider-Man showtimes
INSERT INTO showtimes (movie_id, screen_id, start_time, end_time, base_price, is_active)
SELECT m.id, s.id, '2024-12-16 11:00:00', '2024-12-16 13:28:00', 70000, true
FROM movies m, screens s 
WHERE m.slug = 'spider-man-no-way-home' AND s.name = 'Phòng 3 - Standard' LIMIT 1;

INSERT INTO showtimes (movie_id, screen_id, start_time, end_time, base_price, is_active)
SELECT m.id, s.id, '2024-12-16 20:00:00', '2024-12-16 22:28:00', 80000, true
FROM movies m, screens s 
WHERE m.slug = 'spider-man-no-way-home' AND s.name = 'Phòng 3 - Standard' LIMIT 1;

-- ============================================
-- 10. PRODUCTS
-- ============================================
-- Category: 0=Food, 1=Drink, 2=Combo
INSERT INTO products (name, description, price, image_url, category, is_active) VALUES 
('Bắp Rang Bơ Lớn', 'Bắp rang bơ thơm ngon, size lớn', 60000, 'https://cinepass.com/products/popcorn-large.jpg', 0, true),
('Bắp Rang Bơ Vừa', 'Bắp rang bơ thơm ngon, size vừa', 45000, 'https://cinepass.com/products/popcorn-medium.jpg', 0, true),
('Bắp Rang Bơ Nhỏ', 'Bắp rang bơ thơm ngon, size nhỏ', 30000, 'https://cinepass.com/products/popcorn-small.jpg', 0, true),
('Coca Cola Lớn', 'Nước ngọt Coca Cola, size lớn', 35000, 'https://cinepass.com/products/coke-large.jpg', 1, true),
('Coca Cola Vừa', 'Nước ngọt Coca Cola, size vừa', 25000, 'https://cinepass.com/products/coke-medium.jpg', 1, true),
('Pepsi Lớn', 'Nước ngọt Pepsi, size lớn', 35000, 'https://cinepass.com/products/pepsi-large.jpg', 1, true),
('Nước Suối', 'Nước suối tinh khiết', 15000, 'https://cinepass.com/products/water.jpg', 1, true),
('Combo 1 (Bắp + Nước)', 'Bắp rang bơ lớn + 2 Coca lớn', 100000, 'https://cinepass.com/products/combo1.jpg', 2, true),
('Combo 2 (Bắp Family)', 'Bắp rang bơ lớn + 4 Coca vừa', 150000, 'https://cinepass.com/products/combo2.jpg', 2, true),
('Combo 3 (Couple)', 'Bắp rang bơ vừa + 2 Coca vừa', 80000, 'https://cinepass.com/products/combo3.jpg', 2, true);

-- ============================================
-- 11. ORDERS
-- ============================================
-- Status: 0=Pending, 1=Confirmed, 2=Cancelled, 3=Refunded
INSERT INTO orders (user_id, total_amount, status, payment_method, expire_at) 
SELECT u.id, 260000, 1, 'VNPay', NOW() + INTERVAL '24 hours'
FROM users u WHERE u.email = 'customer1@gmail.com' LIMIT 1;

INSERT INTO orders (user_id, total_amount, status, payment_method, expire_at) 
SELECT u.id, 340000, 1, 'MoMo', NOW() + INTERVAL '24 hours'
FROM users u WHERE u.email = 'customer2@gmail.com' LIMIT 1;

INSERT INTO orders (user_id, total_amount, status, payment_method, expire_at) 
SELECT u.id, 180000, 0, NULL, NOW() + INTERVAL '15 minutes'
FROM users u WHERE u.email = 'customer3@gmail.com' LIMIT 1;

-- ============================================
-- 12. ORDER_TICKETS
-- ============================================
-- Order 1: 2 VIP tickets for Avatar 14:00 showtime
INSERT INTO order_tickets (order_id, showtime_id, seat_id, price)
SELECT o.id, st.id, s.id, 135000
FROM orders o, showtimes st, seats s, movies m, screens sc
WHERE o.total_amount = 260000 
  AND st.start_time = '2024-12-16 14:00:00'
  AND m.slug = 'avatar-the-way-of-water'
  AND st.movie_id = m.id
  AND st.screen_id = sc.id
  AND s.screen_id = sc.id
  AND s.seat_code = 'G5'
LIMIT 1;

INSERT INTO order_tickets (order_id, showtime_id, seat_id, price)
SELECT o.id, st.id, s.id, 135000
FROM orders o, showtimes st, seats s, movies m, screens sc
WHERE o.total_amount = 260000 
  AND st.start_time = '2024-12-16 14:00:00'
  AND m.slug = 'avatar-the-way-of-water'
  AND st.movie_id = m.id
  AND st.screen_id = sc.id
  AND s.screen_id = sc.id
  AND s.seat_code = 'G6'
LIMIT 1;

-- Order 2: 2 COUPLE tickets for Avatar 19:00 showtime
INSERT INTO order_tickets (order_id, showtime_id, seat_id, price)
SELECT o.id, st.id, s.id, 200000
FROM orders o, showtimes st, seats s, movies m, screens sc
WHERE o.total_amount = 340000 
  AND st.start_time = '2024-12-16 19:00:00'
  AND m.slug = 'avatar-the-way-of-water'
  AND st.movie_id = m.id
  AND st.screen_id = sc.id
  AND s.screen_id = sc.id
  AND s.seat_code = 'J5'
LIMIT 1;

INSERT INTO order_tickets (order_id, showtime_id, seat_id, price)
SELECT o.id, st.id, s.id, 200000
FROM orders o, showtimes st, seats s, movies m, screens sc
WHERE o.total_amount = 340000 
  AND st.start_time = '2024-12-16 19:00:00'
  AND m.slug = 'avatar-the-way-of-water'
  AND st.movie_id = m.id
  AND st.screen_id = sc.id
  AND s.screen_id = sc.id
  AND s.seat_code = 'J6'
LIMIT 1;

-- Order 3: 2 Normal tickets for Spider-Man 11:00 showtime
INSERT INTO order_tickets (order_id, showtime_id, seat_id, price)
SELECT o.id, st.id, s.id, 70000
FROM orders o, showtimes st, seats s, movies m, screens sc
WHERE o.total_amount = 180000 
  AND st.start_time = '2024-12-16 11:00:00'
  AND m.slug = 'spider-man-no-way-home'
  AND st.movie_id = m.id
  AND st.screen_id = sc.id
  AND s.screen_id = sc.id
  AND s.seat_code = 'A1'
LIMIT 1;

INSERT INTO order_tickets (order_id, showtime_id, seat_id, price)
SELECT o.id, st.id, s.id, 70000
FROM orders o, showtimes st, seats s, movies m, screens sc
WHERE o.total_amount = 180000 
  AND st.start_time = '2024-12-16 11:00:00'
  AND m.slug = 'spider-man-no-way-home'
  AND st.movie_id = m.id
  AND st.screen_id = sc.id
  AND s.screen_id = sc.id
  AND s.seat_code = 'A2'
LIMIT 1;

-- ============================================
-- 13. ORDER_PRODUCTS
-- ============================================
-- Order 1: No products

-- Order 2: Combo 3
INSERT INTO order_products (order_id, product_id, quantity, unit_price)
SELECT o.id, p.id, 1, 80000
FROM orders o, products p
WHERE o.total_amount = 340000 AND p.name = 'Combo 3 (Couple)'
LIMIT 1;

-- Order 3: Bắp Rang Bơ Nhỏ + Nước Suối
INSERT INTO order_products (order_id, product_id, quantity, unit_price)
SELECT o.id, p.id, 1, 30000
FROM orders o, products p
WHERE o.total_amount = 180000 AND p.name = 'Bắp Rang Bơ Nhỏ'
LIMIT 1;

INSERT INTO order_products (order_id, product_id, quantity, unit_price)
SELECT o.id, p.id, 2, 15000
FROM orders o, products p
WHERE o.total_amount = 180000 AND p.name = 'Nước Suối'
LIMIT 1;

-- ============================================
-- 14. E_TICKETS
-- ============================================
-- E-Tickets for confirmed orders only
INSERT INTO e_tickets (order_ticket_id, ticket_code, qr_data, is_used, used_at)
SELECT ot.id, 
       'TKT-' || UPPER(SUBSTRING(MD5(RANDOM()::text) FROM 1 FOR 10)),
       'QRDATA-' || ot.id::text || '-' || EXTRACT(EPOCH FROM NOW())::text,
       false,
       NULL
FROM order_tickets ot
INNER JOIN orders o ON ot.order_id = o.id
WHERE o.status = 1; -- Confirmed orders only

-- Mark one ticket as used (for testing)
UPDATE e_tickets SET is_used = true, used_at = NOW() 
WHERE id = (SELECT id FROM e_tickets ORDER BY id LIMIT 1);

-- ============================================
-- 15. PAYMENT_TRANSACTIONS
-- ============================================
INSERT INTO payment_transactions (order_id, provider_trans_id, amount, status, response_json)
SELECT o.id, 
       'VNPAY-' || UPPER(SUBSTRING(MD5(RANDOM()::text) FROM 1 FOR 12)),
       260000,
       'success',
       '{"transactionId": "VNP123456", "bankCode": "NCB", "cardType": "ATM"}'::jsonb
FROM orders o WHERE o.total_amount = 260000 LIMIT 1;

INSERT INTO payment_transactions (order_id, provider_trans_id, amount, status, response_json)
SELECT o.id, 
       'MOMO-' || UPPER(SUBSTRING(MD5(RANDOM()::text) FROM 1 FOR 12)),
       340000,
       'success',
       '{"partnerCode": "MOMO", "orderId": "MM123456", "payType": "qr"}'::jsonb
FROM orders o WHERE o.total_amount = 340000 LIMIT 1;

-- ============================================
-- 16. MOVIE_REVIEWS
-- ============================================
INSERT INTO movie_reviews (movie_id, user_id, rating, comment)
SELECT m.id, u.id, 5, 'Phim rất hay! Hình ảnh đẹp mắt, kỹ xảo tuyệt vời!'
FROM movies m, users u 
WHERE m.slug = 'avatar-the-way-of-water' AND u.email = 'customer1@gmail.com' LIMIT 1;

INSERT INTO movie_reviews (movie_id, user_id, rating, comment)
SELECT m.id, u.id, 4, 'Phim hay nhưng hơi dài. Tuy nhiên rất đáng xem!'
FROM movies m, users u 
WHERE m.slug = 'avatar-the-way-of-water' AND u.email = 'customer2@gmail.com' LIMIT 1;

INSERT INTO movie_reviews (movie_id, user_id, rating, comment)
SELECT m.id, u.id, 5, 'Đỉnh cao điện ảnh Marvel! Cảm động và hoành tráng!'
FROM movies m, users u 
WHERE m.slug = 'avengers-endgame' AND u.email = 'customer3@gmail.com' LIMIT 1;

INSERT INTO movie_reviews (movie_id, user_id, rating, comment)
SELECT m.id, u.id, 4, 'Spider-Man hay nhất từ trước đến nay!'
FROM movies m, users u 
WHERE m.slug = 'spider-man-no-way-home' AND u.email = 'customer4@gmail.com' LIMIT 1;

-- ============================================
-- 17. MEMBER_POINTS
-- ============================================
INSERT INTO member_points (user_id, points)
SELECT id, 1500 FROM users WHERE email = 'customer1@gmail.com';

INSERT INTO member_points (user_id, points)
SELECT id, 2300 FROM users WHERE email = 'customer2@gmail.com';

INSERT INTO member_points (user_id, points)
SELECT id, 500 FROM users WHERE email = 'customer3@gmail.com';

INSERT INTO member_points (user_id, points)
SELECT id, 800 FROM users WHERE email = 'customer4@gmail.com';

INSERT INTO member_points (user_id, points)
SELECT id, 0 FROM users WHERE email = 'customer5@gmail.com';

-- ============================================
-- 18. POINT_HISTORY
-- ============================================
-- Type: 0=Purchase, 1=Refund, 2=Reward
INSERT INTO point_history (user_id, points, type)
SELECT u.id, 260, 0 FROM users u WHERE u.email = 'customer1@gmail.com';

INSERT INTO point_history (user_id, points, type)
SELECT u.id, 340, 0 FROM users u WHERE u.email = 'customer2@gmail.com';

INSERT INTO point_history (user_id, points, type)
SELECT u.id, 500, 2 FROM users u WHERE u.email = 'customer1@gmail.com';

INSERT INTO point_history (user_id, points, type)
SELECT u.id, 1000, 2 FROM users u WHERE u.email = 'customer2@gmail.com';

-- ============================================
-- 19. REWARDS
-- ============================================
INSERT INTO rewards (name, slug, description, image_url, points) VALUES 
('Giảm 50k cho đơn hàng tiếp theo', 'giam-50k', 'Voucher giảm giá 50,000đ cho đơn hàng tối thiểu 200,000đ', 'https://cinepass.com/rewards/voucher-50k.jpg', 500),
('Combo bắp nước miễn phí', 'combo-free', 'Nhận miễn phí 1 combo bắp nước size vừa', 'https://cinepass.com/rewards/combo-free.jpg', 800),
('Vé xem phim miễn phí', 've-mien-phi', 'Vé xem phim miễn phí (áp dụng suất chiếu trong tuần)', 'https://cinepass.com/rewards/free-ticket.jpg', 1500),
('Nâng hạng ghế VIP', 'upgrade-vip', 'Nâng hạng miễn phí từ ghế thường lên ghế VIP', 'https://cinepass.com/rewards/upgrade-vip.jpg', 1000),
('Giảm 100k cho đơn hàng tiếp theo', 'giam-100k', 'Voucher giảm giá 100,000đ cho đơn hàng tối thiểu 500,000đ', 'https://cinepass.com/rewards/voucher-100k.jpg', 2000);

-- ============================================
-- END OF SAMPLE DATA
-- ============================================
