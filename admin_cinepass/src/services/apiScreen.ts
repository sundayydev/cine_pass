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
 * DTO cho tạo màn hình mới
 */
export interface ScreenCreateDto {
  cinemaId: string;
  name: string;
  capacity: number;
  description?: string;
}

/**
 * DTO cho cập nhật màn hình
 */
export interface ScreenUpdateDto {
  cinemaId?: string;
  name?: string;
  capacity?: number;
  description?: string;
}

/**
 * Response DTO cho màn hình
 */
export interface ScreenResponseDto {
  id: string;
  cinemaId: string;
  name: string;
  capacity: number;
  description?: string;
  createdAt: string;
  updatedAt: string;
}

// ==================== API FUNCTIONS ====================

/**
 * Lấy danh sách màn hình theo rạp chiếu phim
 * GET /api/screens/cinema/{cinemaId}
 */
export const getByCinemaId = async (cinemaId: string): Promise<ScreenResponseDto[]> => {
  const response = (await axiosClient.get(
    `/api/screens/cinema/${cinemaId}`
  )) as ApiResponseDto<ScreenResponseDto[]>;

  if (!response.success || !response.data) {
    throw new Error(response.message || 'Lỗi khi lấy danh sách màn hình');
  }

  return response.data;
};

/**
 * Lấy thông tin màn hình theo ID
 * GET /api/screens/{id}
 */
export const getById = async (id: string): Promise<ScreenResponseDto> => {
  const response = (await axiosClient.get(
    `/api/screens/${id}`
  )) as ApiResponseDto<ScreenResponseDto>;

  if (!response.success || !response.data) {
    throw new Error(response.message || `Không tìm thấy màn hình có ID ${id}`);
  }

  return response.data;
};

/**
 * Tạo màn hình mới
 * POST /api/screens
 */
export const create = async (dto: ScreenCreateDto): Promise<ScreenResponseDto> => {
  const response = (await axiosClient.post(
    '/api/screens',
    dto
  )) as ApiResponseDto<ScreenResponseDto>;

  if (!response.success || !response.data) {
    throw new Error(response.message || 'Lỗi khi tạo màn hình');
  }

  return response.data;
};

/**
 * Cập nhật thông tin màn hình
 * PUT /api/screens/{id}
 */
export const update = async (id: string, dto: ScreenUpdateDto): Promise<ScreenResponseDto> => {
  const response = (await axiosClient.put(
    `/api/screens/${id}`,
    dto
  )) as ApiResponseDto<ScreenResponseDto>;

  if (!response.success || !response.data) {
    throw new Error(response.message || `Không tìm thấy màn hình có ID ${id}`);
  }

  return response.data;
};

/**
 * Xóa màn hình
 * DELETE /api/screens/{id}
 */
export const deleteScreen = async (id: string): Promise<void> => {
  const response = (await axiosClient.delete(
    `/api/screens/${id}`
  )) as ApiResponseDto<unknown>;

  if (!response.success) {
    throw new Error(response.message || `Không tìm thấy màn hình có ID ${id}`);
  }
};

// ==================== EXPORT OBJECT ====================

/**
 * Object chứa tất cả các API functions cho Screen
 */
export const screenApi = {
  getByCinemaId,
  getById,
  create,
  update,
  delete: deleteScreen,
};

