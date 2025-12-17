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
 * Response DTO for image upload (từ backend UploadImageResponseDto)
 */
export interface UploadImageResponseDto {
    publicId: string;
    url: string;
    secureUrl: string;
    width: number;
    height: number;
    format: string;
    bytes: number;
}

/**
 * Response DTO for multiple images upload (từ backend UploadMultipleImagesResponseDto)
 */
export interface UploadMultipleImagesResponseDto {
    images: UploadImageResponseDto[];
    successCount: number;
    failedCount: number;
    errors: string[];
}

// ==================== CONSTANTS ====================

/**
 * Valid entity types for entity-specific uploads
 */
export const ENTITY_TYPES = {
    MOVIE: 'movie',
    ACTOR: 'actor',
    CINEMA: 'cinema',
    PRODUCT: 'product',
    BANNER: 'banner',
    USER: 'user',
} as const;

export type EntityType = typeof ENTITY_TYPES[keyof typeof ENTITY_TYPES];

// ==================== API FUNCTIONS ====================

/**
 * Upload a single image
 * POST /api/upload/image
 * @param file - Image file to upload
 * @param folder - Optional folder name (default: cinepass)
 * @returns Uploaded image details including URL
 */
export const uploadImage = async (
    file: File,
    folder?: string
): Promise<UploadImageResponseDto> => {
    const formData = new FormData();
    formData.append('file', file);

    const response = (await axiosClient.post(
        '/api/upload/image',
        formData,
        {
            params: folder ? { folder } : undefined,
            headers: {
                'Content-Type': 'multipart/form-data',
            },
        }
    )) as ApiResponseDto<UploadImageResponseDto>;

    if (!response.success || !response.data) {
        throw new Error(response.message || 'Lỗi khi upload ảnh');
    }

    return response.data;
};

/**
 * Upload multiple images at once
 * POST /api/upload/images
 * @param files - Array of image files to upload
 * @param folder - Optional folder name (default: cinepass)
 * @returns List of uploaded images with success/failure count
 */
export const uploadMultipleImages = async (
    files: File[],
    folder?: string
): Promise<UploadMultipleImagesResponseDto> => {
    const formData = new FormData();
    files.forEach((file) => {
        formData.append('files', file);
    });

    const response = (await axiosClient.post(
        '/api/upload/images',
        formData,
        {
            params: folder ? { folder } : undefined,
            headers: {
                'Content-Type': 'multipart/form-data',
            },
        }
    )) as ApiResponseDto<UploadMultipleImagesResponseDto>;

    if (!response.success || !response.data) {
        throw new Error(response.message || 'Lỗi khi upload nhiều ảnh');
    }

    return response.data;
};

/**
 * Delete an image from Cloudinary
 * DELETE /api/upload/image/{publicId}
 * @param publicId - Public ID of the image to delete
 * @returns Deletion status
 */
export const deleteImage = async (publicId: string): Promise<boolean> => {
    // Encode publicId to handle folder/filename structures
    const encodedPublicId = encodeURIComponent(publicId);

    const response = (await axiosClient.delete(
        `/api/upload/image/${encodedPublicId}`
    )) as ApiResponseDto<boolean>;

    if (!response.success) {
        throw new Error(response.message || 'Lỗi khi xóa ảnh');
    }

    return response.data ?? true;
};

/**
 * Get transformed image URL
 * GET /api/upload/transform/{publicId}
 * @param publicId - Public ID of the image
 * @param width - Target width in pixels
 * @param height - Target height in pixels
 * @param crop - Crop mode: fill, fit, scale, crop, thumb, pad, limit, mfit, lfill
 * @returns Transformed image URL
 */
export const getTransformedImageUrl = async (
    publicId: string,
    width?: number,
    height?: number,
    crop: string = 'fill'
): Promise<string> => {
    const encodedPublicId = encodeURIComponent(publicId);

    const params: Record<string, any> = { crop };
    if (width) params.width = width;
    if (height) params.height = height;

    const response = (await axiosClient.get(
        `/api/upload/transform/${encodedPublicId}`,
        { params }
    )) as ApiResponseDto<string>;

    if (!response.success || !response.data) {
        throw new Error(response.message || 'Lỗi khi lấy URL ảnh đã transform');
    }

    return response.data;
};

/**
 * Upload image for a specific entity type
 * POST /api/upload/entity/{entityType}
 * @param file - Image file to upload
 * @param entityType - Entity type: movie, actor, cinema, product, banner, user
 * @param entityId - Optional entity ID to organize images
 * @returns Uploaded image details
 */
export const uploadEntityImage = async (
    file: File,
    entityType: EntityType,
    entityId?: string
): Promise<UploadImageResponseDto> => {
    const formData = new FormData();
    formData.append('file', file);

    const response = (await axiosClient.post(
        `/api/upload/entity/${entityType}`,
        formData,
        {
            params: entityId ? { entityId } : undefined,
            headers: {
                'Content-Type': 'multipart/form-data',
            },
        }
    )) as ApiResponseDto<UploadImageResponseDto>;

    if (!response.success || !response.data) {
        throw new Error(response.message || `Lỗi khi upload ảnh cho ${entityType}`);
    }

    return response.data;
};

// ==================== HELPER FUNCTIONS ====================

/**
 * Validate file size
 * @param file - File to validate
 * @param maxSizeMB - Maximum size in MB (default: 10MB)
 * @returns true if valid, throws error otherwise
 */
export const validateFileSize = (file: File, maxSizeMB: number = 10): boolean => {
    const maxBytes = maxSizeMB * 1024 * 1024;
    if (file.size > maxBytes) {
        throw new Error(`Kích thước file không được vượt quá ${maxSizeMB}MB`);
    }
    return true;
};

/**
 * Validate file type (image only)
 * @param file - File to validate
 * @param allowedTypes - Allowed MIME types (default: common image types)
 * @returns true if valid, throws error otherwise
 */
export const validateFileType = (
    file: File,
    allowedTypes: string[] = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp']
): boolean => {
    if (!allowedTypes.includes(file.type)) {
        throw new Error(`Chỉ chấp nhận các định dạng: ${allowedTypes.join(', ')}`);
    }
    return true;
};

/**
 * Validate image file before upload
 * @param file - File to validate
 * @param maxSizeMB - Maximum size in MB (default: 10MB)
 * @returns true if valid, throws error otherwise
 */
export const validateImageFile = (file: File, maxSizeMB: number = 10): boolean => {
    validateFileType(file);
    validateFileSize(file, maxSizeMB);
    return true;
};

// ==================== EXPORT OBJECT ====================

/**
 * Object chứa tất cả các API functions cho Upload
 * Sử dụng pattern này để dễ import: import { uploadApi } from '@/services/apiUpload'
 */
export const uploadApi = {
    uploadImage,
    uploadMultipleImages,
    deleteImage,
    getTransformedImageUrl,
    uploadEntityImage,
    // Helper functions
    validateFileSize,
    validateFileType,
    validateImageFile,
};
