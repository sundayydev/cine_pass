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
 * DTO cho tạo người dùng mới
 */
export interface UserCreateDto {
  email: string;
  phone?: string;
  password: string;
  fullName?: string;
  role?: string;
}

/**
 * DTO cho cập nhật người dùng
 */
export interface UserUpdateDto {
  email?: string;
  phone?: string;
  password?: string;
  fullName?: string;
  role?: string;
}

/**
 * Response DTO cho người dùng
 */
export interface UserResponseDto {
  id: string;
  email: string;
  phone?: string;
  fullName?: string;
  role: string;
  createdAt: string;
  updatedAt: string;
}

// ==================== API FUNCTIONS ====================

/**
 * Lấy danh sách tất cả người dùng
 * GET /api/users
 */
export const getAll = async (): Promise<UserResponseDto[]> => {
  const response = (await axiosClient.get(
    '/api/users'
  )) as ApiResponseDto<UserResponseDto[]>;

  if (!response.success || !response.data) {
    throw new Error(response.message || 'Lỗi khi lấy danh sách người dùng');
  }

  return response.data;
};

/**
 * Lấy thông tin người dùng theo ID
 * GET /api/users/{id}
 */
export const getById = async (id: string): Promise<UserResponseDto> => {
  const response = (await axiosClient.get(
    `/api/users/${id}`
  )) as ApiResponseDto<UserResponseDto>;

  if (!response.success || !response.data) {
    throw new Error(response.message || `Không tìm thấy người dùng có ID ${id}`);
  }

  return response.data;
};

/**
 * Lấy thông tin người dùng theo email
 * GET /api/users/email/{email}
 */
export const getByEmail = async (email: string): Promise<UserResponseDto> => {
  const response = (await axiosClient.get(
    `/api/users/email/${email}`
  )) as ApiResponseDto<UserResponseDto>;

  if (!response.success || !response.data) {
    throw new Error(response.message || `Không tìm thấy người dùng với email ${email}`);
  }

  return response.data;
};

/**
 * Lấy danh sách người dùng theo vai trò
 * GET /api/users/role/{role}
 */
export const getByRole = async (role: string): Promise<UserResponseDto[]> => {
  const response = (await axiosClient.get(
    `/api/users/role/${role}`
  )) as ApiResponseDto<UserResponseDto[]>;

  if (!response.success || !response.data) {
    throw new Error(response.message || 'Lỗi khi lấy danh sách người dùng');
  }

  return response.data;
};

/**
 * Tạo người dùng mới
 * POST /api/users
 */
export const create = async (dto: UserCreateDto): Promise<UserResponseDto> => {
  const response = (await axiosClient.post(
    '/api/users',
    dto
  )) as ApiResponseDto<UserResponseDto>;

  if (!response.success || !response.data) {
    throw new Error(response.message || 'Lỗi khi tạo người dùng');
  }

  return response.data;
};

/**
 * Cập nhật thông tin người dùng
 * PUT /api/users/{id}
 */
export const update = async (id: string, dto: UserUpdateDto): Promise<UserResponseDto> => {
  const response = (await axiosClient.put(
    `/api/users/${id}`,
    dto
  )) as ApiResponseDto<UserResponseDto>;

  if (!response.success || !response.data) {
    throw new Error(response.message || `Không tìm thấy người dùng có ID ${id}`);
  }

  return response.data;
};

/**
 * Xóa người dùng
 * DELETE /api/users/{id}
 */
export const deleteUser = async (id: string): Promise<void> => {
  const response = (await axiosClient.delete(
    `/api/users/${id}`
  )) as ApiResponseDto<unknown>;

  if (response && typeof response === 'object' && 'success' in response) {
    const errorResponse = response as ApiResponseDto<unknown>;
    if (!errorResponse.success) {
      throw new Error(errorResponse.message || `Không tìm thấy người dùng có ID ${id}`);
    }
  }
};

// ==================== EXPORT OBJECT ====================

/**
 * Object chứa tất cả các API functions cho User
 */
export const userApi = {
  getAll,
  getById,
  getByEmail,
  getByRole,
  create,
  update,
  delete: deleteUser,
};

