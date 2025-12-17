import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { ArrowLeft, Save, Loader2, Upload, X, ImageIcon } from "lucide-react";
import { toast } from "sonner";
import { cn } from "@/lib/utils";

// API
import { actorApi } from "@/services/apiActor";
import type { ActorCreateDto } from "@/services/apiActor";
import { PATHS } from "@/config/paths";

// UI Components
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";

const CreateActorPage = () => {
    const navigate = useNavigate();
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [imageFile, setImageFile] = useState<File | null>(null);
    const [imagePreview, setImagePreview] = useState<string>("");

    const [formData, setFormData] = useState<ActorCreateDto>({
        name: "",
        slug: "",
        description: "",
        imageUrl: "",
    });

    const [errors, setErrors] = useState<{ [key: string]: string }>({});

    const validateForm = (): boolean => {
        const newErrors: { [key: string]: string } = {};

        if (!formData.name.trim()) {
            newErrors.name = "Tên diễn viên là bắt buộc";
        }

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        if (!validateForm()) {
            toast.error("Vui lòng kiểm tra lại thông tin");
            return;
        }

        let createdActorId: string | null = null;

        try {
            setIsSubmitting(true);

            // BƯỚC 1: Tạo actor TRƯỚC (không có imageUrl từ file)
            toast.info("Đang tạo diễn viên...");
            const payload: ActorCreateDto = {
                name: formData.name.trim(),
                slug: formData.slug?.trim() || undefined,
                description: formData.description?.trim() || undefined,
                // Chỉ gửi imageUrl nếu user paste URL (không upload file)
                imageUrl: !imageFile && formData.imageUrl?.trim() ? formData.imageUrl.trim() : undefined,
            };

            const createdActor = await actorApi.create(payload);
            createdActorId = createdActor.id;

            console.log("Actor created with ID:", createdActorId);

            // BƯỚC 2: Upload image với actorId (nếu có file)
            if (imageFile && createdActorId) {
                toast.info("Đang upload ảnh...");

                const { uploadApi } = await import("@/services/apiUpload");
                const uploadResult = await uploadApi.uploadEntityImage(
                    imageFile,
                    "actor",
                    createdActorId
                );

                console.log("Image uploaded:", uploadResult.secureUrl);

                // BƯỚC 3: Cập nhật imageUrl cho actor
                toast.info("Đang cập nhật ảnh...");
                await actorApi.update(createdActorId, {
                    imageUrl: uploadResult.secureUrl,
                });

                console.log("Actor updated with imageUrl");
            }

            toast.success("Tạo diễn viên thành công");
            navigate(PATHS.ACTORS);
        } catch (error: any) {
            console.error("Error creating actor:", error);

            // Nếu đã tạo actor nhưng lỗi upload/update, thông báo cho user
            if (createdActorId) {
                toast.warning(
                    "Diễn viên đã được tạo nhưng có lỗi khi upload ảnh. Bạn có thể chỉnh sửa để thêm ảnh sau.",
                    { duration: 7000 }
                );
                navigate(PATHS.ACTORS);
                return;
            }

            toast.error(error.message || "Lỗi khi tạo diễn viên");
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleChange = (
        e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>
    ) => {
        const { name, value } = e.target;
        setFormData((prev) => ({ ...prev, [name]: value }));
        // Clear error when user starts typing
        if (errors[name]) {
            setErrors((prev) => ({ ...prev, [name]: "" }));
        }
    };

    return (
        <div className="flex-1 overflow-y-auto p-6 lg:p-10 bg-slate-50 dark:bg-[#0f172a] min-h-screen">
            <div className="max-w-4xl mx-auto space-y-6">
                {/* Header */}
                <div className="flex items-center gap-4">
                    <Button
                        variant="outline"
                        size="icon"
                        onClick={() => navigate(PATHS.ACTORS)}
                        className="bg-white dark:bg-slate-800 border-slate-200 dark:border-slate-700"
                    >
                        <ArrowLeft className="h-5 w-5" />
                    </Button>
                    <div>
                        <h1 className="text-3xl font-bold text-slate-900 dark:text-white">
                            Thêm Diễn viên Mới
                        </h1>
                        <p className="text-sm text-slate-500 dark:text-slate-400 mt-1">
                            Điền thông tin để tạo diễn viên mới
                        </p>
                    </div>
                </div>

                {/* Form */}
                <form onSubmit={handleSubmit}>
                    <Card className="bg-white dark:bg-[#1e293b] border-slate-200 dark:border-slate-700">
                        <CardHeader>
                            <CardTitle>Thông tin Diễn viên</CardTitle>
                            <CardDescription>
                                Các trường đánh dấu * là bắt buộc
                            </CardDescription>
                        </CardHeader>
                        <CardContent className="space-y-6">
                            {/* Name */}
                            <div className="space-y-2">
                                <Label htmlFor="name" className="text-sm font-medium">
                                    Tên diễn viên <span className="text-red-500">*</span>
                                </Label>
                                <Input
                                    id="name"
                                    name="name"
                                    value={formData.name}
                                    onChange={handleChange}
                                    placeholder="VD: Tom Cruise"
                                    className={errors.name ? "border-red-500" : ""}
                                />
                                {errors.name && (
                                    <p className="text-xs text-red-500">{errors.name}</p>
                                )}
                            </div>

                            {/* Slug */}
                            <div className="space-y-2">
                                <Label htmlFor="slug" className="text-sm font-medium">
                                    Slug
                                </Label>
                                <Input
                                    id="slug"
                                    name="slug"
                                    value={formData.slug}
                                    onChange={handleChange}
                                    placeholder="VD: tom-cruise (tự động tạo nếu để trống)"
                                />
                                <p className="text-xs text-muted-foreground">
                                    URL thân thiện. Để trống để hệ thống tự tạo
                                </p>
                            </div>

                            {/* Description */}
                            <div className="space-y-2">
                                <Label htmlFor="description" className="text-sm font-medium">
                                    Mô tả
                                </Label>
                                <Textarea
                                    id="description"
                                    name="description"
                                    value={formData.description}
                                    onChange={handleChange}
                                    placeholder="Mô tả về diễn viên..."
                                    rows={5}
                                />
                            </div>

                            {/* Image Upload - File or URL */}
                            <div className="space-y-2">
                                <Label className="text-sm font-medium">
                                    Ảnh diễn viên
                                </Label>

                                {/* Tab-like choice: Upload or URL */}
                                <div className="space-y-4">
                                    {/* Upload File Section */}
                                    <div
                                        className={cn(
                                            "relative rounded-lg border-2 border-dashed transition-colors",
                                            imageFile
                                                ? "border-solid"
                                                : "border-muted-foreground/25"
                                        )}
                                    >
                                        <input
                                            type="file"
                                            accept="image/jpeg,image/jpg,image/png,image/gif,image/webp"
                                            onChange={(e) => {
                                                const file = e.target.files?.[0];
                                                if (file) {
                                                    // Validate file
                                                    const maxSizeMB = 10;
                                                    const maxBytes = maxSizeMB * 1024 * 1024;
                                                    if (file.size > maxBytes) {
                                                        toast.error(
                                                            `Kích thước file không được vượt quá ${maxSizeMB}MB`
                                                        );
                                                        return;
                                                    }

                                                    const allowedTypes = [
                                                        "image/jpeg",
                                                        "image/jpg",
                                                        "image/png",
                                                        "image/gif",
                                                        "image/webp",
                                                    ];
                                                    if (!allowedTypes.includes(file.type)) {
                                                        toast.error(
                                                            "Chỉ chấp nhận các định dạng: JPG, PNG, GIF, WEBP"
                                                        );
                                                        return;
                                                    }

                                                    // Save file for later upload
                                                    setImageFile(file);

                                                    // Clear URL if user uploads file
                                                    setFormData((prev) => ({ ...prev, imageUrl: "" }));

                                                    // Create preview
                                                    const reader = new FileReader();
                                                    reader.onloadend = () => {
                                                        setImagePreview(reader.result as string);
                                                    };
                                                    reader.readAsDataURL(file);
                                                }
                                            }}
                                            className="hidden"
                                            id="image-upload"
                                        />

                                        {imageFile ? (
                                            <div className="relative group">
                                                <img
                                                    src={imagePreview}
                                                    alt="Preview"
                                                    className="w-full h-auto max-h-64 rounded-lg object-cover"
                                                />
                                                <div className="absolute inset-0 bg-black/50 opacity-0 group-hover:opacity-100 transition-opacity rounded-lg flex items-center justify-center gap-2">
                                                    <Button
                                                        type="button"
                                                        size="sm"
                                                        variant="secondary"
                                                        onClick={() => {
                                                            document
                                                                .getElementById("image-upload")
                                                                ?.click();
                                                        }}
                                                    >
                                                        <Upload className="h-4 w-4 mr-2" />
                                                        Thay đổi
                                                    </Button>
                                                    <Button
                                                        type="button"
                                                        size="sm"
                                                        variant="destructive"
                                                        onClick={() => {
                                                            setImageFile(null);
                                                            setImagePreview("");
                                                        }}
                                                    >
                                                        <X className="h-4 w-4 mr-2" />
                                                        Xóa
                                                    </Button>
                                                </div>
                                            </div>
                                        ) : (
                                            <label
                                                htmlFor="image-upload"
                                                className="w-full p-8 flex flex-col items-center justify-center gap-2 text-muted-foreground hover:text-foreground hover:bg-muted/50 transition-colors rounded-lg cursor-pointer"
                                            >
                                                <ImageIcon className="h-12 w-12" />
                                                <div className="text-center">
                                                    <p className="text-sm font-medium">
                                                        Click để chọn ảnh
                                                    </p>
                                                    <p className="text-xs mt-1">
                                                        PNG, JPG, GIF, WEBP (tối đa 10MB)
                                                    </p>
                                                </div>
                                            </label>
                                        )}
                                    </div>

                                    {/* OR Divider */}
                                    {!imageFile && (
                                        <div className="relative">
                                            <div className="absolute inset-0 flex items-center">
                                                <span className="w-full border-t" />
                                            </div>
                                            <div className="relative flex justify-center text-xs uppercase">
                                                <span className="bg-background px-2 text-muted-foreground">
                                                    Hoặc
                                                </span>
                                            </div>
                                        </div>
                                    )}

                                    {/* URL Input Section */}
                                    {!imageFile && (
                                        <div className="space-y-2">
                                            <Label htmlFor="imageUrl" className="text-sm font-medium">
                                                Hoặc nhập URL ảnh
                                            </Label>
                                            <Input
                                                id="imageUrl"
                                                name="imageUrl"
                                                type="url"
                                                value={formData.imageUrl}
                                                onChange={handleChange}
                                                placeholder="https://example.com/image.jpg"
                                            />
                                            {formData.imageUrl && (
                                                <div className="mt-3">
                                                    <p className="text-xs text-muted-foreground mb-2">
                                                        Xem trước:
                                                    </p>
                                                    <img
                                                        src={formData.imageUrl}
                                                        alt="Preview"
                                                        className="h-32 w-32 rounded-lg object-cover border border-slate-200 dark:border-slate-700"
                                                        onError={(e) => {
                                                            (e.target as HTMLImageElement).src =
                                                                "https://placehold.co/200/e2e8f0/64748b?text=Invalid+URL";
                                                        }}
                                                    />
                                                </div>
                                            )}
                                        </div>
                                    )}

                                    {imageFile && (
                                        <p className="text-sm text-muted-foreground">
                                            Ảnh sẽ được upload sau khi tạo diễn viên
                                        </p>
                                    )}
                                </div>
                            </div>
                        </CardContent>
                    </Card>

                    {/* Actions */}
                    <div className="flex items-center justify-end gap-4 mt-6">
                        <Button
                            type="button"
                            variant="outline"
                            onClick={() => navigate(PATHS.ACTORS)}
                            disabled={isSubmitting}
                        >
                            Hủy
                        </Button>
                        <Button
                            type="submit"
                            disabled={isSubmitting}
                            className="gap-2"
                        >
                            {isSubmitting ? (
                                <>
                                    <Loader2 className="h-4 w-4 animate-spin" />
                                    Đang tạo...
                                </>
                            ) : (
                                <>
                                    <Save className="h-4 w-4" />
                                    Tạo diễn viên
                                </>
                            )}
                        </Button>
                    </div>
                </form>
            </div>
        </div>
    );
};

export default CreateActorPage;
