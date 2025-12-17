import axiosClient from '@/lib/axiosClient';

// ==================== TYPES ====================

/**
 * Generic API Response wrapper
 */
export interface ApiResponseDto<T> {
  success: boolean;
  message?: string;
  data?: T;
  errors?: string[];
}

/**
 * DTO cho response từ backend (CinemaResponseDto)
 */
export interface CinemaResponseDto {
  id: string;
  name: string;
  slug: string;
  description?: string;

  // Địa điểm
  address?: string;
  city?: string;

  // Tọa độ & Liên hệ
  latitude?: number;
  longitude?: number;
  phone?: string;
  email?: string;
  website?: string;

  // Hình ảnh & Tiện ích
  bannerUrl?: string;
  totalScreens: number;
  facilities?: string[];

  // Metadata
  createdAt: string;
  updatedAt: string;
  isActive: boolean;
}

/**
 * DTO cho tạo cinema mới (CinemaCreateDto)
 */
export interface CinemaCreateDto {
  // Thông tin cơ bản
  name: string;
  slug?: string; // Backend sẽ tự generate nếu null
  description?: string;

  // Liên hệ & Địa điểm
  address?: string;
  city?: string;
  phone?: string;
  email?: string;
  website?: string;

  // Tọa độ
  latitude?: number;
  longitude?: number;

  // Hình ảnh
  bannerUrl?: string;

  // Thông tin phụ
  totalScreens?: number;
  facilities?: string[];

  // Trạng thái
  isActive?: boolean;
}

/**
 * DTO cho cập nhật cinema (CinemaUpdateDto)
 */
export interface CinemaUpdateDto {
  // Thông tin cơ bản
  name?: string;
  slug?: string;
  description?: string;

  // Liên hệ & Địa điểm
  address?: string;
  city?: string;
  phone?: string;
  email?: string;
  website?: string;

  // Tọa độ
  latitude?: number;
  longitude?: number;

  // Hình ảnh
  bannerUrl?: string;

  // Thông tin phụ
  totalScreens?: number;
  facilities?: string[];

  // Trạng thái
  isActive?: boolean;
}

/**
 * DTO cho MovieResponseDto (phim từ backend)
 */
export interface MovieResponseDto {
  id: string;
  title: string;
  slug?: string;
  durationMinutes: number;
  description?: string;
  ageLimit: number;
  posterUrl?: string;
  trailerUrl?: string;
  releaseDate?: string;
  category: string;
  status: string;
  createdAt: string;
}

/**
 * DTO cho ShowtimeResponseDto (lịch chiếu từ backend)
 */
export interface ShowtimeResponseDto {
  id: string;
  movieId: string;
  screenId: string;
  startTime: string;
  endTime: string;
  basePrice: number;
  isActive: boolean;
}

/**
 * DTO cho phim kèm lịch chiếu (MovieWithShowtimesDto)
 */
export interface MovieWithShowtimesDto {
  movie: MovieResponseDto;
  showtimes: ShowtimeResponseDto[];
}

/**
 * DTO chi tiết rạp kèm phim đang chiếu (CinemaDetailResponseDto)
 */
export interface CinemaDetailResponseDto {
  id: string;
  name: string;
  slug: string;
  address?: string;
  city?: string;
  phone?: string;
  email?: string;
  website?: string;
  latitude?: number;
  longitude?: number;
  bannerUrl?: string;
  totalScreens: number;
  facilities?: string[];
  isActive: boolean;
  createdAt: string;
  updatedAt: string;

  // Danh sách phim đang chiếu tại rạp này
  currentlyShowingMovies: MovieResponseDto[];
}

/**
 * DTO rạp kèm phim và lịch chiếu (CinemaMoviesWithShowtimesResponseDto)
 */
export interface CinemaMoviesWithShowtimesResponseDto {
  cinemaId: string;
  cinemaName: string;
  slug?: string;
  address?: string;
  movies: MovieWithShowtimesDto[];
}

// ==================== API FUNCTIONS ====================

/**
 * Lấy danh sách tất cả rạp chiếu phim
 * GET /api/cinemas
 */
export const getAll = async (): Promise<CinemaResponseDto[]> => {
  const response = await axiosClient.get('/api/cinemas') as ApiResponseDto<CinemaResponseDto[]>;
  if (!response.success || !response.data) {
    throw new Error(response.message || 'Lỗi khi lấy danh sách rạp chiếu phim');
  }
  return response.data;
};

/**
 * Lấy danh sách rạp chiếu phim đang hoạt động
 * GET /api/cinemas/active
 */
export const getActive = async (): Promise<CinemaResponseDto[]> => {
  const response = await axiosClient.get('/api/cinemas/active') as ApiResponseDto<CinemaResponseDto[]>;
  if (!response.success || !response.data) {
    throw new Error(response.message || 'Lỗi khi lấy danh sách rạp đang hoạt động');
  }
  return response.data;
};

/**
 * Lấy danh sách rạp chiếu phim theo thành phố
 * GET /api/cinemas/city/{city}
 */
export const getByCity = async (city: string): Promise<CinemaResponseDto[]> => {
  const response = await axiosClient.get(`/api/cinemas/city/${encodeURIComponent(city)}`) as ApiResponseDto<CinemaResponseDto[]>;
  if (!response.success || !response.data) {
    throw new Error(response.message || `Lỗi khi lấy danh sách rạp tại ${city}`);
  }
  return response.data;
};

/**
 * Lấy thông tin rạp chiếu phim theo ID
 * GET /api/cinemas/{id}
 */
