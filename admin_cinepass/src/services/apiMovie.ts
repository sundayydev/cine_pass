import axiosClient from '@/lib/axiosClient';
import type { Movie, MoviePayload } from '@/types/moveType';

// Re-export types for convenience
export type { Movie, MoviePayload } from '@/types/moveType';

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
 * DTO cho tạo phim mới (từ backend)
 */
export interface MovieCreateDto {
  title: string;
  slug?: string;
  durationMinutes: number;
  description?: string;
  posterUrl?: string;
  trailerUrl?: string;
  releaseDate?: string; // ISO date string
  status: 'ComingSoon' | 'Showing' | 'Ended'; // MovieStatus enum
}

/**
 * DTO cho cập nhật phim (từ backend)
 */
export interface MovieUpdateDto {
  title?: string;
  slug?: string;
  durationMinutes?: number;
  description?: string;
  posterUrl?: string;
  trailerUrl?: string;
  releaseDate?: string; // ISO date string
  status?: 'ComingSoon' | 'Showing' | 'Ended'; // MovieStatus enum
}

// ==================== HELPER FUNCTIONS ====================

/**
 * Helper: Map frontend MovieStatus sang backend MovieStatus
 */
const mapStatusToBackend = (status: string): 'ComingSoon' | 'Showing' | 'Ended' => {
  const statusMap: Record<string, 'ComingSoon' | 'Showing' | 'Ended'> = {
    COMING_SOON: 'ComingSoon',
    NOW_SHOWING: 'Showing',
    ENDED: 'Ended',
    CANCELLED: 'Ended',
  };
  return statusMap[status] || 'ComingSoon';
};

/**
 * Helper: Map backend MovieStatus sang frontend MovieStatus
 */
const mapStatusFromBackend = (status: string): 'COMING_SOON' | 'NOW_SHOWING' | 'ENDED' | 'CANCELLED' => {
  const statusMap: Record<string, 'COMING_SOON' | 'NOW_SHOWING' | 'ENDED' | 'CANCELLED'> = {
    ComingSoon: 'COMING_SOON',
    Showing: 'NOW_SHOWING',
    Ended: 'ENDED',
  };
  return statusMap[status] || 'COMING_SOON';
};

/**
 * Helper: Map Movie từ backend sang frontend (chuyển đổi status)
 */
const mapMovieFromBackend = (movie: Movie): Movie => {
  return {
    ...movie,
    status: mapStatusFromBackend(movie.status),
  };
};

/**
 * Helper: Map danh sách Movie từ backend sang frontend
 */
const mapMoviesFromBackend = (movies: Movie[]): Movie[] => {
  return movies.map(mapMovieFromBackend);
};

// ==================== API FUNCTIONS ====================

/**
 * Lấy danh sách tất cả phim
 * GET /api/movies
 * @param _params - Optional pagination params (not used by backend, kept for compatibility)
 */
export const getAll = async (_params?: unknown): Promise<Movie[]> => {
  const response = (await axiosClient.get(
    '/api/movies'
  )) as ApiResponseDto<Movie[]>;

  if (!response.success || !response.data) {
    throw new Error(response.message || 'Lỗi khi lấy danh sách phim');
  }

  return mapMoviesFromBackend(response.data);
};

/**
 * Lấy thông tin phim theo ID
 * GET /api/movies/{id}
 */
export const getById = async (id: string): Promise<Movie> => {
  const response = (await axiosClient.get(
    `/api/movies/${id}`
  )) as ApiResponseDto<Movie>;

  if (!response.success || !response.data) {
    throw new Error(response.message || `Không tìm thấy phim có ID ${id}`);
  }

  return mapMovieFromBackend(response.data);
};

/**
 * Lấy thông tin phim theo slug
 * GET /api/movies/slug/{slug}
 */
export const getBySlug = async (slug: string): Promise<Movie> => {
  const response = (await axiosClient.get(
    `/api/movies/slug/${slug}`
  )) as ApiResponseDto<Movie>;

  if (!response.success || !response.data) {
    throw new Error(response.message || `Không tìm thấy phim với slug ${slug}`);
  }

  return mapMovieFromBackend(response.data);
};

/**
 * Lấy danh sách phim đang chiếu
 * GET /api/movies/now-showing
 */
export const getNowShowing = async (): Promise<Movie[]> => {
  const response = (await axiosClient.get(
    '/api/movies/now-showing'
  )) as ApiResponseDto<Movie[]>;

  if (!response.success || !response.data) {
    throw new Error(response.message || 'Lỗi khi lấy danh sách phim đang chiếu');
  }

  return mapMoviesFromBackend(response.data);
};

