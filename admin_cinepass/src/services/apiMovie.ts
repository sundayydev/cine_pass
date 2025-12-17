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
 * DTO cho thông tin diễn viên trong phim (từ backend MovieActorDto)
 */
export interface MovieActorDto {
  id: string;
  name: string;
  slug?: string;
  description?: string;
  imageUrl?: string;
}

/**
 * DTO cho review của phim (từ backend MovieReviewDto)
 */
export interface MovieReviewDto {
  id: string;
  userId?: string;
  userName?: string;
  rating?: number;
  comment?: string;
  createdAt: string;
}

/**
 * DTO cho thông tin phim response (từ backend MovieResponseDto)
 */
export interface MovieResponseDto {
  id: string;
  title: string;
  slug?: string;
  durationMinutes: number;
  description?: string;
  ageLimit: number;
  posterUrl?: string;
  trailerUrl?: string;
  releaseDate?: string; // ISO date string
  category: string; // Backend trả về string (PascalCase: "Action", "Comedy", etc.)
  status: string; // Backend trả về string (PascalCase: "Showing", "ComingSoon", "Ended")
  createdAt: string;
}

/**
 * DTO cho thông tin chi tiết phim (từ backend MovieDetailResponseDto)
 * Extends MovieResponseDto và thêm actors, reviews
 */
export interface MovieDetailResponseDto extends MovieResponseDto {
  actors: MovieActorDto[];
  reviews: MovieReviewDto[];
}

/**
 * DTO cho tạo phim mới (từ backend MovieCreateDto)
 */
export interface MovieCreateDto {
  title: string;
  slug?: string;
  durationMinutes: number;
  description?: string;
  ageLimit?: number;
  posterUrl?: string;
  trailerUrl?: string;
  releaseDate?: string; // ISO date string
  category: number; // MovieCategory enum (0-21)
  status: number; // MovieStatus enum (0 = ComingSoon, 1 = Showing, 2 = Ended)
}

/**
 * DTO cho cập nhật phim (từ backend MovieUpdateDto)
 */
export interface MovieUpdateDto {
  title?: string;
  slug?: string;
  durationMinutes?: number;
  description?: string;
  ageLimit?: number;
  posterUrl?: string;
  trailerUrl?: string;
  releaseDate?: string; // ISO date string
  category?: number; // MovieCategory enum (0-21)
  status?: number; // MovieStatus enum (0 = ComingSoon, 1 = Showing, 2 = Ended)
}

// ==================== CONSTANTS ====================

/**
 * MovieStatus constants (match backend enum)
 */
export const MovieStatus = {
  ComingSoon: 0,
  Showing: 1,
  Ended: 2
} as const;

/**
 * MovieCategory constants (match backend enum)
 */
export const MovieCategory = {
  Movie: 0,
  Series: 1,
  Documentary: 2,
  Animation: 3,
  Action: 4,
  Comedy: 5,
  Drama: 6,
  Horror: 7,
  Romance: 8,
  SciFi: 9,
  Thriller: 10,
  War: 11,
  Western: 12,
  Musical: 13,
  Family: 14,
  Fantasy: 15,
  Adventure: 16,
  Biography: 17,
  History: 18,
  Sport: 19,
  Religious: 20,
  Other: 21
} as const;

// ==================== API FUNCTIONS ====================

/**
 * Lấy danh sách tất cả phim
 * GET /api/movies
 */
export const getAll = async (): Promise<MovieResponseDto[]> => {
  const response = (await axiosClient.get(
    '/api/movies'
  )) as ApiResponseDto<MovieResponseDto[]>;

  if (!response.success || !response.data) {
    throw new Error(response.message || 'Lỗi khi lấy danh sách phim');
  }

  return response.data;
};

/**
 * Lấy thông tin chi tiết phim theo ID (bao gồm actors, reviews)
 * GET /api/movies/{id}
 */
export const getById = async (id: string): Promise<MovieDetailResponseDto> => {
  const response = (await axiosClient.get(
    `/api/movies/${id}`
  )) as ApiResponseDto<MovieDetailResponseDto>;

  if (!response.success || !response.data) {
    throw new Error(response.message || `Không tìm thấy phim có ID ${id}`);
  }

  return response.data;
};

/**
 * Lấy thông tin chi tiết phim theo slug (bao gồm actors, reviews)
 * GET /api/movies/slug/{slug}
 */
export const getBySlug = async (slug: string): Promise<MovieDetailResponseDto> => {
  const response = (await axiosClient.get(
    `/api/movies/slug/${slug}`
  )) as ApiResponseDto<MovieDetailResponseDto>;

  if (!response.success || !response.data) {
    throw new Error(response.message || `Không tìm thấy phim với slug ${slug}`);
  }

  return response.data;
};

/**
 * Lấy danh sách phim đang chiếu
 * GET /api/movies/now-showing
 */
export const getNowShowing = async (): Promise<MovieResponseDto[]> => {
  const response = (await axiosClient.get(
    '/api/movies/now-showing'
  )) as ApiResponseDto<MovieResponseDto[]>;

  if (!response.success || !response.data) {
    throw new Error(response.message || 'Lỗi khi lấy danh sách phim đang chiếu');
  }

  return response.data;
};

/**
 * Lấy danh sách phim sắp chiếu
 * GET /api/movies/coming-soon
 */
export const getComingSoon = async (): Promise<MovieResponseDto[]> => {
  const response = (await axiosClient.get(
    '/api/movies/coming-soon'
  )) as ApiResponseDto<MovieResponseDto[]>;

  if (!response.success || !response.data) {
    throw new Error(response.message || 'Lỗi khi lấy danh sách phim sắp chiếu');
  }

  return response.data;
};

/**
 * Tìm kiếm phim
 * GET /api/movies/search?searchTerm={searchTerm}
 */
export const search = async (searchTerm: string): Promise<MovieResponseDto[]> => {
  if (!searchTerm || searchTerm.trim() === '') {
    throw new Error('Từ khóa tìm kiếm không được để trống');
  }

  const response = (await axiosClient.get(
    '/api/movies/search',
    {
      params: { searchTerm },
    }
  )) as ApiResponseDto<MovieResponseDto[]>;

  if (!response.success || !response.data) {
    throw new Error(response.message || 'Lỗi khi tìm kiếm phim');
  }

  return response.data;
};

/**
 * Tạo phim mới
 * POST /api/movies
 */
export const create = async (dto: MovieCreateDto): Promise<MovieResponseDto> => {
  const response = (await axiosClient.post(
    '/api/movies',
    dto
  )) as ApiResponseDto<MovieResponseDto>;

  if (!response.success || !response.data) {
    throw new Error(response.message || 'Lỗi khi tạo phim');
  }

  return response.data;
};

/**
 * Cập nhật thông tin phim
 * PUT /api/movies/{id}
 */
export const update = async (id: string, dto: MovieUpdateDto): Promise<MovieResponseDto> => {
  const response = (await axiosClient.put(
    `/api/movies/${id}`,
    dto
  )) as ApiResponseDto<MovieResponseDto>;

  if (!response.success || !response.data) {
    throw new Error(response.message || `Không tìm thấy phim có ID ${id}`);
  }

  return response.data;
};

/**
 * Xóa phim
 * DELETE /api/movies/{id}
 */
export const deleteMovie = async (id: string): Promise<void> => {
  const response = (await axiosClient.delete(
    `/api/movies/${id}`
  )) as ApiResponseDto<unknown>;

  // Backend trả về NoContent (204) cho delete thành công
  // Axios sẽ không có response.data cho 204
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


