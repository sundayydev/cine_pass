--user
insert into users (email, phone, password_hash, full_name, role) values ('admin@gmail.com', '0909090909', '123456', 'Admin', 2);

-- movie
insert into movies (title, slug, description, duration, release_date, poster_url, trailer_url, is_active) values ('Phim 1', 'phim-1', 'Phim 1', 120, '2025-01-01', 'https://www.phim1.com/poster.jpg', 'https://www.phim1.com/trailer.mp4', true);
insert into movies (title, slug, description, duration, release_date, poster_url, trailer_url, is_active) values ('Phim 2', 'phim-2', 'Phim 2', 120, '2025-01-01', 'https://www.phim2.com/poster.jpg', 'https://www.phim2.com/trailer.mp4', true);
insert into movies (title, slug, description, duration, release_date, poster_url, trailer_url, is_active) values ('Phim 3', 'phim-3', 'Phim 3', 120, '2025-01-01', 'https://www.phim3.com/poster.jpg', 'https://www.phim3.com/trailer.mp4', true);
insert into movies (title, slug, description, duration, release_date, poster_url, trailer_url, is_active) values ('Phim 4', 'phim-4', 'Phim 4', 120, '2025-01-01', 'https://www.phim4.com/poster.jpg', 'https://www.phim4.com/trailer.mp4', true);
insert into movies (title, slug, description, duration, release_date, poster_url, trailer_url, is_active) values ('Phim 5', 'phim-5', 'Phim 5', 120, '2025-01-01', 'https://www.phim5.com/poster.jpg', 'https://www.phim5.com/trailer.mp4', true);
insert into movies (title, slug, description, duration, release_date, poster_url, trailer_url, is_active) values ('Phim 6', 'phim-6', 'Phim 6', 120, '2025-01-01', 'https://www.phim6.com/poster.jpg', 'https://www.phim6.com/trailer.mp4', true);
insert into movies (title, slug, description, duration, release_date, poster_url, trailer_url, is_active) values ('Phim 7', 'phim-7', 'Phim 7', 120, '2025-01-01', 'https://www.phim7.com/poster.jpg', 'https://www.phim7.com/trailer.mp4', true);
insert into movies (title, slug, description, duration, release_date, poster_url, trailer_url, is_active) values ('Phim 8', 'phim-8', 'Phim 8', 120, '2025-01-01', 'https://www.phim8.com/poster.jpg', 'https://www.phim8.com/trailer.mp4', true);
insert into movies (title, slug, description, duration, release_date, poster_url, trailer_url, is_active) values ('Phim 9', 'phim-9', 'Phim 9', 120, '2025-01-01', 'https://www.phim9.com/poster.jpg', 'https://www.phim9.com/trailer.mp4', true);
insert into movies (title, slug, description, duration, release_date, poster_url, trailer_url, is_active) values ('Phim 10', 'phim-10', 'Phim 10', 120, '2025-01-01', 'https://www.phim10.com/poster.jpg', 'https://www.phim10.com/trailer.mp4', true);

