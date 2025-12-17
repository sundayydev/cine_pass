import { useState, useEffect } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";
import { useNavigate, useParams } from "react-router-dom";
import { ArrowLeft, Loader2, Upload, X } from "lucide-react";
import { toast } from "sonner";

// API Services
import { productApi, type ProductUpdateDto } from "@/services/apiProduct";

// Shadcn UI
import { Button } from "@/components/ui/button";
import {
    Form,
    FormControl,
    FormField,
    FormItem,
    FormLabel,
    FormMessage,
    FormDescription,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/select";
import { Switch } from "@/components/ui/switch";

// Product Category
export const ProductCategory = {
    Food: 0,
    Drink: 1,
    Combo: 2,
    Other: 3
} as const;

// Schema validation
const productSchema = z.object({
    name: z.string().min(1, "Tên sản phẩm không được để trống").max(255, "Tên sản phẩm tối đa 255 ký tự"),
    description: z.string().max(1000, "Mô tả tối đa 1000 ký tự").optional(),
    price: z.number().min(1000, "Giá tối thiểu 1,000đ").max(100000000, "Giá tối đa 100,000,000đ"),
    imageUrl: z.string().url("URL ảnh không hợp lệ").optional().or(z.literal("")),
    category: z.number().min(0).max(3),
    isActive: z.boolean(),
});

type ProductFormValues = z.infer<typeof productSchema>;

const EditProductPage = () => {
    const navigate = useNavigate();
    const { id } = useParams<{ id: string }>();
    const [isLoading, setIsLoading] = useState(false);
    const [isLoadingData, setIsLoadingData] = useState(true);

    const form = useForm<ProductFormValues>({
        resolver: zodResolver(productSchema),
        defaultValues: {
            name: "",
            description: "",
            price: 0,
            imageUrl: "",
            category: ProductCategory.Food,
            isActive: true,
        },
    });

    // Load product data
    useEffect(() => {
        if (id) {
            loadProduct(id);
        }
    }, [id]);

    const loadProduct = async (productId: string) => {
        try {
            setIsLoadingData(true);
            const product = await productApi.getById(productId);
            form.reset({
                name: product.name,
                description: product.description || "",
                price: product.price,
                imageUrl: product.imageUrl || "",
                category: product.category,
                isActive: product.isActive,
            });
        } catch (error) {
            console.error("Error loading product:", error);
            toast.error("Lỗi khi tải thông tin sản phẩm");
            navigate("/products");
        } finally {
            setIsLoadingData(false);
        }
    };

    const onSubmit = async (data: ProductFormValues) => {
        if (!id) return;

        setIsLoading(true);
        try {
            const dto: ProductUpdateDto = {
                name: data.name,
                description: data.description || undefined,
                price: data.price,
                imageUrl: data.imageUrl || undefined,
                category: data.category,
                isActive: data.isActive,
            };

            await productApi.update(id, dto);
            toast.success("Cập nhật sản phẩm thành công");
            navigate("/products");
        } catch (error) {
            console.error("Error updating product:", error);
            toast.error(error instanceof Error ? error.message : "Lỗi khi cập nhật sản phẩm");
        } finally {
            setIsLoading(false);
        }
    };

    // Format price for display
    const formatPrice = (price: number) => {
        return new Intl.NumberFormat('vi-VN').format(price);
    };

    if (isLoadingData) {
        return (
            <div className="flex items-center justify-center min-h-[60vh]">
                <div className="flex flex-col items-center gap-4">
                    <Loader2 className="h-8 w-8 animate-spin text-primary" />
                    <p className="text-muted-foreground">Đang tải thông tin sản phẩm...</p>
                </div>
            </div>
        );
    }

    return (
        <div className="space-y-6">
            {/* Header */}
            <div className="flex items-center gap-4">
                <Button variant="ghost" size="icon" onClick={() => navigate("/products")}>
                    <ArrowLeft className="h-4 w-4" />
                </Button>
                <div>
                    <h1 className="text-3xl font-bold tracking-tight bg-gradient-to-r from-primary to-orange-600 bg-clip-text text-transparent">
                        Chỉnh sửa Sản phẩm
                    </h1>
                    <p className="text-muted-foreground mt-1">Cập nhật thông tin sản phẩm</p>
                </div>
            </div>

            {/* Form */}
            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                {/* Main Form */}
                <Card className="lg:col-span-2">
                    <CardHeader>
                        <CardTitle>Thông tin Sản phẩm</CardTitle>
                        <CardDescription>Chỉnh sửa thông tin sản phẩm bên dưới</CardDescription>
                    </CardHeader>
                    <CardContent>
                        <Form {...form}>
                            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
                                {/* Tên sản phẩm */}
                                <FormField
                                    control={form.control}
                                    name="name"
                                    render={({ field }) => (
                                        <FormItem>
                                            <FormLabel>Tên Sản phẩm *</FormLabel>
                                            <FormControl>
                                                <Input
                                                    placeholder="Ví dụ: Bắp rang bơ lớn"
                                                    {...field}
                                                />
                                            </FormControl>
                                            <FormDescription>
                                                Tên hiển thị của sản phẩm (tối đa 255 ký tự)
                                            </FormDescription>
                                            <FormMessage />
                                        </FormItem>
                                    )}
                                />

                                {/* Mô tả */}
                                <FormField
                                    control={form.control}
                                    name="description"
                                    render={({ field }) => (
                                        <FormItem>
                                            <FormLabel>Mô tả</FormLabel>
                                            <FormControl>
                                                <Textarea
                                                    placeholder="Mô tả chi tiết sản phẩm..."
                                                    className="resize-none min-h-[100px]"
                                                    {...field}
                                                />
                                            </FormControl>
                                            <FormDescription>
                                                Mô tả ngắn gọn về sản phẩm (tùy chọn, tối đa 1000 ký tự)
                                            </FormDescription>
                                            <FormMessage />
                                        </FormItem>
                                    )}
                                />

                                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                                    {/* Danh mục */}
                                    <FormField
                                        control={form.control}
                                        name="category"
                                        render={({ field }) => (
                                            <FormItem>
                                                <FormLabel>Danh mục *</FormLabel>
                                                <Select
                                                    onValueChange={(value) => field.onChange(parseInt(value))}
                                                    value={field.value.toString()}
                                                >
                                                    <FormControl>
                                                        <SelectTrigger>
                                                            <SelectValue placeholder="Chọn danh mục" />
                                                        </SelectTrigger>
                                                    </FormControl>
                                                    <SelectContent>
                                                        <SelectItem value="0">🍿 Đồ ăn</SelectItem>
                                                        <SelectItem value="1">☕ Đồ uống</SelectItem>
                                                        <SelectItem value="2">🎁 Combo</SelectItem>
                                                        <SelectItem value="3">📦 Khác</SelectItem>
                                                    </SelectContent>
                                                </Select>
                                                <FormDescription>
                                                    Phân loại sản phẩm
                                                </FormDescription>
                                                <FormMessage />
                                            </FormItem>
                                        )}
                                    />

                                    {/* Giá */}
                                    <FormField
                                        control={form.control}
                                        name="price"
                                        render={({ field }) => (
                                            <FormItem>
                                                <FormLabel>Giá bán *</FormLabel>
                                                <FormControl>
                                                    <div className="relative">
                                                        <Input
                                                            type="number"
                                                            min="1000"
                                                            step="1000"
                                                            placeholder="50000"
                                                            {...field}
                                                            onChange={(e) => {
                                                                const value = e.target.value;
                                                                field.onChange(value === "" ? 0 : parseInt(value));
                                                            }}
                                                        />
                                                        <span className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground text-sm">
                                                            VNĐ
                                                        </span>
                                                    </div>
                                                </FormControl>
                                                <FormDescription>
                                                    Giá bán sản phẩm (tối thiểu 1,000đ)
                                                </FormDescription>
                                                <FormMessage />
                                            </FormItem>
                                        )}
                                    />
                                </div>

                                {/* URL Ảnh */}
                                <FormField
                                    control={form.control}
                                    name="imageUrl"
                                    render={({ field }) => (
                                        <FormItem>
                                            <FormLabel>URL Ảnh sản phẩm</FormLabel>
                                            <FormControl>
                                                <div className="flex gap-2">
                                                    <Input
                                                        placeholder="https://example.com/image.jpg"
                                                        {...field}
                                                    />
                                                    {field.value && (
                                                        <Button
                                                            type="button"
                                                            variant="outline"
                                                            size="icon"
                                                            onClick={() => field.onChange("")}
                                                        >
                                                            <X className="h-4 w-4" />
                                                        </Button>
                                                    )}
                                                </div>
                                            </FormControl>
                                            <FormDescription>
                                                Đường dẫn URL đến ảnh sản phẩm (tùy chọn)
                                            </FormDescription>
                                            <FormMessage />
                                        </FormItem>
                                    )}
                                />

                                {/* Trạng thái */}
                                <FormField
                                    control={form.control}
                                    name="isActive"
                                    render={({ field }) => (
                                        <FormItem className="flex flex-row items-center justify-between rounded-lg border p-4">
                                            <div className="space-y-0.5">
                                                <FormLabel className="text-base">Đang bán</FormLabel>
                                                <FormDescription>
                                                    Bật để hiển thị sản phẩm cho khách hàng
                                                </FormDescription>
                                            </div>
                                            <FormControl>
                                                <Switch
                                                    checked={field.value}
                                                    onCheckedChange={field.onChange}
                                                />
                                            </FormControl>
                                        </FormItem>
                                    )}
                                />

                                {/* Actions */}
                                <div className="flex items-center justify-end gap-4">
                                    <Button
                                        type="button"
                                        variant="outline"
                                        onClick={() => navigate("/products")}
                                        disabled={isLoading}
                                    >
                                        Hủy
                                    </Button>
                                    <Button type="submit" disabled={isLoading}>
                                        {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                                        Cập nhật Sản phẩm
                                    </Button>
                                </div>
                            </form>
                        </Form>
                    </CardContent>
                </Card>

                {/* Preview Card */}
                <Card className="h-fit sticky top-6">
                    <CardHeader>
                        <CardTitle className="text-lg">Xem trước</CardTitle>
                        <CardDescription>Preview sản phẩm</CardDescription>
                    </CardHeader>
                    <CardContent>
                        <div className="space-y-4">
                            {/* Image Preview */}
                            <div className="aspect-square rounded-lg border bg-muted/50 overflow-hidden">
                                {form.watch("imageUrl") ? (
                                    <img
                                        src={form.watch("imageUrl")}
                                        alt="Preview"
                                        className="w-full h-full object-cover"
                                        onError={(e) => {
                                            (e.target as HTMLImageElement).style.display = 'none';
                                        }}
                                    />
                                ) : (
                                    <div className="w-full h-full flex items-center justify-center text-muted-foreground">
                                        <Upload className="h-12 w-12" />
                                    </div>
                                )}
                            </div>

                            {/* Info Preview */}
                            <div className="space-y-2">
                                <h3 className="font-semibold text-lg line-clamp-2">
                                    {form.watch("name") || "Tên sản phẩm"}
                                </h3>
                                {form.watch("description") && (
                                    <p className="text-sm text-muted-foreground line-clamp-2">
                                        {form.watch("description")}
                                    </p>
                                )}
                                <div className="flex items-center justify-between pt-2">
                                    <span className="text-lg font-bold text-emerald-600">
                                        {formatPrice(form.watch("price") || 0)}đ
                                    </span>
                                    {form.watch("isActive") ? (
                                        <span className="text-xs px-2 py-1 rounded-full bg-emerald-100 text-emerald-700">
                                            Đang bán
                                        </span>
                                    ) : (
                                        <span className="text-xs px-2 py-1 rounded-full bg-slate-100 text-slate-600">
                                            Ngưng bán
                                        </span>
                                    )}
                                </div>
                            </div>
                        </div>
                    </CardContent>
                </Card>
            </div>
        </div>
    );
};

export default EditProductPage;
