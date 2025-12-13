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
 * DTO cho tạo rạp chiếu phim mới
 */
export interface CinemaCreateDto {
  name: string;
  slug?: string;
  description?: string;
  address?: string;
  city?: string;
  phone?: string;
  email?: string;
  website?: string;
  latitude?: number;
  longitude?: number;
  bannerUrl?: string;
  totalScreens?: number;
  facilities?: string[];
  isActive?: boolean;
}

/**
 * DTO cho cập nhật rạp chiếu phim
 */
export interface CinemaUpdateDto {
  name?: string;
  slug?: string;
  description?: string;
  address?: string;
  city?: string;
  phone?: string;
  email?: string;
  website?: string;
  latitude?: number;
  longitude?: number;
  bannerUrl?: string;
  totalScreens?: number;
  facilities?: string[];
  isActive?: boolean;
}

/**
 * Response DTO cho rạp chiếu phim
 */
export interface CinemaResponseDto {
  id: string;
  name: string;
  slug: string;
  description?: string;
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
  createdAt?: string;
  updatedAt?: string;
}

// ==================== API FUNCTIONS ====================

/**
 * Lấy danh sách tất cả rạp chiếu phim
 * GET /api/cinemas
 */
export const getAll = async (): Promise<CinemaResponseDto[]> => {
  const response = (await axiosClient.get(
    '/api/cinemas'
  )) as ApiResponseDto<CinemaResponseDto[]>;

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
  const response = (await axiosClient.get(
    '/api/cinemas/active'
  )) as ApiResponseDto<CinemaResponseDto[]>;

  if (!response.success || !response.data) {
    throw new Error(response.message || 'Lỗi khi lấy danh sách rạp chiếu phim');
  }

  return response.data;
};

/**
 * Lấy danh sách rạp chiếu phim theo thành phố
 * GET /api/cinemas/city/{city}
 */
export const getByCity = async (city: string): Promise<CinemaResponseDto[]> => {
  const response = (await axiosClient.get(
    `/api/cinemas/city/${city}`
  )) as ApiResponseDto<CinemaResponseDto[]>;

  if (!response.success || !response.data) {
    throw new Error(response.message || 'Lỗi khi lấy danh sách rạp chiếu phim');
  }

  return response.data;
};

/**
 * Lấy thông tin rạp chiếu phim theo ID
 * GET /api/cinemas/{id}
 */
export const getById = async (id: string): Promise<CinemaResponseDto> => {
  const response = (await axiosClient.get(
    `/api/cinemas/${id}`
  )) as ApiResponseDto<CinemaResponseDto>;

  if (!response.success || !response.data) {
    throw new Error(response.message || `Không tìm thấy rạp chiếu phim có ID ${id}`);
  }

  return response.data;
};

/**
 * Tạo rạp chiếu phim mới
 * POST /api/cinemas
 */
export const create = async (dto: CinemaCreateDto): Promise<CinemaResponseDto> => {
  const response = (await axiosClient.post(
    '/api/cinemas',
    dto
  )) as ApiResponseDto<CinemaResponseDto>;

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
  const response = (await axiosClient.put(
    `/api/cinemas/${id}`,
    dto
  )) as ApiResponseDto<CinemaResponseDto>;

  if (!response.success || !response.data) {
    throw new Error(response.message || `Không tìm thấy rạp chiếu phim có ID ${id}`);
  }

  return response.data;
};

/**
 * Xóa rạp chiếu phim
 * DELETE /api/cinemas/{id}
 */
export const deleteCinema = async (id: string): Promise<void> => {
  const response = (await axiosClient.delete(
    `/api/cinemas/${id}`
  )) as ApiResponseDto<unknown>;

  if (!response.success) {
    throw new Error(response.message || `Không tìm thấy rạp chiếu phim có ID ${id}`);
  }
};

// ==================== EXPORT OBJECT ====================

/**
 * Object chứa tất cả các API functions cho Cinema
 */
export const cinemaApi = {
  getAll,
  getActive,
  getByCity,
  getById,
  create,
  update,
  delete: deleteCinema,
};

