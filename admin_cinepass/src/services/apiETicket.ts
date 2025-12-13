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
 * Response DTO cho vé điện tử
 */
export interface ETicketResponseDto {
  id: string;
  orderTicketId: string;
  ticketCode: string;
  isUsed: boolean;
  usedAt?: string;
  createdAt: string;
  updatedAt: string;
}

/**
 * Response DTO cho chi tiết vé điện tử
 */
export interface ETicketDetailDto extends ETicketResponseDto {
  // Additional detail fields from backend
  [key: string]: any;
}

// ==================== API FUNCTIONS ====================

/**
 * Lấy thông tin vé điện tử theo ID
 * GET /api/etickets/{id}
 */
export const getById = async (id: string): Promise<ETicketResponseDto> => {
  const response = (await axiosClient.get(
    `/api/etickets/${id}`
  )) as ApiResponseDto<ETicketResponseDto>;

  if (!response.success || !response.data) {
    throw new Error(response.message || `Không tìm thấy vé điện tử có ID ${id}`);
  }

  return response.data;
};

/**
 * Lấy thông tin vé điện tử theo mã vé
 * GET /api/etickets/code/{ticketCode}
 */
export const getByTicketCode = async (ticketCode: string): Promise<ETicketResponseDto> => {
  const response = (await axiosClient.get(
    `/api/etickets/code/${ticketCode}`
  )) as ApiResponseDto<ETicketResponseDto>;

  if (!response.success || !response.data) {
    throw new Error(response.message || `Không tìm thấy vé điện tử với mã ${ticketCode}`);
  }

  return response.data;
};

/**
 * Lấy chi tiết vé điện tử theo mã vé
 * GET /api/etickets/code/{ticketCode}/detail
 */
export const getDetailByTicketCode = async (ticketCode: string): Promise<ETicketDetailDto> => {
  const response = (await axiosClient.get(
    `/api/etickets/code/${ticketCode}/detail`
  )) as ApiResponseDto<ETicketDetailDto>;

  if (!response.success || !response.data) {
    throw new Error(response.message || `Không tìm thấy vé điện tử với mã ${ticketCode}`);
  }

  return response.data;
};

/**
 * Lấy danh sách vé điện tử theo order ticket ID
 * GET /api/etickets/order-ticket/{orderTicketId}
 */
export const getByOrderTicketId = async (orderTicketId: string): Promise<ETicketResponseDto[]> => {
  const response = (await axiosClient.get(
    `/api/etickets/order-ticket/${orderTicketId}`
  )) as ApiResponseDto<ETicketResponseDto[]>;

  if (!response.success || !response.data) {
    throw new Error(response.message || 'Lỗi khi lấy danh sách vé điện tử');
  }

  return response.data;
};

/**
 * Tạo vé điện tử (sau khi thanh toán thành công)
 * POST /api/etickets/generate/{orderTicketId}
 */
export const generateETicket = async (orderTicketId: string): Promise<ETicketResponseDto> => {
  const response = (await axiosClient.post(
    `/api/etickets/generate/${orderTicketId}`
  )) as ApiResponseDto<ETicketResponseDto>;

  if (!response.success || !response.data) {
    throw new Error(response.message || 'Lỗi khi tạo vé điện tử');
  }

  return response.data;
};

/**
 * Xác thực vé điện tử
 * GET /api/etickets/validate/{ticketCode}
 */
export const validateTicket = async (ticketCode: string): Promise<boolean> => {
  const response = (await axiosClient.get(
    `/api/etickets/validate/${ticketCode}`
  )) as ApiResponseDto<boolean>;

  if (!response.success || response.data === undefined) {
    throw new Error(response.message || 'Lỗi khi xác thực vé');
  }

  return response.data;
};

/**
 * Sử dụng vé (check-in)
 * POST /api/etickets/use/{ticketCode}
 */
export const useTicket = async (ticketCode: string): Promise<void> => {
  const response = (await axiosClient.post(
    `/api/etickets/use/${ticketCode}`
  )) as ApiResponseDto<unknown>;

  if (!response.success) {
    throw new Error(response.message || `Không tìm thấy vé điện tử với mã ${ticketCode}`);
  }
};

// ==================== EXPORT OBJECT ====================

/**
 * Object chứa tất cả các API functions cho ETicket
 */
export const eTicketApi = {
  getById,
  getByTicketCode,
  getDetailByTicketCode,
  getByOrderTicketId,
  generateETicket,
  validateTicket,
  useTicket,
};

