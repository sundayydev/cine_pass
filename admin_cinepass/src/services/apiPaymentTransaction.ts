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
 * DTO cho tạo giao dịch thanh toán mới
 */
export interface PaymentTransactionCreateDto {
  orderId: string;
  amount: number;
  paymentMethod: string;
  providerTransId?: string;
  status?: string;
}

/**
 * Response DTO cho giao dịch thanh toán
 */
export interface PaymentTransactionResponseDto {
  id: string;
  orderId: string;
  amount: number;
  paymentMethod: string;
  providerTransId?: string;
  status: string;
  createdAt: string;
  updatedAt: string;
}

// ==================== API FUNCTIONS ====================

/**
 * Lấy thông tin giao dịch thanh toán theo ID
 * GET /api/paymenttransactions/{id}
 */
export const getById = async (id: string): Promise<PaymentTransactionResponseDto> => {
  const response = (await axiosClient.get(
    `/api/paymenttransactions/${id}`
  )) as ApiResponseDto<PaymentTransactionResponseDto>;

  if (!response.success || !response.data) {
    throw new Error(response.message || `Không tìm thấy giao dịch thanh toán có ID ${id}`);
  }

  return response.data;
};

/**
 * Lấy danh sách giao dịch thanh toán theo đơn hàng
 * GET /api/paymenttransactions/order/{orderId}
 */
export const getByOrderId = async (orderId: string): Promise<PaymentTransactionResponseDto[]> => {
  const response = (await axiosClient.get(
    `/api/paymenttransactions/order/${orderId}`
  )) as ApiResponseDto<PaymentTransactionResponseDto[]>;

  if (!response.success || !response.data) {
    throw new Error(response.message || 'Lỗi khi lấy danh sách giao dịch thanh toán');
  }

  return response.data;
};

/**
 * Lấy thông tin giao dịch thanh toán theo mã giao dịch của nhà cung cấp
 * GET /api/paymenttransactions/provider/{providerTransId}
 */
export const getByProviderTransId = async (providerTransId: string): Promise<PaymentTransactionResponseDto> => {
  const response = (await axiosClient.get(
    `/api/paymenttransactions/provider/${providerTransId}`
  )) as ApiResponseDto<PaymentTransactionResponseDto>;

  if (!response.success || !response.data) {
    throw new Error(response.message || `Không tìm thấy giao dịch thanh toán với mã ${providerTransId}`);
  }

  return response.data;
};

/**
 * Lấy danh sách giao dịch thanh toán thành công
 * GET /api/paymenttransactions/successful
 */
export const getSuccessful = async (): Promise<PaymentTransactionResponseDto[]> => {
  const response = (await axiosClient.get(
    '/api/paymenttransactions/successful'
  )) as ApiResponseDto<PaymentTransactionResponseDto[]>;

  if (!response.success || !response.data) {
    throw new Error(response.message || 'Lỗi khi lấy danh sách giao dịch thanh toán');
  }

  return response.data;
};

/**
 * Lấy danh sách giao dịch thanh toán thất bại
 * GET /api/paymenttransactions/failed
 */
export const getFailed = async (): Promise<PaymentTransactionResponseDto[]> => {
  const response = (await axiosClient.get(
    '/api/paymenttransactions/failed'
  )) as ApiResponseDto<PaymentTransactionResponseDto[]>;

  if (!response.success || !response.data) {
    throw new Error(response.message || 'Lỗi khi lấy danh sách giao dịch thanh toán');
  }

  return response.data;
};

/**
 * Tạo giao dịch thanh toán mới
 * POST /api/paymenttransactions
 */
export const create = async (dto: PaymentTransactionCreateDto): Promise<PaymentTransactionResponseDto> => {
  const response = (await axiosClient.post(
    '/api/paymenttransactions',
    dto
  )) as ApiResponseDto<PaymentTransactionResponseDto>;

  if (!response.success || !response.data) {
    throw new Error(response.message || 'Lỗi khi tạo giao dịch thanh toán');
  }

  return response.data;
};

/**
 * Cập nhật trạng thái giao dịch thanh toán
 * PUT /api/paymenttransactions/{id}/status
 */
export const updateStatus = async (id: string, status: string): Promise<PaymentTransactionResponseDto> => {
  // ASP.NET Core [FromBody] string expects JSON string, so we need to send it as a JSON string
  const response = (await axiosClient.put(
    `/api/paymenttransactions/${id}/status`,
    JSON.stringify(status),
    {
      headers: {
        'Content-Type': 'application/json',
      },
    }
  )) as ApiResponseDto<PaymentTransactionResponseDto>;

  if (!response.success || !response.data) {
    throw new Error(response.message || `Không tìm thấy giao dịch thanh toán có ID ${id}`);
  }

  return response.data;
};

// ==================== EXPORT OBJECT ====================

/**
 * Object chứa tất cả các API functions cho PaymentTransaction
 */
export const paymentTransactionApi = {
  getById,
  getByOrderId,
  getByProviderTransId,
  getSuccessful,
  getFailed,
  create,
  updateStatus,
};

