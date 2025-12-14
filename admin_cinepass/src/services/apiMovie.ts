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
  releaseDate?: string; // ISO date string (YYYY-MM-DDTHH:mm:ss.sssZ)
  category: number; // MovieCategory enum
  status: number; // MovieStatus enum: 0 = ComingSoon, 1 = Showing, 2 = Ended
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
  releaseDate?: string; // ISO date string (YYYY-MM-DDTHH:mm:ss.sssZ)
  category?: number; // MovieCategory enum
  status?: number; // MovieStatus enum: 0 = ComingSoon, 1 = Showing, 2 = Ended
}

// ==================== HELPER FUNCTIONS ====================

/**
 * Helper: Map frontend MovieStatus sang backend MovieStatus (số enum)
 * Backend enum: ComingSoon = 0, Showing = 1, Ended = 2
 */
const mapStatusToBackend = (status: string): number => {
  const statusMap: Record<string, number> = {
    COMING_SOON: 0,    // ComingSoon
    NOW_SHOWING: 1,    // Showing
    ENDED: 2,          // Ended
    CANCELLED: 2,      // Ended (map CANCELLED về Ended)
  };
  return statusMap[status] ?? 0; // Default to ComingSoon (0)
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
 * Helper: Map frontend MovieCategory (string) sang backend MovieCategory enum (number)
 * Backend enum: Movie=0, Series=1, Documentary=2, Animation=3, Action=4, Comedy=5, Drama=6, Horror=7, Romance=8, SciFi=9, Thriller=10, War=11, Western=12, Musical=13, Family=14, Fantasy=15, Adventure=16, Biography=17, History=18, Sport=19, Religious=20, Other=21
 */
const mapCategoryToBackend = (category: string): number => {
  const categoryMap: Record<string, number> = {
    MOVIE: 0,
    SERIES: 1,
    DOCUMENTARY: 2,
    ANIMATION: 3,
    ACTION: 4,
    COMEDY: 5,
    DRAMA: 6,
    HORROR: 7,
    ROMANCE: 8,
    SCIFI: 9,
    THRILLER: 10,
    WAR: 11,
    WESTERN: 12,
    MUSICAL: 13,
    FAMILY: 14,
    FANTASY: 15,
    ADVENTURE: 16,
    BIOGRAPHY: 17,
    HISTORY: 18,
    SPORT: 19,
    RELIGIOUS: 20,
    OTHER: 21,
  };
  return categoryMap[category] ?? 0; // Default to Movie (0)
};

/**
 * Helper: Map backend MovieCategory enum (string/number) sang frontend MovieCategory (string)
 */
const mapCategoryFromBackend = (category: string | number): string => {
  if (typeof category === 'number') {
    const categoryMap: Record<number, string> = {
      0: 'MOVIE',
      1: 'SERIES',
      2: 'DOCUMENTARY',
      3: 'ANIMATION',
      4: 'ACTION',
      5: 'COMEDY',
      6: 'DRAMA',
      7: 'HORROR',
      8: 'ROMANCE',
      9: 'SCIFI',
      10: 'THRILLER',
      11: 'WAR',
      12: 'WESTERN',
      13: 'MUSICAL',
      14: 'FAMILY',
      15: 'FANTASY',
      16: 'ADVENTURE',
      17: 'BIOGRAPHY',
      18: 'HISTORY',
      19: 'SPORT',
      20: 'RELIGIOUS',
      21: 'OTHER',
    };
    return categoryMap[category] || 'MOVIE';
  }
  
  // If string, map PascalCase to UPPER_SNAKE_CASE
  const categoryMap: Record<string, string> = {
    Movie: 'MOVIE',
    Series: 'SERIES',
    Documentary: 'DOCUMENTARY',
    Animation: 'ANIMATION',
    Action: 'ACTION',
    Comedy: 'COMEDY',
    Drama: 'DRAMA',
    Horror: 'HORROR',
    Romance: 'ROMANCE',
    SciFi: 'SCIFI',
    Thriller: 'THRILLER',
    War: 'WAR',
    Western: 'WESTERN',
    Musical: 'MUSICAL',
    Family: 'FAMILY',
    Fantasy: 'FANTASY',
    Adventure: 'ADVENTURE',
    Biography: 'BIOGRAPHY',
    History: 'HISTORY',
    Sport: 'SPORT',
    Religious: 'RELIGIOUS',
    Other: 'OTHER',
  };
  return categoryMap[category] || 'MOVIE';
};

/**
 * Helper: Map Movie từ backend sang frontend (chuyển đổi status và category)
 */
const mapMovieFromBackend = (movie: Movie): Movie => {
  return {
    ...movie,
    status: mapStatusFromBackend(movie.status),
    category: mapCategoryFromBackend(movie.category) as any,
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
  // Format releaseDate thành ISO string
  let releaseDate: string | undefined;
  if (payload.releaseDate) {
    if (typeof payload.releaseDate === 'string') {
      // Nếu đã là string YYYY-MM-DD, convert sang ISO string
      if (/^\d{4}-\d{2}-\d{2}$/.test(payload.releaseDate)) {
        releaseDate = `${payload.releaseDate}T00:00:00.000Z`;
      } else {
        // Nếu đã là ISO string, giữ nguyên
        releaseDate = payload.releaseDate;
      }
    } else {
      // Nếu là Date object, convert sang ISO string
      const date = new Date(payload.releaseDate);
      date.setUTCHours(0, 0, 0, 0);
      releaseDate = date.toISOString();
    }
  }

  // Chuyển đổi MoviePayload sang MovieCreateDto
  // Chỉ gửi các field có giá trị, không gửi undefined
  const dto: MovieCreateDto = {
    title: payload.title,
    durationMinutes: payload.durationMinutes,
    status: mapStatusToBackend(payload.status),
    category: mapCategoryToBackend(payload.category),
    // Chỉ thêm các field optional nếu có giá trị
    ...(payload.description && { description: payload.description }),
    ...(payload.trailerUrl && { trailerUrl: payload.trailerUrl }),
    ...(releaseDate && { releaseDate }),
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
    // Format releaseDate giống như trong create function
    if (typeof payload.releaseDate === 'string') {
      // Nếu đã là string YYYY-MM-DD, convert sang ISO string
      if (/^\d{4}-\d{2}-\d{2}$/.test(payload.releaseDate)) {
        dto.releaseDate = `${payload.releaseDate}T00:00:00.000Z`;
      } else {
        // Nếu đã là ISO string, giữ nguyên
        dto.releaseDate = payload.releaseDate;
      }
    } else {
      // Nếu là Date object, convert sang ISO string
      const date = new Date(payload.releaseDate);
      date.setUTCHours(0, 0, 0, 0);
      dto.releaseDate = date.toISOString();
    }
  }
  if (payload.status !== undefined) {
    dto.status = mapStatusToBackend(payload.status);
  }
  if (payload.category !== undefined) {
    dto.category = mapCategoryToBackend(payload.category);
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

  if (response && typeof response === 'object' && 'success' in response) {
    const errorResponse = response as ApiResponseDto<unknown>;
    if (!errorResponse.success) {
      throw new Error(errorResponse.message || `Không tìm thấy phim có ID ${id}`);
    }
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

