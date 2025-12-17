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
 * DTO cho thông tin movie-actor response từ backend
 */
export interface MovieActorResponseDto {
    id: string;
    movieId: string;
    actorId: string;
    characterName?: string;
    role?: string;
    // Additional fields từ backend nếu có
    createdAt?: string;
    updatedAt?: string;
}

/**
 * DTO cho tạo mới movie-actor (thêm diễn viên vào phim)
 */
export interface MovieActorCreateDto {
    movieId: string;
    actorId: string;
    characterName?: string;
    role?: string;
}

/**
 * DTO cho cập nhật movie-actor
 */
export interface MovieActorUpdateDto {
    characterName?: string;
    role?: string;
}

// ==================== API FUNCTIONS ====================

/**
 * Lấy danh sách tất cả các mối quan hệ phim-diễn viên
 * GET /api/movieactors
 */
export const getAll = async (): Promise<MovieActorResponseDto[]> => {
    const response = (await axiosClient.get(
        '/api/movieactors'
    )) as ApiResponseDto<MovieActorResponseDto[]>;

    if (!response.success || !response.data) {
        throw new Error(response.message || 'Lỗi khi lấy danh sách phim-diễn viên');
    }

    return response.data;
};

/**
 * Lấy thông tin mối quan hệ phim-diễn viên theo ID
 * GET /api/movieactors/{id}
 */
export const getById = async (id: string): Promise<MovieActorResponseDto> => {
    const response = (await axiosClient.get(
        `/api/movieactors/${id}`
    )) as ApiResponseDto<MovieActorResponseDto>;

    if (!response.success || !response.data) {
        throw new Error(response.message || `Không tìm thấy mối quan hệ có ID ${id}`);
    }

    return response.data;
};

/**
 * Lấy danh sách diễn viên của một phim
 * GET /api/movieactors/movie/{movieId}
 */
export const getByMovieId = async (movieId: string): Promise<MovieActorResponseDto[]> => {
    const response = (await axiosClient.get(
        `/api/movieactors/movie/${movieId}`
    )) as ApiResponseDto<MovieActorResponseDto[]>;

    if (!response.success || !response.data) {
        throw new Error(response.message || 'Lỗi khi lấy danh sách diễn viên của phim');
    }

    return response.data;
};

/**
 * Lấy danh sách phim của một diễn viên
 * GET /api/movieactors/actor/{actorId}
 */
export const getByActorId = async (actorId: string): Promise<MovieActorResponseDto[]> => {
    const response = (await axiosClient.get(
        `/api/movieactors/actor/${actorId}`
    )) as ApiResponseDto<MovieActorResponseDto[]>;

    if (!response.success || !response.data) {
        throw new Error(response.message || 'Lỗi khi lấy danh sách phim của diễn viên');
    }

    return response.data;
};

/**
 * Thêm diễn viên vào phim
 * POST /api/movieactors
 */
export const create = async (payload: MovieActorCreateDto): Promise<MovieActorResponseDto> => {
    const response = (await axiosClient.post(
        '/api/movieactors',
        payload
    )) as ApiResponseDto<MovieActorResponseDto>;

    if (!response.success || !response.data) {
        throw new Error(response.message || 'Lỗi khi thêm diễn viên vào phim');
    }

    return response.data;
};

/**
 * Xóa diễn viên khỏi phim
 * DELETE /api/movieactors/{id}
 */
export const deleteMovieActor = async (id: string): Promise<void> => {
    const response = (await axiosClient.delete(
        `/api/movieactors/${id}`
    )) as ApiResponseDto<unknown>;

    // Backend trả về NoContent (204) cho delete thành công
    // Axios sẽ không có response.data cho 204
    if (response && typeof response === 'object' && 'success' in response) {
        const errorResponse = response as ApiResponseDto<unknown>;
        if (!errorResponse.success) {
            throw new Error(errorResponse.message || `Không tìm thấy mối quan hệ có ID ${id}`);
        }
    }
};

// ==================== EXPORT OBJECT ====================

/**
 * Object chứa tất cả các API functions cho MovieActor
 * Sử dụng pattern này để dễ import: import { movieActorApi } from '@/services/apiMovieActor'
 */
export const movieActorApi = {
    getAll,
    getById,
    getByMovieId,
    getByActorId,
    create,
    delete: deleteMovieActor,
};
