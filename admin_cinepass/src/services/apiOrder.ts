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
 * DTO cho tạo đơn hàng mới
 */
export interface OrderCreateDto {
  userId: string;
  orderTickets: OrderTicketCreateDto[];
  orderProducts?: OrderProductCreateDto[];
}

/**
 * DTO cho tạo order ticket
 */
export interface OrderTicketCreateDto {
  showtimeId: string;
  seatId: string;
  price: number;
}

/**
 * DTO cho tạo order product
 */
export interface OrderProductCreateDto {
  productId: string;
  quantity: number;
  price: number;
}

/**
 * DTO cho cập nhật đơn hàng
 */
export interface OrderUpdateDto {
  status?: number; // OrderStatus enum
}

/**
 * Response DTO cho đơn hàng
 */
export interface OrderResponseDto {
  id: string;
  userId: string | null;
  totalAmount: number;
  status: string; // OrderStatus string from backend
  paymentMethod?: string;
  expireAt: string | null;
  createdAt: string;
}

/**
 * User info
 */
export interface UserInfo {
  id: string;
  email: string;
  phone?: string;
  fullName?: string;
  role: number;
  createdAt: string;
  updatedAt: string;
}

/**
 * Movie info
 */
export interface MovieInfo {
  id: string;
  title: string;
  slug: string;
  durationMinutes: number;
  description?: string;
  posterUrl?: string;
  trailerUrl?: string;
  releaseDate: string;
  status: string;
  createdAt: string;
}

/**
 * Screen info
 */
export interface ScreenInfo {
  id: string;
  cinemaId: string;
  name: string;
  totalSeats: number;
  seatMapLayout?: string;
}

/**
 * Showtime detail
 */
export interface ShowtimeDetail {
  id: string;
  startTime: string;
  endTime: string;
  basePrice: number;
  isActive: boolean;
  movie: MovieInfo;
  screen: ScreenInfo;
}

/**
 * Seat info
 */
export interface SeatInfo {
  id: string;
  screenId: string;
  seatRow: string;
  seatNumber: number;
  seatCode: string;
  seatTypeCode?: string;
  isActive: boolean;
}

/**
 * E-Ticket info
 */
export interface ETicketInfo {
  id: string;
  orderTicketId: string;
  ticketCode: string;
  qrData?: string;
  isUsed: boolean;
  usedAt?: string;
}

/**
 * Order Ticket Detail
 */
export interface OrderTicketDetail {
  id: string;
  showtimeId: string;
  seatId: string;
  price: number;
  showtime?: ShowtimeDetail;
  seat?: SeatInfo;
  eTicket?: ETicketInfo;
}

/**
 * Product info
 */
export interface ProductInfo {
  id: string;
  name: string;
  description?: string;
  price: number;
  imageUrl?: string;
  category: string;
  isActive: boolean;
}

/**
 * Order Product Detail
 */
export interface OrderProductDetail {
  id: string;
  productId: string;
  quantity: number;
  unitPrice: number;
  product?: ProductInfo;
}

/**
 * Response DTO cho chi tiết đơn hàng
 */
export interface OrderDetailDto extends OrderResponseDto {
  user?: UserInfo;
  tickets: OrderTicketDetail[];
  products: OrderProductDetail[];
}

// ==================== API FUNCTIONS ====================

/**
 * Lấy danh sách tất cả đơn hàng
 * GET /api/orders
 */
export const getAll = async (): Promise<OrderResponseDto[]> => {
  const response = (await axiosClient.get(
    '/api/orders'
  )) as ApiResponseDto<OrderResponseDto[]>;

  if (!response.success || !response.data) {
    throw new Error(response.message || 'Lỗi khi lấy danh sách đơn hàng');
  }

  return response.data;
};

/**
 * Lấy thông tin đơn hàng theo ID
 * GET /api/orders/{id}
 */
export const getById = async (id: string): Promise<OrderResponseDto> => {
  const response = (await axiosClient.get(
    `/api/orders/${id}`
  )) as ApiResponseDto<OrderResponseDto>;

  if (!response.success || !response.data) {
    throw new Error(response.message || `Không tìm thấy đơn hàng có ID ${id}`);
  }

  return response.data;
};

