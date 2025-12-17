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

/**
 * Seat status enum matching backend
 */
export const SeatStatus = {
  Available: 0,
  Sold: 1,
  Holding: 2,
} as const;
export type SeatStatus = typeof SeatStatus[keyof typeof SeatStatus];

/**
 * Seat with booking status
 */
export interface SeatWithStatusDto {
  id: string;
  seatRow: string;
  seatNumber: number;
  seatCode: string;
  seatTypeCode?: string;
  status: SeatStatus;
  price: number;
  heldByUserId?: string;
}

/**
 * Response for showtime seats with status
 */
export interface ShowtimeSeatsResponse {
  showtimeId: string;
  screenId: string;
  screenName: string;
  showDateTime: string;
  seats: SeatWithStatusDto[];
  totalSeats: number;
  availableSeats: number;
  soldSeats: number;
  holdingSeats: number;
}


// ==================== API FUNCTIONS ====================

/**
 * Fetch all showtimes (Note: Backend doesn't have GetAll endpoint yet)
 * Will use GetByDate with a wide date range as fallback
 */
export const getAllShowtimes = async (): Promise<Showtime[]> => {
  try {
    // Try to get all showtimes - if endpoint exists
    const response = await axiosClient.get('/api/showtimes') as ApiResponseDto<Showtime[]>;
    if (response.success && response.data) {
      return response.data;
    }
  } catch (error) {
    console.warn('GetAll endpoint not available, using date range fallback');
  }

  // Fallback: Get showtimes for next 30 days
  const today = new Date();
  const endDate = new Date();
  endDate.setDate(today.getDate() + 30);

  const showtimes: Showtime[] = [];
  const currentDate = new Date(today);

  while (currentDate <= endDate) {
    try {
      const dateStr = currentDate.toISOString().split('T')[0];
      const dailyShowtimes = await getShowtimesByDate(dateStr);
      showtimes.push(...dailyShowtimes);
    } catch (error) {
      console.error('Error fetching showtimes for date:', currentDate, error);
    }
    currentDate.setDate(currentDate.getDate() + 1);
  }

  return showtimes;
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

/**
 * Get showtimes by movie ID
 * GET /api/showtimes/movie/{movieId}
 */
export const getShowtimesByMovie = async (movieId: string): Promise<Showtime[]> => {
  const response = await axiosClient.get(`/api/showtimes/movie/${movieId}`) as ApiResponseDto<Showtime[]>;
  if (!response.success || !response.data) {
    throw new Error(response.message || 'Failed to fetch showtimes by movie');
  }
  return response.data;
};

/**
 * Get showtimes by date
 * GET /api/showtimes/date/{date}
 * @param date - ISO date string (YYYY-MM-DD)
 */
export const getShowtimesByDate = async (date: string): Promise<Showtime[]> => {
  const response = await axiosClient.get(`/api/showtimes/date/${date}`) as ApiResponseDto<Showtime[]>;
  if (!response.success || !response.data) {
    throw new Error(response.message || 'Failed to fetch showtimes by date');
  }
  return response.data;
};

/**
 * Get showtimes by movie and date
 * GET /api/showtimes/movie/{movieId}/date/{date}
 */
export const getShowtimesByMovieAndDate = async (movieId: string, date: string): Promise<Showtime[]> => {
  const response = await axiosClient.get(`/api/showtimes/movie/${movieId}/date/${date}`) as ApiResponseDto<Showtime[]>;
  if (!response.success || !response.data) {
    throw new Error(response.message || 'Failed to fetch showtimes');
  }
  return response.data;
};

/**
 * Get showtimes by date range
 * Client-side implementation - fetches multiple dates
 */
export const getShowtimesByDateRange = async (startDate: string, endDate: string): Promise<Showtime[]> => {
  const start = new Date(startDate);
  const end = new Date(endDate);
  const showtimes: Showtime[] = [];

  const currentDate = new Date(start);
  while (currentDate <= end) {
    try {
      const dateStr = currentDate.toISOString().split('T')[0];
      const dailyShowtimes = await getShowtimesByDate(dateStr);
      showtimes.push(...dailyShowtimes);
    } catch (error) {
      console.error('Error fetching showtimes for date:', currentDate, error);
    }
    currentDate.setDate(currentDate.getDate() + 1);
  }

  return showtimes;
};

/**
 * Get seats with status for a showtime
 * GET /api/showtimes/{id}/seats
 */
export const getSeatsWithStatus = async (showtimeId: string): Promise<ShowtimeSeatsResponse> => {
  const response = await axiosClient.get(`/api/showtimes/${showtimeId}/seats`) as ApiResponseDto<ShowtimeSeatsResponse>;
  if (!response.success || !response.data) {
    throw new Error(response.message || 'Failed to fetch showtime seats');
  }
  return response.data;
};

// ==================== EXPORT OBJECT ====================

export const showtimeApi = {
  getAll: getAllShowtimes,
  getById: getShowtimeById,
  getByMovie: getShowtimesByMovie,
  getByDate: getShowtimesByDate,
  getByMovieAndDate: getShowtimesByMovieAndDate,
  getByDateRange: getShowtimesByDateRange,
  getSeatsWithStatus,
  create: createShowtime,
  update: updateShowtime,
  delete: deleteShowtime,
};