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
 * DTO cho tạo ghế mới
 */
export interface SeatCreateDto {
  screenId: string;
  seatRow: string;
  seatNumber: number;
  seatCode: string;
  seatTypeCode?: string;
  isActive?: boolean;
}

/**
 * DTO cho tạo ghế tự động
 */
export interface SeatGenerateDto {
  screenId: string;
  rows: number;
  seatsPerRow: number;
  defaultSeatTypeCode?: string;
}

/**
 * DTO cho cập nhật ghế
 */
export interface SeatUpdateDto {
  seatTypeCode?: string;
  isActive?: boolean;
}

/**
 * Response DTO cho ghế
 */
export interface SeatResponseDto {
  id: string;
  screenId: string;
  seatRow: string;
  seatNumber: number;
  seatCode: string;
  seatTypeCode?: string;
  isActive: boolean;
}

// ==================== API FUNCTIONS ====================

/**
 * Lấy danh sách ghế theo màn hình
 * GET /api/seats/screen/{screenId}
 */
export const getByScreenId = async (screenId: string): Promise<SeatResponseDto[]> => {
  const response = (await axiosClient.get(
    `/api/seats/screen/${screenId}`
  )) as ApiResponseDto<SeatResponseDto[]>;

  if (!response.success || !response.data) {
    throw new Error(response.message || 'Lỗi khi lấy danh sách ghế');
  }

  return response.data;
};

/**
 * Lấy danh sách ghế đang hoạt động theo màn hình
 * GET /api/seats/screen/{screenId}/active
 */
export const getActiveByScreenId = async (screenId: string): Promise<SeatResponseDto[]> => {
  const response = (await axiosClient.get(
    `/api/seats/screen/${screenId}/active`
  )) as ApiResponseDto<SeatResponseDto[]>;

  if (!response.success || !response.data) {
    throw new Error(response.message || 'Lỗi khi lấy danh sách ghế');
  }

  return response.data;
};

/**
 * Lấy thông tin ghế theo ID
 * GET /api/seats/{id}
 */
export const getById = async (id: string): Promise<SeatResponseDto> => {
  const response = (await axiosClient.get(
    `/api/seats/${id}`
  )) as ApiResponseDto<SeatResponseDto>;

  if (!response.success || !response.data) {
    throw new Error(response.message || `Không tìm thấy ghế có ID ${id}`);
  }

  return response.data;
};

/**
 * Kiểm tra ghế có còn trống không
 * GET /api/seats/{seatId}/available?showtimeId={showtimeId}
 */
export const isSeatAvailable = async (seatId: string, showtimeId: string): Promise<boolean> => {
  const response = (await axiosClient.get(
    `/api/seats/${seatId}/available`,
    {
      params: { showtimeId },
    }
  )) as ApiResponseDto<boolean>;

  if (!response.success || response.data === undefined) {
    throw new Error(response.message || 'Lỗi khi kiểm tra ghế');
  }

  return response.data;
};

/**
 * Tạo ghế mới
 * POST /api/seats
 */
export const create = async (dto: SeatCreateDto): Promise<SeatResponseDto> => {
  const response = (await axiosClient.post(
    '/api/seats',
    dto
  )) as ApiResponseDto<SeatResponseDto>;

  if (!response.success || !response.data) {
    throw new Error(response.message || 'Lỗi khi tạo ghế');
  }

  return response.data;
};

/**
 * Tự động tạo ghế theo cấu hình
 * POST /api/seats/generate
 */
export const generateSeats = async (dto: SeatGenerateDto): Promise<SeatResponseDto[]> => {
  const response = (await axiosClient.post(
    '/api/seats/generate',
    dto
  )) as ApiResponseDto<SeatResponseDto[]>;

  if (!response.success || !response.data) {
    throw new Error(response.message || 'Lỗi khi tạo ghế tự động');
  }

  return response.data;
};

/**
 * Cập nhật thông tin ghế
 * PUT /api/seats/{id}
 */
export const update = async (id: string, dto: SeatUpdateDto): Promise<SeatResponseDto> => {
  const response = (await axiosClient.put(
    `/api/seats/${id}`,
    dto
  )) as ApiResponseDto<SeatResponseDto>;

  if (!response.success || !response.data) {
    throw new Error(response.message || `Không tìm thấy ghế có ID ${id}`);
  }

  return response.data;
};

/**
 * Xóa ghế
 * DELETE /api/seats/{id}
 */
export const deleteSeat = async (id: string): Promise<void> => {
  const response = (await axiosClient.delete(
    `/api/seats/${id}`
  )) as ApiResponseDto<unknown>;

  if (!response.success) {
    throw new Error(response.message || `Không tìm thấy ghế có ID ${id}`);
  }
};

// ==================== EXPORT OBJECT ====================

/**
 * Object chứa tất cả các API functions cho Seat
 */
export const seatApi = {
  getByScreenId,
  getActiveByScreenId,
  getById,
  isSeatAvailable,
  create,
  generateSeats,
  update,
  delete: deleteSeat,
};

