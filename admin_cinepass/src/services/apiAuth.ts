import axiosClient from '@/lib/axiosClient';

// ==================== TYPES ====================

/**
 * DTO cho đăng nhập
 */
export interface UserLoginDto {
  email: string;
  password: string;
}

/**
 * DTO cho đăng ký
 */
export interface UserCreateDto {
  email: string;
  phone?: string;
  password: string;
  fullName?: string;
  role?: string; // UserRole enum từ backend
}

/**
 * DTO cho refresh token request
 */
export interface RefreshTokenRequestDto {
  refreshToken: string;
}

/**
 * Response từ API authentication
 */
export interface AuthResponseDto {
  accessToken: string;
  accessTokenExpiresAt: string; // ISO date string
  refreshToken: string;
  refreshTokenExpiresAt: string; // ISO date string
}

/**
 * Generic API Response wrapper
 */
export interface ApiResponseDto<T> {
  success: boolean;
  message?: string;
  data?: T;
  errors?: string[];
}

// ==================== API FUNCTIONS ====================

/**
 * Đăng nhập
 * POST /api/auth/login
 */
export const login = async (dto: UserLoginDto): Promise<AuthResponseDto> => {
  // axiosClient interceptor trả về response.data trực tiếp
  const response = (await axiosClient.post(
    '/api/auth/login',
    dto
  )) as ApiResponseDto<AuthResponseDto>;

  if (!response.success || !response.data) {
    throw new Error(response.message || 'Đăng nhập thất bại');
  }

  // Lưu tokens vào localStorage
  localStorage.setItem('accessToken', response.data.accessToken);
  localStorage.setItem('refreshToken', response.data.refreshToken);
  localStorage.setItem('accessTokenExpiresAt', response.data.accessTokenExpiresAt);
  localStorage.setItem('refreshTokenExpiresAt', response.data.refreshTokenExpiresAt);

  return response.data;
};

/**
 * Đăng ký tài khoản mới
 * POST /api/auth/register
 */
export const register = async (dto: UserCreateDto): Promise<AuthResponseDto> => {
  // axiosClient interceptor trả về response.data trực tiếp
  const response = (await axiosClient.post(
    '/api/auth/register',
    dto
  )) as ApiResponseDto<AuthResponseDto>;

  if (!response.success || !response.data) {
    throw new Error(response.message || 'Đăng ký thất bại');
  }

  // Lưu tokens vào localStorage
  localStorage.setItem('accessToken', response.data.accessToken);
  localStorage.setItem('refreshToken', response.data.refreshToken);
  localStorage.setItem('accessTokenExpiresAt', response.data.accessTokenExpiresAt);
  localStorage.setItem('refreshTokenExpiresAt', response.data.refreshTokenExpiresAt);

  return response.data;
};

/**
 * Làm mới access token từ refresh token
 * POST /api/auth/refresh
 */
export const refreshToken = async (refreshToken: string): Promise<AuthResponseDto> => {
  // axiosClient interceptor trả về response.data trực tiếp
  const response = (await axiosClient.post(
    '/api/auth/refresh',
    { refreshToken } as RefreshTokenRequestDto
  )) as ApiResponseDto<AuthResponseDto>;

  if (!response.success || !response.data) {
    throw new Error(response.message || 'Làm mới token thất bại');
  }

  // Cập nhật tokens trong localStorage
  localStorage.setItem('accessToken', response.data.accessToken);
  localStorage.setItem('refreshToken', response.data.refreshToken);
  localStorage.setItem('accessTokenExpiresAt', response.data.accessTokenExpiresAt);
  localStorage.setItem('refreshTokenExpiresAt', response.data.refreshTokenExpiresAt);

  return response.data;
};

/**
 * Đăng xuất và thu hồi refresh token
 * POST /api/auth/logout
 */
export const logout = async (refreshToken: string): Promise<void> => {
  // axiosClient interceptor trả về response.data trực tiếp
  const response = (await axiosClient.post(
    '/api/auth/logout',
    { refreshToken } as RefreshTokenRequestDto
  )) as ApiResponseDto<boolean>;

  if (!response.success) {
    throw new Error(response.message || 'Đăng xuất thất bại');
  }

  // Xóa tokens khỏi localStorage
  localStorage.removeItem('accessToken');
  localStorage.removeItem('refreshToken');
  localStorage.removeItem('accessTokenExpiresAt');
  localStorage.removeItem('refreshTokenExpiresAt');
  localStorage.removeItem('user');
};

/**
 * Lấy access token từ localStorage
 */
export const getAccessToken = (): string | null => {
  return localStorage.getItem('accessToken');
};

/**
 * Lấy refresh token từ localStorage
 */
export const getRefreshToken = (): string | null => {
  return localStorage.getItem('refreshToken');
};

/**
 * Kiểm tra xem access token có hết hạn không
 */
export const isAccessTokenExpired = (): boolean => {
  const expiresAt = localStorage.getItem('accessTokenExpiresAt');
  if (!expiresAt) return true;

  const expiryDate = new Date(expiresAt);
  return expiryDate <= new Date();
};

