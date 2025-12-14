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
  name?: string;
  surchargeRate: number;
}

/**
 * DTO cho cập nhật loại ghế
 */
export interface SeatTypeUpdateDto {
  name?: string;
  surchargeRate?: number;
}

/**
 * Response DTO cho loại ghế
 */
export interface SeatTypeResponseDto {
  code: string;
  name?: string;
  surchargeRate: number;
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
  const response = await axiosClient.delete(`/api/seattypes/${code}`);
  
  // 204 NoContent không có response body, response sẽ là null/undefined/empty khi thành công
  // Nếu có response body thì đó là trường hợp lỗi (404, 500, etc.)
  if (response && typeof response === 'object' && 'success' in response) {
    const errorResponse = response as ApiResponseDto<unknown>;
    if (!errorResponse.success) {
      throw new Error(errorResponse.message || `Không tìm thấy loại ghế có mã ${code}`);
    }
  }
  // Nếu response rỗng/null/undefined thì đó là 204 NoContent - thành công, không cần làm gì
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

