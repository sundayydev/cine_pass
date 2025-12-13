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
 * DTO cho tạo loại ghế mới
 */
export interface SeatTypeCreateDto {
  code: string;
  name: string;
  price: number;
  description?: string;
}

/**
 * DTO cho cập nhật loại ghế
 */
export interface SeatTypeUpdateDto {
  name?: string;
  price?: number;
  description?: string;
}

/**
 * Response DTO cho loại ghế
 */
export interface SeatTypeResponseDto {
  code: string;
  name: string;
  price: number;
  description?: string;
  createdAt: string;
  updatedAt: string;
}

// ==================== API FUNCTIONS ====================

/**
 * Lấy danh sách tất cả loại ghế
 * GET /api/seattypes
 */
export const getAll = async (): Promise<SeatTypeResponseDto[]> => {
  const response = (await axiosClient.get(
    '/api/seattypes'
  )) as ApiResponseDto<SeatTypeResponseDto[]>;

  if (!response.success || !response.data) {
    throw new Error(response.message || 'Lỗi khi lấy danh sách loại ghế');
  }

  return response.data;
};

/**
 * Lấy thông tin loại ghế theo mã
 * GET /api/seattypes/{code}
 */
export const getByCode = async (code: string): Promise<SeatTypeResponseDto> => {
  const response = (await axiosClient.get(
    `/api/seattypes/${code}`
  )) as ApiResponseDto<SeatTypeResponseDto>;

  if (!response.success || !response.data) {
    throw new Error(response.message || `Không tìm thấy loại ghế có mã ${code}`);
  }

  return response.data;
};

/**
 * Tạo loại ghế mới
 * POST /api/seattypes
 */
export const create = async (dto: SeatTypeCreateDto): Promise<SeatTypeResponseDto> => {
  const response = (await axiosClient.post(
    '/api/seattypes',
    dto
  )) as ApiResponseDto<SeatTypeResponseDto>;

  if (!response.success || !response.data) {
    throw new Error(response.message || 'Lỗi khi tạo loại ghế');
  }

  return response.data;
};

/**
 * Cập nhật thông tin loại ghế
 * PUT /api/seattypes/{code}
 */
export const update = async (code: string, dto: SeatTypeUpdateDto): Promise<SeatTypeResponseDto> => {
  const response = (await axiosClient.put(
    `/api/seattypes/${code}`,
    dto
  )) as ApiResponseDto<SeatTypeResponseDto>;

  if (!response.success || !response.data) {
    throw new Error(response.message || `Không tìm thấy loại ghế có mã ${code}`);
  }

  return response.data;
};

/**
 * Xóa loại ghế
 * DELETE /api/seattypes/{code}
 */
export const deleteSeatType = async (code: string): Promise<void> => {
  const response = (await axiosClient.delete(
    `/api/seattypes/${code}`
  )) as ApiResponseDto<unknown>;

  if (!response.success) {
    throw new Error(response.message || `Không tìm thấy loại ghế có mã ${code}`);
  }
};

// ==================== EXPORT OBJECT ====================

/**
 * Object chứa tất cả các API functions cho SeatType
 */
export const seatTypeApi = {
  getAll,
  getByCode,
  create,
  update,
  delete: deleteSeatType,
};