/**
 * Lấy danh sách phim sắp chiếu
 * GET /api/movies/coming-soon
 */
export const getComingSoon = async (): Promise<Movie[]> => {
  const response = (await axiosClient.get(
    '/api/movies/coming-soon'
  )) as ApiResponseDto<Movie[]>;

  if (!response.success || !response.data) {
    throw new Error(response.message || 'Lỗi khi lấy danh sách phim sắp chiếu');
  }

  return mapMoviesFromBackend(response.data);
};

/**
 * Tìm kiếm phim
 * GET /api/movies/search?searchTerm={searchTerm}
 */
export const search = async (searchTerm: string): Promise<Movie[]> => {
  if (!searchTerm || searchTerm.trim() === '') {
    throw new Error('Từ khóa tìm kiếm không được để trống');
  }

  const response = (await axiosClient.get(
    '/api/movies/search',
    {
      params: { searchTerm },
    }
  )) as ApiResponseDto<Movie[]>;

  if (!response.success || !response.data) {
    throw new Error(response.message || 'Lỗi khi tìm kiếm phim');
  }

  return mapMoviesFromBackend(response.data);
};

/**
 * Tạo phim mới
 * POST /api/movies
 */
export const create = async (payload: MoviePayload): Promise<Movie> => {
  // Chuyển đổi MoviePayload sang MovieCreateDto
  const dto: MovieCreateDto = {
    title: payload.title,
    // Slug sẽ được backend tự động tạo từ title nếu không cung cấp
    durationMinutes: payload.durationMinutes,
    description: payload.description,
    // TODO: Handle file upload - posterUrl sẽ được set sau khi upload file
    posterUrl: undefined,
    trailerUrl: payload.trailerUrl,
    releaseDate: payload.releaseDate
      ? typeof payload.releaseDate === 'string'
        ? payload.releaseDate
        : new Date(payload.releaseDate).toISOString()
      : undefined,
    status: mapStatusToBackend(payload.status),
  };

  const response = (await axiosClient.post(
    '/api/movies',
    dto
  )) as ApiResponseDto<Movie>;

  if (!response.success || !response.data) {
    throw new Error(response.message || 'Lỗi khi tạo phim');
  }

  // Map status từ backend sang frontend
  const movie = response.data;
  return {
    ...movie,
    status: mapStatusFromBackend(movie.status),
  };
};

/**
 * Cập nhật thông tin phim
 * PUT /api/movies/{id}
 */
export const update = async (id: string, payload: Partial<MoviePayload>): Promise<Movie> => {
  // Chuyển đổi Partial<MoviePayload> sang MovieUpdateDto
  const dto: MovieUpdateDto = {};

  if (payload.title !== undefined) dto.title = payload.title;
  if (payload.durationMinutes !== undefined) dto.durationMinutes = payload.durationMinutes;
  if (payload.description !== undefined) dto.description = payload.description;
  // TODO: Handle file upload - posterUrl sẽ được set sau khi upload file
  if (payload.trailerUrl !== undefined) dto.trailerUrl = payload.trailerUrl;
  if (payload.releaseDate !== undefined) {
    dto.releaseDate =
      typeof payload.releaseDate === 'string'
        ? payload.releaseDate
        : new Date(payload.releaseDate).toISOString();
  }
  if (payload.status !== undefined) {
    dto.status = mapStatusToBackend(payload.status);
  }

  const response = (await axiosClient.put(
    `/api/movies/${id}`,
    dto
  )) as ApiResponseDto<Movie>;

  if (!response.success || !response.data) {
    throw new Error(response.message || `Không tìm thấy phim có ID ${id}`);
  }

  // Map status từ backend sang frontend
  const movie = response.data;
  return {
    ...movie,
    status: mapStatusFromBackend(movie.status),
  };
};

/**
 * Xóa phim
 * DELETE /api/movies/{id}
 */
export const deleteMovie = async (id: string): Promise<void> => {
  const response = (await axiosClient.delete(
    `/api/movies/${id}`
  )) as ApiResponseDto<unknown>;

  if (!response.success) {
    throw new Error(response.message || `Không tìm thấy phim có ID ${id}`);
  }
};

// ==================== EXPORT OBJECT ====================

/**
 * Object chứa tất cả các API functions cho Movie
 * Sử dụng pattern này để dễ import: import { movieApi } from '@/services/apiMovie'
 */
export const movieApi = {
  getAll,
  getById,
  getBySlug,
  getNowShowing,
  getComingSoon,
  search,
  create,
  update,
  delete: deleteMovie,
};

