import { useState, useRef } from 'react';
import { Upload, X, Loader2, ImageIcon } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { cn } from '@/lib/utils';
import { uploadApi, ENTITY_TYPES, type EntityType } from '@/services/apiUpload';
import { toast } from 'sonner';

// Re-export for convenience
export { ENTITY_TYPES };
export type { EntityType };

interface ImageUploadProps {
    /**
     * Callback khi upload thành công (trả về URL của ảnh)
     */
    onUploadSuccess: (url: string, publicId: string) => void;

    /**
     * Callback khi xóa ảnh
     */
    onRemove?: () => void;

    /**
     * URL ảnh hiện tại (nếu có)
     */
    value?: string;

    /**
     * Entity type để organize ảnh vào folder phù hợp
     */
    entityType?: EntityType;

    /**
     * Entity ID để organize ảnh vào subfolder
     */
    entityId?: string;

    /**
     * Custom folder name (nếu không dùng entity type)
     */
    folder?: string;

    /**
     * Kích thước tối đa (MB), mặc định 10MB
     */
    maxSizeMB?: number;

    /**
     * Disable upload
     */
    disabled?: boolean;

    /**
     * Hiển thị preview ảnh
     */
    showPreview?: boolean;

    /**
     * Custom className
     */
    className?: string;

    /**
     * Label hiển thị
     */
    label?: string;

    /**
     * Mô tả (description)
     */
    description?: string;
}

/**
 * Component upload ảnh với preview và validation
 * 
 * Usage:
 * ```tsx
 * <ImageUpload
 *   entityType={ENTITY_TYPES.MOVIE}
 *   entityId="movie-123"
 *   onUploadSuccess={(url, publicId) => {
 *     console.log('Uploaded:', url, publicId);
 *     setMoviePosterUrl(url);
 *   }}
 *   value={moviePosterUrl}
 *   label="Poster Phim"
 * />
 * ```
 */
export const ImageUpload: React.FC<ImageUploadProps> = ({
    onUploadSuccess,
    onRemove,
    value,
    entityType,
    entityId,
    folder,
    maxSizeMB = 10,
    disabled = false,
    showPreview = true,
    className,
    label = 'Upload ảnh',
    description,
}) => {
    const [isUploading, setIsUploading] = useState(false);
    const [preview, setPreview] = useState<string>(value || '');
    const fileInputRef = useRef<HTMLInputElement>(null);

    const handleFileSelect = async (event: React.ChangeEvent<HTMLInputElement>) => {
        const file = event.target.files?.[0];
        if (!file) return;

        try {
            // Validate file
            uploadApi.validateImageFile(file, maxSizeMB);

            // Create preview
            const reader = new FileReader();
            reader.onloadend = () => {
                setPreview(reader.result as string);
            };
            reader.readAsDataURL(file);

            // Upload
            setIsUploading(true);
            let result;

            if (entityType) {
                // Upload với entity type (sẽ tự động organize vào folder)
                result = await uploadApi.uploadEntityImage(file, entityType, entityId);
            } else if (folder) {
                // Upload với custom folder
                result = await uploadApi.uploadImage(file, folder);
            } else {
                // Upload mặc định
                result = await uploadApi.uploadImage(file);
            }

            // Callback success
            onUploadSuccess(result.secureUrl, result.publicId);
            setPreview(result.secureUrl);

            toast.success('Upload ảnh thành công!');
        } catch (error) {
            console.error('Upload error:', error);
            const errorMessage = error instanceof Error ? error.message : 'Lỗi upload ảnh';
            toast.error(errorMessage);

            // Reset preview nếu upload thất bại
            setPreview(value || '');
        } finally {
            setIsUploading(false);
            // Reset input để có thể upload lại cùng một file
            if (fileInputRef.current) {
                fileInputRef.current.value = '';
            }
        }
    };

    const handleRemove = () => {
        setPreview('');
        if (onRemove) {
            onRemove();
        }
        if (fileInputRef.current) {
            fileInputRef.current.value = '';
        }
    };

    const handleClick = () => {
        fileInputRef.current?.click();
    };

    return (
        <div className={cn('space-y-2', className)}>
            {label && (
                <label className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70">
                    {label}
                </label>
            )}

            <div
                className={cn(
                    'relative rounded-lg border-2 border-dashed transition-colors',
                    preview ? 'border-solid' : 'border-muted-foreground/25',
                    disabled && 'opacity-50 cursor-not-allowed'
                )}
            >
                {/* Hidden file input */}
                <input
                    ref={fileInputRef}
                    type="file"
                    accept="image/jpeg,image/jpg,image/png,image/gif,image/webp"
                    onChange={handleFileSelect}
                    className="hidden"
                    disabled={disabled || isUploading}
                />

                {/* Preview / Upload Area */}
                {showPreview && preview ? (
                    <div className="relative group">
                        <img
                            src={preview}
                            alt="Preview"
                            className="w-full h-auto rounded-lg object-cover"
                        />

                        {/* Overlay with actions */}
                        <div className="absolute inset-0 bg-black/50 opacity-0 group-hover:opacity-100 transition-opacity rounded-lg flex items-center justify-center gap-2">
                            <Button
                                type="button"
                                size="sm"
                                variant="secondary"
                                onClick={handleClick}
                                disabled={disabled || isUploading}
                            >
                                <Upload className="h-4 w-4 mr-2" />
                                Thay đổi
                            </Button>
                            <Button
                                type="button"
                                size="sm"
                                variant="destructive"
                                onClick={handleRemove}
                                disabled={disabled || isUploading}
                            >
                                <X className="h-4 w-4 mr-2" />
                                Xóa
                            </Button>
                        </div>

                        {/* Loading overlay */}
                        {isUploading && (
                            <div className="absolute inset-0 bg-black/70 flex items-center justify-center rounded-lg">
                                <Loader2 className="h-8 w-8 text-white animate-spin" />
                            </div>
                        )}
                    </div>
                ) : (
                    <button
                        type="button"
                        onClick={handleClick}
                        disabled={disabled || isUploading}
                        className={cn(
                            'w-full p-8 flex flex-col items-center justify-center gap-2 text-muted-foreground hover:text-foreground hover:bg-muted/50 transition-colors rounded-lg',
                            disabled && 'cursor-not-allowed'
                        )}
                    >
                        {isUploading ? (
                            <>
                                <Loader2 className="h-12 w-12 animate-spin" />
                                <p className="text-sm">Đang upload...</p>
                            </>
                        ) : (
                            <>
                                <ImageIcon className="h-12 w-12" />
                                <div className="text-center">
                                    <p className="text-sm font-medium">Click để chọn ảnh</p>
                                    <p className="text-xs mt-1">
                                        PNG, JPG, GIF, WEBP (tối đa {maxSizeMB}MB)
                                    </p>
                                </div>
                            </>
                        )}
                    </button>
                )}
            </div>

            {description && (
                <p className="text-sm text-muted-foreground">{description}</p>
            )}
        </div>
    );
};