/**
 * Lấy chi tiết đơn hàng theo ID (bao gồm tickets và products)
 * GET /api/orders/{id}/detail
 */
export const getDetailById = async (id: string): Promise<OrderDetailDto> => {
  const response = (await axiosClient.get(
    `/api/orders/${id}/detail`
  )) as ApiResponseDto<OrderDetailDto>;

  if (!response.success || !response.data) {
    throw new Error(response.message || `Không tìm thấy đơn hàng có ID ${id}`);
  }

  return response.data;
};

/**
 * Lấy danh sách đơn hàng theo người dùng
 * GET /api/orders/user/{userId}
 */
export const getByUserId = async (userId: string): Promise<OrderResponseDto[]> => {
  const response = (await axiosClient.get(
    `/api/orders/user/${userId}`
  )) as ApiResponseDto<OrderResponseDto[]>;

  if (!response.success || !response.data) {
    throw new Error(response.message || 'Lỗi khi lấy danh sách đơn hàng');
  }

  return response.data;
};

/**
 * Lấy danh sách đơn hàng theo trạng thái
 * GET /api/orders/status/{status}
 */
export const getByStatus = async (status: number): Promise<OrderResponseDto[]> => {
  const response = (await axiosClient.get(
    `/api/orders/status/${status}`
  )) as ApiResponseDto<OrderResponseDto[]>;

  if (!response.success || !response.data) {
    throw new Error(response.message || 'Lỗi khi lấy danh sách đơn hàng');
  }

  return response.data;
};

/**
 * Lấy danh sách đơn hàng đã hết hạn
 * GET /api/orders/expired
 */
export const getExpired = async (): Promise<OrderResponseDto[]> => {
  const response = (await axiosClient.get(
    '/api/orders/expired'
  )) as ApiResponseDto<OrderResponseDto[]>;

  if (!response.success || !response.data) {
    throw new Error(response.message || 'Lỗi khi lấy danh sách đơn hàng hết hạn');
  }

  return response.data;
};

/**
 * Tạo đơn hàng mới
 * POST /api/orders
 */
export const create = async (dto: OrderCreateDto): Promise<OrderResponseDto> => {
  const response = (await axiosClient.post(
    '/api/orders',
    dto
  )) as ApiResponseDto<OrderResponseDto>;

  if (!response.success || !response.data) {
    throw new Error(response.message || 'Lỗi khi tạo đơn hàng');
  }

  return response.data;
};

/**
 * Cập nhật đơn hàng
 * PUT /api/orders/{id}
 */
export const update = async (id: string, dto: OrderUpdateDto): Promise<OrderResponseDto> => {
  const response = (await axiosClient.put(
    `/api/orders/${id}`,
    dto
  )) as ApiResponseDto<OrderResponseDto>;

  if (!response.success || !response.data) {
    throw new Error(response.message || `Không tìm thấy đơn hàng có ID ${id}`);
  }

  return response.data;
};

/**
 * Xác nhận đơn hàng (sau khi thanh toán thành công)
 * POST /api/orders/{id}/confirm
 */
export const confirmOrder = async (id: string): Promise<void> => {
  const response = (await axiosClient.post(
    `/api/orders/${id}/confirm`
  )) as ApiResponseDto<unknown>;

  if (!response.success) {
    throw new Error(response.message || `Không tìm thấy đơn hàng có ID ${id}`);
  }
};

/**
 * Hủy đơn hàng
 * POST /api/orders/{id}/cancel
 */
export const cancelOrder = async (id: string): Promise<void> => {
  const response = (await axiosClient.post(
    `/api/orders/${id}/cancel`
  )) as ApiResponseDto<unknown>;

  if (!response.success) {
    throw new Error(response.message || `Không tìm thấy đơn hàng có ID ${id}`);
  }
};

// ==================== EXPORT OBJECT ====================

/**
 * Object chứa tất cả các API functions cho Order
 */
export const orderApi = {
  getAll,
  getById,
  getDetailById,
  getByUserId,
  getByStatus,
  getExpired,
  create,
  update,
  confirmOrder,
  cancelOrder,
};

