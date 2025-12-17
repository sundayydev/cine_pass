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
 * DTO cho thông tin diễn viên response (từ backend ActorResponseDto)
 */
export interface ActorResponseDto {
    id: string;
    name: string;
    slug?: string;
    description?: string;
    imageUrl?: string;
    createdAt: string;
    updatedAt: string;
}

/**
 * DTO cho tạo diễn viên mới (từ backend ActorCreateDto)
 */
export interface ActorCreateDto {
    name: string;
    slug?: string;
    description?: string;
    imageUrl?: string;
}

/**
 * DTO cho cập nhật diễn viên (từ backend ActorUpdateDto)
 */
export interface ActorUpdateDto {
    name?: string;
    slug?: string;
    description?: string;
    imageUrl?: string;
}

// ==================== API FUNCTIONS ====================

/**
 * Lấy danh sách tất cả diễn viên
 * GET /api/actors
 */
export const getAll = async (): Promise<ActorResponseDto[]> => {
    const response = (await axiosClient.get(
        '/api/actors'
    )) as ApiResponseDto<ActorResponseDto[]>;

    if (!response.success || !response.data) {
        throw new Error(response.message || 'Lỗi khi lấy danh sách diễn viên');
    }

    return response.data;
};

/**
 * Lấy thông tin diễn viên theo ID
 * GET /api/actors/{id}
 */
export const getById = async (id: string): Promise<ActorResponseDto> => {
    const response = (await axiosClient.get(
        `/api/actors/${id}`
    )) as ApiResponseDto<ActorResponseDto>;

    if (!response.success || !response.data) {
        throw new Error(response.message || `Không tìm thấy diễn viên có ID ${id}`);
    }

    return response.data;
};

/**
 * Lấy thông tin diễn viên theo slug
 * GET /api/actors/slug/{slug}
 */
export const getBySlug = async (slug: string): Promise<ActorResponseDto> => {
    const response = (await axiosClient.get(
        `/api/actors/slug/${slug}`
    )) as ApiResponseDto<ActorResponseDto>;

    if (!response.success || !response.data) {
        throw new Error(response.message || `Không tìm thấy diễn viên với slug ${slug}`);
    }

    return response.data;
};

/**
 * Tìm kiếm diễn viên
 * GET /api/actors/search?searchTerm={searchTerm}
 */
export const search = async (searchTerm: string): Promise<ActorResponseDto[]> => {
    if (!searchTerm || searchTerm.trim() === '') {
        throw new Error('Từ khóa tìm kiếm không được để trống');
    }

    const response = (await axiosClient.get(
        '/api/actors/search',
        {
            params: { searchTerm },
        }
    )) as ApiResponseDto<ActorResponseDto[]>;

    if (!response.success || !response.data) {
        throw new Error(response.message || 'Lỗi khi tìm kiếm diễn viên');
    }

    return response.data;
};

/**
 * Tạo diễn viên mới
 * POST /api/actors
 */
export const create = async (dto: ActorCreateDto): Promise<ActorResponseDto> => {
    const response = (await axiosClient.post(
        '/api/actors',
        dto
    )) as ApiResponseDto<ActorResponseDto>;

    if (!response.success || !response.data) {
        throw new Error(response.message || 'Lỗi khi tạo diễn viên');
    }

    return response.data;
};

/**
 * Cập nhật thông tin diễn viên
 * PUT /api/actors/{id}
 */
export const update = async (id: string, dto: ActorUpdateDto): Promise<ActorResponseDto> => {
    const response = (await axiosClient.put(
        `/api/actors/${id}`,
        dto
    )) as ApiResponseDto<ActorResponseDto>;

    if (!response.success || !response.data) {
        throw new Error(response.message || `Không tìm thấy diễn viên có ID ${id}`);
    }

    return response.data;
};

/**
 * Xóa diễn viên
 * DELETE /api/actors/{id}
 */
export const deleteActor = async (id: string): Promise<void> => {
    const response = (await axiosClient.delete(
        `/api/actors/${id}`
    )) as ApiResponseDto<unknown>;

    // Backend trả về NoContent (204) cho delete thành công
    // Axios sẽ không có response.data cho 204
    if (response && typeof response === 'object' && 'success' in response) {
        const errorResponse = response as ApiResponseDto<unknown>;
        if (!errorResponse.success) {
            throw new Error(errorResponse.message || `Không tìm thấy diễn viên có ID ${id}`);
        }
    }
};

// ==================== EXPORT OBJECT ====================

/**
 * Object chứa tất cả các API functions cho Actor
 * Sử dụng pattern này để dễ import: import { actorApi } from '@/services/apiActor'
 */
export const actorApi = {
    getAll,
    getById,
    getBySlug,
    search,
    create,
    update,
    delete: deleteActor,
};
