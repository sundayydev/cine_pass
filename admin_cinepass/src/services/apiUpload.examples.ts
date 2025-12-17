/**
 * Ví dụ sử dụng Upload API trong React component
 * 
 * File này chứa các ví dụ về cách sử dụng uploadApi trong các component React
 */

import { uploadApi, ENTITY_TYPES } from './apiUpload';
import type { UploadImageResponseDto } from './apiUpload';

// ==================== VÍ DỤ 1: UPLOAD ẢNH ĐƠN ====================

/**
 * Ví dụ: Upload ảnh poster cho phim
 */
export const handleMoviePosterUpload = async (file: File): Promise<string> => {
    try {
        // Validate file trước khi upload
        uploadApi.validateImageFile(file, 10); // Max 10MB

        // Upload ảnh vào folder movies
        const result = await uploadApi.uploadImage(file, 'movies');

        console.log('Upload thành công:', result);

        // Trả về secure URL để lưu vào database
        return result.secureUrl;
    } catch (error) {
        console.error('Lỗi upload ảnh:', error);
        throw error;
    }
};

// ==================== VÍ DỤ 2: UPLOAD NHIỀU ẢNH ====================

/**
 * Ví dụ: Upload nhiều ảnh cho gallery
 */
export const handleGalleryUpload = async (files: File[]): Promise<string[]> => {
    try {
        // Validate từng file
        files.forEach((file) => uploadApi.validateImageFile(file, 5)); // Max 5MB mỗi file

        // Upload tất cả ảnh
        const result = await uploadApi.uploadMultipleImages(files, 'gallery');

        console.log(`Upload thành công ${result.successCount}/${files.length} ảnh`);

        if (result.failedCount > 0) {
            console.warn('Một số ảnh upload thất bại:', result.errors);
        }

        // Trả về danh sách secure URLs
        return result.images.map((img) => img.secureUrl);
    } catch (error) {
        console.error('Lỗi upload nhiều ảnh:', error);
        throw error;
    }
};

// ==================== VÍ DỤ 3: UPLOAD ẢNH CHO ENTITY CỤ THỂ ====================

/**
 * Ví dụ: Upload ảnh cho movie với ID cụ thể
 */
export const handleMovieImageUpload = async (
    file: File,
    movieId: string
): Promise<UploadImageResponseDto> => {
    try {
        uploadApi.validateImageFile(file);

        // Upload vào folder: cinepass/movie/{movieId}
        const result = await uploadApi.uploadEntityImage(
            file,
            ENTITY_TYPES.MOVIE,
            movieId
        );

        return result;
    } catch (error) {
        console.error('Lỗi upload ảnh movie:', error);
        throw error;
    }
};

// ==================== VÍ DỤ 4: XÓA ẢNH ====================

/**
 * Ví dụ: Xóa ảnh từ Cloudinary
 */
export const handleDeleteImage = async (publicId: string): Promise<void> => {
    try {
        await uploadApi.deleteImage(publicId);
        console.log('Đã xóa ảnh thành công');
    } catch (error) {
        console.error('Lỗi xóa ảnh:', error);
        throw error;
    }
};

// ==================== VÍ DỤ 5: LẤY URL ẢNH ĐÃ TRANSFORM ====================

/**
 * Ví dụ: Lấy URL ảnh đã resize
 */
export const getResizedImageUrl = async (
    publicId: string,
    width: number,
    height: number
): Promise<string> => {
    try {
        const transformedUrl = await uploadApi.getTransformedImageUrl(
            publicId,
            width,
            height,
            'fill' // crop mode
        );

        return transformedUrl;
    } catch (error) {
        console.error('Lỗi lấy URL transform:', error);
        throw error;
    }
};

// ==================== VÍ DỤ 6: REACT COMPONENT VỚI UPLOAD ====================

