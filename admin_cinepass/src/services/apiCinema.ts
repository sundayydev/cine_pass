import axiosClient from '@/lib/axiosClient';
import type { Cinema } from '@/types/showtimeType';

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
 * DTO for creating a new cinema
 */
export interface CinemaCreateDto {
  name: string;
  address?: string;
  phone?: string;
  email?: string;
}

/**
 * DTO for updating a cinema
 */
export interface CinemaUpdateDto {
  name?: string;
  address?: string;
  phone?: string;
  email?: string;
  isActive?: boolean;
}

// ==================== API FUNCTIONS ====================

/**
 * Fetch all cinemas
 * GET /api/cinemas
 */
export const getAllCinemas = async (): Promise<Cinema[]> => {
  const response = await axiosClient.get('/api/cinemas') as ApiResponseDto<Cinema[]>;
  if (!response.success || !response.data) {
    throw new Error(response.message || 'Failed to fetch cinemas');
  }
  return response.data;
};

/**
 * Fetch a single cinema by its ID
 * GET /api/cinemas/{id}
 */
export const getCinemaById = async (id: string): Promise<Cinema> => {
  const response = await axiosClient.get(`/api/cinemas/${id}`) as ApiResponseDto<Cinema>;
  if (!response.success || !response.data) {
    throw new Error(response.message || `Cinema with ID ${id} not found`);
  }
  return response.data;
};

/**
 * Create a new cinema
 * POST /api/cinemas
 */
export const createCinema = async (payload: CinemaCreateDto): Promise<Cinema> => {
  const response = await axiosClient.post('/api/cinemas', payload) as ApiResponseDto<Cinema>;
  if (!response.success || !response.data) {
    throw new Error(response.message || 'Failed to create cinema');
  }
  return response.data;
};

/**
 * Update an existing cinema
 * PUT /api/cinemas/{id}
 */
export const updateCinema = async (id: string, payload: CinemaUpdateDto): Promise<Cinema> => {
  const response = await axiosClient.put(`/api/cinemas/${id}`, payload) as ApiResponseDto<Cinema>;
  if (!response.success || !response.data) {
    throw new Error(response.message || `Failed to update cinema with ID ${id}`);
  }
  return response.data;
};

/**
 * Delete a cinema
 * DELETE /api/cinemas/{id}
 */
export const deleteCinema = async (id: string): Promise<void> => {
  const response = await axiosClient.delete(`/api/cinemas/${id}`) as ApiResponseDto<unknown>;
  if (!response.success) {
    throw new Error(response.message || `Failed to delete cinema with ID ${id}`);
  }
};

// ==================== EXPORT OBJECT ====================

export const cinemaApi = {
  getAll: getAllCinemas,
  getById: getCinemaById,
  create: createCinema,
  update: updateCinema,
  delete: deleteCinema,
};