--cinema
insert into cinemas (name, slug, description, address, city, phone, email, website, latitude, longitude, banner_url, total_screens, facilities, is_active) values ('Rạp 1', 'rap-1', 'Rạp 1', '123 Đường ABC, Quận XYZ, TP. HCM', 'TP. HCM', '0909090909', 'rap1@gmail.com', 'https://www.rap1.com', 10.7769, 106.7007, 'https://www.rap1.com/banner.jpg', 10, ARRAY['Parking', 'Wifi', 'Dolby Atmos'], true);
insert into cinemas (name, slug, description, address, city, phone, email, website, latitude, longitude, banner_url, total_screens, facilities, is_active) values ('Rạp 2', 'rap-2', 'Rạp 2', '123 Đường ABC, Quận XYZ, TP. HCM', 'TP. HCM', '0909090909', 'rap2@gmail.com', 'https://www.rap2.com', 10.7769, 106.7007, 'https://www.rap2.com/banner.jpg', 10, ARRAY['Parking', 'Wifi', 'Dolby Atmos'], true);
insert into cinemas (name, slug, description, address, city, phone, email, website, latitude, longitude, banner_url, total_screens, facilities, is_active) values ('Rạp 3', 'rap-3', 'Rạp 3', '123 Đường ABC, Quận XYZ, TP. HCM', 'TP. HCM', '0909090909', 'rap3@gmail.com', 'https://www.rap3.com', 10.7769, 106.7007, 'https://www.rap3.com/banner.jpg', 10, ARRAY['Parking', 'Wifi', 'Dolby Atmos'], true);
insert into cinemas (name, slug, description, address, city, phone, email, website, latitude, longitude, banner_url, total_screens, facilities, is_active) values ('Rạp 4', 'rap-4', 'Rạp 4', '123 Đường ABC, Quận XYZ, TP. HCM', 'TP. HCM', '0909090909', 'rap4@gmail.com', 'https://www.rap4.com', 10.7769, 106.7007, 'https://www.rap4.com/banner.jpg', 10, ARRAY['Parking', 'Wifi', 'Dolby Atmos'], true);
insert into cinemas (name, slug, description, address, city, phone, email, website, latitude, longitude, banner_url, total_screens, facilities, is_active) values ('Rạp 5', 'rap-5', 'Rạp 5', '123 Đường ABC, Quận XYZ, TP. HCM', 'TP. HCM', '0909090909', 'rap5@gmail.com', 'https://www.rap5.com', 10.7769, 106.7007, 'https://www.rap5.com/banner.jpg', 10, ARRAY['Parking', 'Wifi', 'Dolby Atmos'], true);
insert into cinemas (name, slug, description, address, city, phone, email, website, latitude, longitude, banner_url, total_screens, facilities, is_active) values ('Rạp 6', 'rap-6', 'Rạp 6', '123 Đường ABC, Quận XYZ, TP. HCM', 'TP. HCM', '0909090909', 'rap6@gmail.com', 'https://www.rap6.com', 10.7769, 106.7007, 'https://www.rap6.com/banner.jpg', 10, ARRAY['Parking', 'Wifi', 'Dolby Atmos'], true);
insert into cinemas (name, slug, description, address, city, phone, email, website, latitude, longitude, banner_url, total_screens, facilities, is_active) values ('Rạp 7', 'rap-7', 'Rạp 7', '123 Đường ABC, Quận XYZ, TP. HCM', 'TP. HCM', '0909090909', 'rap7@gmail.com', 'https://www.rap7.com', 10.7769, 106.7007, 'https://www.rap7.com/banner.jpg', 10, ARRAY['Parking', 'Wifi', 'Dolby Atmos'], true);
insert into cinemas (name, slug, description, address, city, phone, email, website, latitude, longitude, banner_url, total_screens, facilities, is_active) values ('Rạp 8', 'rap-8', 'Rạp 8', '123 Đường ABC, Quận XYZ, TP. HCM', 'TP. HCM', '0909090909', 'rap8@gmail.com', 'https://www.rap8.com', 10.7769, 106.7007, 'https://www.rap8.com/banner.jpg', 10, ARRAY['Parking', 'Wifi', 'Dolby Atmos'], true);
insert into cinemas (name, slug, description, address, city, phone, email, website, latitude, longitude, banner_url, total_screens, facilities, is_active) values ('Rạp 9', 'rap-9', 'Rạp 9', '123 Đường ABC, Quận XYZ, TP. HCM', 'TP. HCM', '0909090909', 'rap9@gmail.com', 'https://www.rap9.com', 10.7769, 106.7007, 'https://www.rap9.com/banner.jpg', 10, ARRAY['Parking', 'Wifi', 'Dolby Atmos'], true);
insert into cinemas (name, slug, description, address, city, phone, email, website, latitude, longitude, banner_url, total_screens, facilities, is_active) values ('Rạp 10', 'rap-10', 'Rạp 10', '123 Đường ABC, Quận XYZ, TP. HCM', 'TP. HCM', '0909090909', 'rap10@gmail.com', 'https://www.rap10.com', 10.7769, 106.7007, 'https://www.rap10.com/banner.jpg', 10, ARRAY['Parking', 'Wifi', 'Dolby Atmos'], true);