export const getById = async (id: string): Promise<CinemaResponseDto> => {
  const response = await axiosClient.get(`/api/cinemas/${id}`) as ApiResponseDto<CinemaResponseDto>;
  if (!response.success || !response.data) {
    throw new Error(response.message || `Không tìm thấy rạp có ID ${id}`);
  }
  return response.data;
};

/**
 * Tạo rạp chiếu phim mới
 * POST /api/cinemas
 */
export const create = async (dto: CinemaCreateDto): Promise<CinemaResponseDto> => {
  const response = await axiosClient.post('/api/cinemas', dto) as ApiResponseDto<CinemaResponseDto>;
  if (!response.success || !response.data) {
    throw new Error(response.message || 'Lỗi khi tạo rạp chiếu phim');
  }
  return response.data;
};

/**
 * Cập nhật thông tin rạp chiếu phim
 * PUT /api/cinemas/{id}
 */
export const update = async (id: string, dto: CinemaUpdateDto): Promise<CinemaResponseDto> => {
  const response = await axiosClient.put(`/api/cinemas/${id}`, dto) as ApiResponseDto<CinemaResponseDto>;
  if (!response.success || !response.data) {
    throw new Error(response.message || `Không tìm thấy rạp có ID ${id}`);
  }
  return response.data;
};

/**
 * Xóa rạp chiếu phim
 * DELETE /api/cinemas/{id}
 */
export const deleteCinema = async (id: string): Promise<void> => {
  const response = await axiosClient.delete(`/api/cinemas/${id}`) as ApiResponseDto<unknown>;

  // Backend trả về NoContent (204) cho delete thành công
  // Axios sẽ không có response.data cho 204
  if (response && typeof response === 'object' && 'success' in response) {
    const errorResponse = response as ApiResponseDto<unknown>;
    if (!errorResponse.success) {
      throw new Error(errorResponse.message || `Không tìm thấy rạp có ID ${id}`);
    }
  }
};

/**
 * Lấy danh sách phim đang chiếu tại một rạp cụ thể
 * GET /api/cinemas/{cinemaId}/movies
 */
export const getCurrentlyShowingMovies = async (cinemaId: string): Promise<MovieResponseDto[]> => {
  const response = await axiosClient.get(`/api/cinemas/${cinemaId}/movies`) as ApiResponseDto<MovieResponseDto[]>;
  if (!response.success || !response.data) {
    throw new Error(response.message || 'Lỗi khi lấy danh sách phim đang chiếu');
  }
  return response.data;
};

/**
 * Lấy thông tin chi tiết rạp kèm danh sách phim đang chiếu
 * GET /api/cinemas/{cinemaId}/detail
 */
export const getCinemaDetail = async (cinemaId: string): Promise<CinemaDetailResponseDto> => {
  const response = await axiosClient.get(`/api/cinemas/${cinemaId}/detail`) as ApiResponseDto<CinemaDetailResponseDto>;
  if (!response.success || !response.data) {
    throw new Error(response.message || 'Lỗi khi lấy thông tin chi tiết rạp');
  }
  return response.data;
};

/**
 * Lấy danh sách tất cả rạp kèm phim đang chiếu
 * GET /api/cinemas/with-movies
 */
export const getAllWithMovies = async (): Promise<CinemaDetailResponseDto[]> => {
  const response = await axiosClient.get('/api/cinemas/with-movies') as ApiResponseDto<CinemaDetailResponseDto[]>;
  if (!response.success || !response.data) {
    throw new Error(response.message || 'Lỗi khi lấy danh sách rạp và phim đang chiếu');
  }
  return response.data;
};

/**
 * Lấy tất cả phim kèm lịch chiếu tại rạp (từ giờ trở đi)
 * GET /api/cinemas/{cinemaId}/movies-with-showtimes
 */
export const getMoviesWithShowtimes = async (cinemaId: string): Promise<CinemaMoviesWithShowtimesResponseDto> => {
  const response = await axiosClient.get(`/api/cinemas/${cinemaId}/movies-with-showtimes`) as ApiResponseDto<CinemaMoviesWithShowtimesResponseDto>;
  if (!response.success || !response.data) {
    throw new Error(response.message || 'Lỗi khi lấy lịch chiếu');
  }
  return response.data;
};

/**
 * Lấy phim kèm lịch chiếu tại rạp theo ngày cụ thể
 * GET /api/cinemas/{cinemaId}/movies-with-showtimes/by-date?date={date}
 */
export const getMoviesWithShowtimesByDate = async (cinemaId: string, date: Date): Promise<CinemaMoviesWithShowtimesResponseDto> => {
  const dateString = date.toISOString();
  const response = await axiosClient.get(`/api/cinemas/${cinemaId}/movies-with-showtimes/by-date`, {
    params: { date: dateString }
  }) as ApiResponseDto<CinemaMoviesWithShowtimesResponseDto>;
  if (!response.success || !response.data) {
    throw new Error(response.message || 'Lỗi khi lấy lịch chiếu theo ngày');
  }
  return response.data;
};

// ==================== EXPORT OBJECT ====================

/**
 * Object chứa tất cả các API functions cho Cinema
 * Sử dụng pattern này để dễ import: import { cinemaApi } from '@/services/apiCinema'
 */
export const cinemaApi = {
  // CRUD cơ bản
  getAll,
  getActive,
  getByCity,
  getById,
  create,
  update,
  delete: deleteCinema,

  // Phim tại rạp
  getCurrentlyShowingMovies,
  getCinemaDetail,
  getAllWithMovies,

  // Phim kèm lịch chiếu
  getMoviesWithShowtimes,
  getMoviesWithShowtimesByDate,
};