/**
 * Ví dụ React Component với input file upload
 *
 * Usage trong component:
 *
 * ```tsx
 * import { useState } from 'react';
 * import { uploadApi } from '@/services/apiUpload';
 *
 * const MoviePosterUpload = () => {
 *   const [uploading, setUploading] = useState(false);
 *   const [posterUrl, setPosterUrl] = useState<string>('');
 *   const [error, setError] = useState<string>('');
 *
 *   const handleFileChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
 *     const file = e.target.files?.[0];
 *     if (!file) return;
 *
 *     try {
 *       setUploading(true);
 *       setError('');
 *
 *       // Validate
 *       uploadApi.validateImageFile(file, 10);
 *
 *       // Upload
 *       const result = await uploadApi.uploadEntityImage(
 *         file,
 *         'movie',
 *         'movie-123' // hoặc lấy từ movieId
 *       );
 *
 *       setPosterUrl(result.secureUrl);
 *       console.log('Upload thành công:', result);
 *     } catch (err) {
 *       setError(err instanceof Error ? err.message : 'Lỗi upload');
 *       console.error(err);
 *     } finally {
 *       setUploading(false);
 *     }
 *   };
 *
 *   return (
 *     <div>
 *       <input
 *         type="file"
 *         accept="image/*"
 *         onChange={handleFileChange}
 *         disabled={uploading}
 *       />
 *       {uploading && <p>Đang upload...</p>}
 *       {error && <p style={{ color: 'red' }}>{error}</p>}
 *       {posterUrl && (
 *         <div>
 *           <p>Upload thành công!</p>
 *           <img src={posterUrl} alt="Poster" style={{ maxWidth: '200px' }} />
 *         </div>
 *       )}
 *     </div>
 *   );
 * };
 * ```
 */

// ==================== VÍ DỤ 7: UPLOAD VỚI PREVIEW ====================

/**
 * Ví dụ: Upload với preview trước khi gửi
 *
 * ```tsx
 * const ImageUploadWithPreview = () => {
 *   const [file, setFile] = useState<File | null>(null);
 *   const [preview, setPreview] = useState<string>('');
 *   const [uploading, setUploading] = useState(false);
 *
 *   const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
 *     const selectedFile = e.target.files?.[0];
 *     if (!selectedFile) return;
 *
 *     // Validate
 *     try {
 *       uploadApi.validateImageFile(selectedFile, 10);
 *       setFile(selectedFile);
 *
 *       // Create preview
 *       const reader = new FileReader();
 *       reader.onloadend = () => {
 *         setPreview(reader.result as string);
 *       };
 *       reader.readAsDataURL(selectedFile);
 *     } catch (err) {
 *       alert(err instanceof Error ? err.message : 'File không hợp lệ');
 *     }
 *   };
 *
 *   const handleUpload = async () => {
 *     if (!file) return;
 *
 *     try {
 *       setUploading(true);
 *       const result = await uploadApi.uploadImage(file);
 *       console.log('Uploaded:', result.secureUrl);
 *       // Lưu result.secureUrl vào state hoặc gửi lên backend
 *     } catch (err) {
 *       console.error(err);
 *     } finally {
 *       setUploading(false);
 *     }
 *   };
 *
 *   return (
 *     <div>
 *       <input type="file" accept="image/*" onChange={handleFileSelect} />
 *       {preview && <img src={preview} alt="Preview" style={{ maxWidth: '300px' }} />}
 *       {file && (
 *         <button onClick={handleUpload} disabled={uploading}>
 *           {uploading ? 'Đang upload...' : 'Upload'}
 *         </button>
 *       )}
 *     </div>
 *   );
 * };
 * ```
 */

// ==================== VÍ DỤ 8: DRAG AND DROP UPLOAD ====================

/**
 * Ví dụ: Upload với drag and drop
 * 
 * ```tsx
 * const DragDropUpload = () => {
 *   const [isDragging, setIsDragging] = useState(false);
 * 
 *   const handleDragOver = (e: React.DragEvent) => {
 *     e.preventDefault();
 *     setIsDragging(true);
 *   };
 * 
 *   const handleDragLeave = () => {
 *     setIsDragging(false);
 *   };
 * 
 *   const handleDrop = async (e: React.DragEvent) => {
 *     e.preventDefault();
 *     setIsDragging(false);
 * 
 *     const files = Array.from(e.dataTransfer.files);
 *     const imageFiles = files.filter((file) => file.type.startsWith('image/'));
 * 
 *     if (imageFiles.length === 0) {
 *       alert('Vui lòng chọn file ảnh');
 *       return;
 *     }
 * 
 *     try {
 *       // Upload multiple images
 *       const result = await uploadApi.uploadMultipleImages(imageFiles);
 *       console.log('Uploaded:', result);
 *     } catch (err) {
 *       console.error(err);
 *     }
 *   };
 * 
 *   return (
 *     <div
 *       onDragOver={handleDragOver}
 *       onDragLeave={handleDragLeave}
 *       onDrop={handleDrop}
 *       style={{
 *         border: `2px dashed ${isDragging ? 'blue' : 'gray'}`,
 *         padding: '50px',
 *         textAlign: 'center',
 *       }}
 *     >
 *       {isDragging ? 'Thả ảnh vào đây' : 'Kéo ảnh vào đây để upload'}
 *     </div>
 *   );
 * };
 * ```
 */