insert into seat_types (code, name, surcharge_rate) values ('NORMAL', 'Ghế thường', 1.0);
insert into seat_types (code, name, surcharge_rate) values ('VIP', 'Ghế VIP', 1.2);
insert into seat_types (code, name, surcharge_rate) values ('COUPLE', 'Ghế đôi', 2.0);

insert into screens (name, total_seats, seat_map_layout) values ('Phòng 1', 100, '{"rows": 10, "cols": 10}');
insert into screens (name, total_seats, seat_map_layout) values ('Phòng 2', 100, '{"rows": 10, "cols": 10}');
insert into screens (name, total_seats, seat_map_layout) values ('Phòng 3', 100, '{"rows": 10, "cols": 10}');
insert into screens (name, total_seats, seat_map_layout) values ('Phòng 4', 100, '{"rows": 10, "cols": 10}');
insert into screens (name, total_seats, seat_map_layout) values ('Phòng 5', 100, '{"rows": 10, "cols": 10}');
insert into screens (name, total_seats, seat_map_layout) values ('Phòng 6', 100, '{"rows": 10, "cols": 10}');
insert into screens (name, total_seats, seat_map_layout) values ('Phòng 7', 100, '{"rows": 10, "cols": 10}');
insert into screens (name, total_seats, seat_map_layout) values ('Phòng 8', 100, '{"rows": 10, "cols": 10}');
insert into screens (name, total_seats, seat_map_layout) values ('Phòng 9', 100, '{"rows": 10, "cols": 10}');
insert into screens (name, total_seats, seat_map_layout) values ('Phòng 10', 100, '{"rows": 10, "cols": 10}');

insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (1, 'A', 1, 'A1', 'NORMAL'); 
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (1, 'A', 2, 'A2', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (1, 'A', 3, 'A3', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (1, 'A', 4, 'A4', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (1, 'A', 5, 'A5', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (1, 'A', 6, 'A6', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (1, 'A', 7, 'A7', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (1, 'A', 8, 'A8', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (1, 'A', 9, 'A9', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (1, 'A', 10, 'A10', 'NORMAL');

insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (2, 'B', 1, 'B1', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (2, 'B', 2, 'B2', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (2, 'B', 3, 'B3', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (2, 'B', 4, 'B4', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (2, 'B', 5, 'B5', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (2, 'B', 6, 'B6', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (2, 'B', 7, 'B7', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (2, 'B', 8, 'B8', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (2, 'B', 9, 'B9', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (2, 'B', 10, 'B10', 'NORMAL');

insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (3, 'C', 1, 'C1', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (3, 'C', 2, 'C2', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (3, 'C', 3, 'C3', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (3, 'C', 4, 'C4', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (3, 'C', 5, 'C5', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (3, 'C', 6, 'C6', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (3, 'C', 7, 'C7', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (3, 'C', 8, 'C8', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (3, 'C', 9, 'C9', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (3, 'C', 10, 'C10', 'NORMAL');

insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (4, 'D', 1, 'D1', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (4, 'D', 2, 'D2', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (4, 'D', 3, 'D3', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (4, 'D', 4, 'D4', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (4, 'D', 5, 'D5', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (4, 'D', 6, 'D6', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (4, 'D', 7, 'D7', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (4, 'D', 8, 'D8', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (4, 'D', 9, 'D9', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (4, 'D', 10, 'D10', 'NORMAL');

insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (5, 'E', 1, 'E1', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (5, 'E', 2, 'E2', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (5, 'E', 3, 'E3', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (5, 'E', 4, 'E4', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (5, 'E', 5, 'E5', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (5, 'E', 6, 'E6', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (5, 'E', 7, 'E7', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (5, 'E', 8, 'E8', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (5, 'E', 9, 'E9', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (5, 'E', 10, 'E10', 'NORMAL');

insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (6, 'F', 1, 'F1', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (6, 'F', 2, 'F2', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (6, 'F', 3, 'F3', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (6, 'F', 4, 'F4', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (6, 'F', 5, 'F5', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (6, 'F', 6, 'F6', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (6, 'F', 7, 'F7', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (6, 'F', 8, 'F8', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (6, 'F', 9, 'F9', 'NORMAL');
insert into seats (screen_id, seat_row, seat_number, seat_code, seat_type_code) values (6, 'F', 10, 'F10', 'NORMAL');
