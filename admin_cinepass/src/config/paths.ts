export const PATHS = {
    // Public
    LOGIN: '/login',
    FORGOT_PASSWORD: '/forgot-password',
    RESET_PASSWORD: '/reset-password',

    // Private - Dashboard
    ADMIN: '/',
    DASHBOARD: '/dashboard',

    // Movies (Quản lý phim)
    MOVIES: '/movies',
    MOVIE_CREATE: '/movies/create',
    MOVIE_EDIT: '/movies/edit/:id', // Dùng dynamic param :id
    MOVIE_DETAIL: '/movies/:slug', // Dùng slug để xem chi tiết

    // Seat Types (Quản lý Loại Ghế)
    SEAT_TYPES: '/seat-types',
    SEAT_TYPE_CREATE: '/seat-types/create',
    SEAT_TYPE_EDIT: '/seat-types/edit/:code',
    SEAT_TYPE_DETAIL: '/seat-types/:code',

    // Cinemas (Quản lý Rạp & Phòng & Ghế)
    CINEMAS: '/cinemas',
    CINEMA_CREATE: '/cinemas/create',
    CINEMA_DETAIL: '/cinemas/:id',
    // Quản lý phòng chiếu trong cụm rạp cụ thể
    ROOMS: '/cinemas/:cinemaId/rooms', 
    ROOM_SEAT_MAP: '/cinemas/:cinemaId/rooms/:roomId/seat-map', // Trang vẽ sơ đồ ghế

    // Showtimes (Lịch chiếu)
    SHOWTIMES: '/showtimes',
    SHOWTIME_CREATE: '/showtimes/create',

    // Bookings (Đơn hàng)
    BOOKINGS: '/bookings',
    BOOKING_DETAIL: '/bookings/:id',

    // Users & Settings
    USERS: '/users',
    SETTINGS: '/settings',
};