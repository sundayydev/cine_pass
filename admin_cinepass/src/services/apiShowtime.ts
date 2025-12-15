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
 * Showtime object from the backend
 */
export interface Showtime {
  id: string; // Guid is a string in TS
  movieId: string;
  screenId: string;
  startTime: string; // DateTime is a string (ISO 8601)
  endTime: string;   // DateTime is a string (ISO 8601)
  basePrice: number; // decimal is a number
  isActive: boolean;
  // Optional navigation properties that might be included
  movie?: { id: string; title: string };
  screen?: { id: string; name: string; cinemaId: string; };
}

/**
 * Payload for creating a new showtime
 */
export interface ShowtimeCreatePayload {
  movieId: string;
  screenId: string;
  startTime: string; // ISO 8601 string
  basePrice: number;
}

/**
 * Payload for updating an existing showtime
 */
export type ShowtimeUpdatePayload = Partial<ShowtimeCreatePayload & { isActive: boolean }>;


// ==================== API FUNCTIONS ====================

/**
 * Fetch all showtimes
 * GET /api/showtimes
 */
export const getAllShowtimes = async (): Promise<Showtime[]> => {
  const response = await axiosClient.get('/api/showtimes') as ApiResponseDto<Showtime[]>;
  if (!response.success || !response.data) {
    throw new Error(response.message || 'Failed to fetch showtimes');
  }
  return response.data;
};

/**
 * Fetch a single showtime by its ID
 * GET /api/showtimes/{id}
 */
export const getShowtimeById = async (id: string): Promise<Showtime> => {
  const response = await axiosClient.get(`/api/showtimes/${id}`) as ApiResponseDto<Showtime>;
  if (!response.success || !response.data) {
    throw new Error(response.message || `Showtime with ID ${id} not found`);
  }
  return response.data;
};

/**
 * Create a new showtime
 * POST /api/showtimes
 */
export const createShowtime = async (payload: ShowtimeCreatePayload): Promise<Showtime> => {
  const response = await axiosClient.post('/api/showtimes', payload) as ApiResponseDto<Showtime>;
  if (!response.success || !response.data) {
    throw new Error(response.message || 'Failed to create showtime');
  }
  return response.data;
};

/**
 * Update an existing showtime
 * PUT /api/showtimes/{id}
 */
export const updateShowtime = async (id: string, payload: ShowtimeUpdatePayload): Promise<Showtime> => {
  const response = await axiosClient.put(`/api/showtimes/${id}`, payload) as ApiResponseDto<Showtime>;
  if (!response.success || !response.data) {
    throw new Error(response.message || `Failed to update showtime with ID ${id}`);
  }
  return response.data;
};

/**
 * Delete a showtime
 * DELETE /api/showtimes/{id}
 */
export const deleteShowtime = async (id: string): Promise<void> => {
  const response = await axiosClient.delete(`/api/showtimes/${id}`) as ApiResponseDto<unknown>;
    if (!response.success) {
      throw new Error(response.message || `Failed to delete showtime with ID ${id}`);
    }
};

// ==================== EXPORT OBJECT ====================

export const showtimeApi = {
  getAll: getAllShowtimes,
  getById: getShowtimeById,
  create: createShowtime,
  update: updateShowtime,
  delete: deleteShowtime,
};