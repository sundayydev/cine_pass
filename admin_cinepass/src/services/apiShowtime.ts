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
 * DTO cho tạo lịch chiếu mới
 */
export interface ShowtimeCreateDto {
  movieId: string;
  screenId: string;
  startTime: string; // ISO date string
  price: number;
}

/**
 * DTO cho cập nhật lịch chiếu
 */
export interface ShowtimeUpdateDto {
  movieId?: string;
  screenId?: string;
  startTime?: string; // ISO date string
  price?: number;
}

/**
 * Response DTO cho lịch chiếu
 */
export interface ShowtimeResponseDto {
  id: string;
  movieId: string;
  screenId: string;
  startTime: string;
  price: number;
  createdAt: string;
  updatedAt: string;
}

// ==================== API FUNCTIONS ====================

/**
 * Lấy danh sách lịch chiếu theo phim
 * GET /api/showtimes/movie/{movieId}
 */
export const getByMovieId = async (movieId: string): Promise<ShowtimeResponseDto[]> => {
  const response = (await axiosClient.get(
    `/api/showtimes/movie/${movieId}`
  )) as ApiResponseDto<ShowtimeResponseDto[]>;

  if (!response.success || !response.data) {
    throw new Error(response.message || 'Lỗi khi lấy danh sách lịch chiếu');
  }

  return response.data;
};

/**
 * Lấy danh sách lịch chiếu theo ngày
 * GET /api/showtimes/date/{date}
 */
export const getByDate = async (date: string): Promise<ShowtimeResponseDto[]> => {
  const response = (await axiosClient.get(
    `/api/showtimes/date/${date}`
  )) as ApiResponseDto<ShowtimeResponseDto[]>;

  if (!response.success || !response.data) {
    throw new Error(response.message || 'Lỗi khi lấy danh sách lịch chiếu');
  }

  return response.data;
};

/**
 * Lấy danh sách lịch chiếu theo phim và ngày
 * GET /api/showtimes/movie/{movieId}/date/{date}
 */
export const getByMovieAndDate = async (movieId: string, date: string): Promise<ShowtimeResponseDto[]> => {
  const response = (await axiosClient.get(
    `/api/showtimes/movie/${movieId}/date/${date}`
  )) as ApiResponseDto<ShowtimeResponseDto[]>;

  if (!response.success || !response.data) {
    throw new Error(response.message || 'Lỗi khi lấy danh sách lịch chiếu');
  }

  return response.data;
};

/**
 * Lấy thông tin lịch chiếu theo ID
 * GET /api/showtimes/{id}
 */
export const getById = async (id: string): Promise<ShowtimeResponseDto> => {
  const response = (await axiosClient.get(
    `/api/showtimes/${id}`
  )) as ApiResponseDto<ShowtimeResponseDto>;

  if (!response.success || !response.data) {
    throw new Error(response.message || `Không tìm thấy lịch chiếu có ID ${id}`);
  }

  return response.data;
};

/**
 * Tạo lịch chiếu mới
 * POST /api/showtimes
 */
export const create = async (dto: ShowtimeCreateDto): Promise<ShowtimeResponseDto> => {
  const response = (await axiosClient.post(
    '/api/showtimes',
    dto
  )) as ApiResponseDto<ShowtimeResponseDto>;

  if (!response.success || !response.data) {
    throw new Error(response.message || 'Lỗi khi tạo lịch chiếu');
  }

  return response.data;
};

/**
 * Cập nhật thông tin lịch chiếu
 * PUT /api/showtimes/{id}
 */
export const update = async (id: string, dto: ShowtimeUpdateDto): Promise<ShowtimeResponseDto> => {
  const response = (await axiosClient.put(
    `/api/showtimes/${id}`,
    dto
  )) as ApiResponseDto<ShowtimeResponseDto>;

  if (!response.success || !response.data) {
    throw new Error(response.message || `Không tìm thấy lịch chiếu có ID ${id}`);
  }

  return response.data;
};

/**
 * Xóa lịch chiếu
 * DELETE /api/showtimes/{id}
 */
export const deleteShowtime = async (id: string): Promise<void> => {
  const response = (await axiosClient.delete(
    `/api/showtimes/${id}`
  )) as ApiResponseDto<unknown>;

  if (!response.success) {
    throw new Error(response.message || `Không tìm thấy lịch chiếu có ID ${id}`);
  }
};

// ==================== EXPORT OBJECT ====================

/**
 * Object chứa tất cả các API functions cho Showtime
 */
export const showtimeApi = {
  getByMovieId,
  getByDate,
  getByMovieAndDate,
  getById,
  create,
  update,
  delete: deleteShowtime,
};

