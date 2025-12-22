import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import {
    Plus,
    Search,
    Pencil,
    Trash2,
    Package,
    AlertCircle,
    Popcorn,
    Coffee,
    Gift,
    ToggleLeft,
    ToggleRight,
    Filter
} from "lucide-react";
import { toast } from "sonner";

// API Services
import { productApi, type ProductResponseDto } from "@/services/apiProduct";

// Shadcn UI
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "@/components/ui/table";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle } from "@/components/ui/alert-dialog";
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "@/components/ui/tooltip";
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/select";

// Hooks
import { useDebounce } from "@/hooks/useDebounce";
import { cn } from "@/lib/utils";

// Product Category (khớp với backend - trả về string)
export const ProductCategory = {
    Food: "Food",
    Drink: "Drink",
    Combo: "Combo"
} as const;

const ProductsListPage = () => {
    const navigate = useNavigate();

    // State
    const [products, setProducts] = useState<ProductResponseDto[]>([]);
    const [isLoading, setIsLoading] = useState(false);
    const [searchTerm, setSearchTerm] = useState("");
    const debouncedSearch = useDebounce(searchTerm, 500);
    const [categoryFilter, setCategoryFilter] = useState<string>("all");
    const [itemToDelete, setItemToDelete] = useState<{ id: string; name: string } | null>(null);
    const [isDeleting, setIsDeleting] = useState(false);

    // Load products
    useEffect(() => {
        loadProducts();
    }, []);

    useEffect(() => {
        loadProducts();
    }, [debouncedSearch, categoryFilter]);

    const loadProducts = async () => {
        try {
            setIsLoading(true);
            let data: ProductResponseDto[];

            // Filter by category if selected
            if (categoryFilter !== "all") {
                data = await productApi.getByCategory(categoryFilter);
            } else {
                data = await productApi.getAll();
            }

            // Apply search filter
            if (debouncedSearch) {
                data = data.filter(
                    (product) =>
                        product.name.toLowerCase().includes(debouncedSearch.toLowerCase()) ||
                        (product.description && product.description.toLowerCase().includes(debouncedSearch.toLowerCase()))
                );
            }

            setProducts(data);
        } catch (error) {
            console.error("Error loading products:", error);
            toast.error("Lỗi khi tải danh sách sản phẩm");
        } finally {
            setIsLoading(false);
        }
    };

    const promptDelete = (id: string, name: string) => {
        setItemToDelete({ id, name });
    };

    const performDelete = async () => {
        if (!itemToDelete) return;
        try {
            setIsDeleting(true);
            await productApi.delete(itemToDelete.id);
            toast.success("Xóa sản phẩm thành công");
            loadProducts();
            setItemToDelete(null);
        } catch (error: any) {
            toast.error(error.message || "Lỗi khi xóa sản phẩm");
        } finally {
            setIsDeleting(false);
        }
    };

    // --- Helper: Get Visuals based on Product Category ---
    const getCategoryVisuals = (category: string) => {
        switch (category) {
            case ProductCategory.Food:
                return {
                    icon: Popcorn,
                    color: "text-orange-500",
                    bg: "bg-orange-500/10 border-orange-200",
                    label: "Đồ ăn"
                };
            case ProductCategory.Drink:
                return {
                    icon: Coffee,
                    color: "text-blue-500",
                    bg: "bg-blue-500/10 border-blue-200",
                    label: "Đồ uống"
                };
            case ProductCategory.Combo:
                return {
                    icon: Gift,
                    color: "text-purple-500",
                    bg: "bg-purple-500/10 border-purple-200",
                    label: "Combo"
                };
            default:
                return {
                    icon: Package,
                    color: "text-slate-500",
                    bg: "bg-slate-100 border-slate-200 dark:bg-slate-800 dark:border-slate-700",
                    label: "Khác"
                };
        }
    };

    // Format price
    const formatPrice = (price: number) => {
        return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: 'VND'
        }).format(price);
    };

    return (
        <div className="space-y-8 p-1">
            {/* Header Section */}
            <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
                <div>
                    <h1 className="text-3xl font-bold tracking-tight bg-gradient-to-r from-primary to-orange-600 bg-clip-text text-transparent">
                        Quản lý Sản phẩm
                    </h1>
                    <p className="text-muted-foreground mt-1 text-sm">
                        Quản lý các sản phẩm bán tại rạp: đồ ăn, thức uống, combo...
                    </p>
                </div>
                <Button
                    onClick={() => navigate("/products/create")}
                    className="shadow-lg shadow-primary/20 transition-all hover:scale-105"
                >
                    <Plus className="mr-2 h-4 w-4" />
                    Thêm Sản phẩm
                </Button>
            </div>

            {/* Main Content */}
            <Card className="border-none shadow-md bg-card/50 backdrop-blur-sm">
                <CardHeader className="pb-3">
                    <div className="flex flex-col md:flex-row items-start md:items-center justify-between gap-4">
                        <CardTitle className="text-xl">Danh sách sản phẩm</CardTitle>
                        <div className="flex flex-col sm:flex-row gap-3 w-full md:w-auto">
                            {/* Category Filter */}
                            <div className="flex items-center gap-2">
                                <Filter className="h-4 w-4 text-muted-foreground" />
                                <Select value={categoryFilter} onValueChange={setCategoryFilter}>
                                    <SelectTrigger className="w-[150px] bg-background">
                                        <SelectValue placeholder="Danh mục" />
                                    </SelectTrigger>
                                    <SelectContent>
                                        <SelectItem value="all">Tất cả</SelectItem>
                                        <SelectItem value={ProductCategory.Food}>Đồ ăn</SelectItem>
                                        <SelectItem value={ProductCategory.Drink}>Đồ uống</SelectItem>
                                        <SelectItem value={ProductCategory.Combo}>Combo</SelectItem>
                                    </SelectContent>
                                </Select>
                            </div>
                            {/* Search */}
                            <div className="relative w-full sm:w-80">
                                <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                                <Input
                                    placeholder="Tìm theo tên sản phẩm..."
                                    value={searchTerm}
                                    onChange={(e) => setSearchTerm(e.target.value)}
                                    className="pl-10 bg-background"
                                />
                            </div>
                        </div>
                    </div>
                </CardHeader>

                <CardContent className="p-0">
                    <div className="rounded-md border bg-background">
                        <Table>
                            <TableHeader>
                                <TableRow className="bg-muted/50 hover:bg-muted/50">
                                    <TableHead className="w-[80px] text-center">Ảnh</TableHead>
                                    <TableHead>Tên sản phẩm</TableHead>
                                    <TableHead>Danh mục</TableHead>
                                    <TableHead className="text-right">Giá</TableHead>
                                    <TableHead className="text-center">Trạng thái</TableHead>
                                    <TableHead className="text-right pr-6">Hành động</TableHead>
                                </TableRow>
                            </TableHeader>
                            <TableBody>
                                {isLoading ? (
                                    <TableRow>
                                        <TableCell colSpan={6} className="h-32 text-center text-muted-foreground">
                                            <div className="flex items-center justify-center gap-2">
                                                <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-primary"></div>
                                                Đang tải dữ liệu...
                                            </div>
                                        </TableCell>
                                    </TableRow>
                                ) : products.length === 0 ? (
                                    <TableRow>
                                        <TableCell colSpan={6} className="h-48 text-center">
                                            <div className="flex flex-col items-center justify-center gap-2">
                                                <div className="h-12 w-12 rounded-full bg-muted flex items-center justify-center">
                                                    <Package className="h-6 w-6 text-muted-foreground/50" />
                                                </div>
                                                <p className="text-muted-foreground font-medium">Chưa có sản phẩm nào</p>
                                                <p className="text-xs text-muted-foreground">Hãy thêm sản phẩm mới để bắt đầu bán hàng.</p>
                                            </div>
                                        </TableCell>
                                    </TableRow>
                                ) : (
                                    products.map((product) => {
                                        const visual = getCategoryVisuals(product.category);
                                        const Icon = visual.icon;

                                        return (
                                            <TableRow key={product.id} className="group hover:bg-muted/30 transition-colors">
                                                {/* Column: Image */}
                                                <TableCell className="text-center">
                                                    {product.imageUrl ? (
                                                        <img
                                                            src={product.imageUrl}
                                                            alt={product.name}
                                                            className="h-12 w-12 object-cover rounded-lg mx-auto border"
                                                        />
                                                    ) : (
                                                        <div className={cn("mx-auto h-12 w-12 rounded-lg flex items-center justify-center border transition-all", visual.bg)}>
                                                            <Icon className={cn("h-6 w-6", visual.color)} />
                                                        </div>
                                                    )}
                                                </TableCell>

                                                {/* Column: Name & Description */}
                                                <TableCell>
                                                    <div>
                                                        <div className="font-medium text-foreground">{product.name}</div>
                                                        {product.description && (
                                                            <p className="text-xs text-muted-foreground line-clamp-1 max-w-[250px]">
                                                                {product.description}
                                                            </p>
                                                        )}
                                                    </div>
                                                </TableCell>

                                                {/* Column: Category */}
                                                <TableCell>
                                                    <TooltipProvider>
                                                        <Tooltip>
                                                            <TooltipTrigger asChild>
                                                                <Badge variant="outline" className={cn("gap-1", visual.bg, visual.color)}>
                                                                    <Icon className="h-3 w-3" />
                                                                    {visual.label}
                                                                </Badge>
                                                            </TooltipTrigger>
                                                            <TooltipContent>
                                                                <p>Danh mục: {visual.label}</p>
                                                            </TooltipContent>
                                                        </Tooltip>
                                                    </TooltipProvider>
                                                </TableCell>

                                                {/* Column: Price */}
                                                <TableCell className="text-right">
                                                    <span className="font-semibold text-emerald-600 dark:text-emerald-400">
                                                        {formatPrice(product.price)}
                                                    </span>
                                                </TableCell>

                                                {/* Column: Status */}
                                                <TableCell className="text-center">
                                                    {product.isActive ? (
                                                        <Badge className="bg-emerald-100 text-emerald-700 hover:bg-emerald-100 border-emerald-200 dark:bg-emerald-900/30 dark:text-emerald-400 dark:border-emerald-800">
                                                            <ToggleRight className="w-3 h-3 mr-1" />
                                                            Đang bán
                                                        </Badge>
                                                    ) : (
                                                        <Badge variant="secondary" className="text-muted-foreground bg-slate-100 dark:bg-slate-800">
                                                            <ToggleLeft className="w-3 h-3 mr-1" />
                                                            Ngưng bán
                                                        </Badge>
                                                    )}
                                                </TableCell>

                                                {/* Column: Actions */}
                                                <TableCell className="text-right pr-6">
                                                    <div className="flex items-center justify-end gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                                                        <Button
                                                            variant="ghost"
                                                            size="icon"
                                                            className="h-8 w-8 text-muted-foreground hover:text-primary hover:bg-primary/10"
                                                            onClick={() => navigate(`/products/edit/${product.id}`)}
                                                        >
                                                            <Pencil className="h-4 w-4" />
                                                        </Button>
                                                        <Button
                                                            variant="ghost"
                                                            size="icon"
                                                            className="h-8 w-8 text-muted-foreground hover:text-red-600 hover:bg-red-50"
                                                            onClick={() => promptDelete(product.id, product.name)}
                                                        >
                                                            <Trash2 className="h-4 w-4" />
                                                        </Button>
                                                    </div>
                                                </TableCell>
                                            </TableRow>
                                        );
                                    })
                                )}
                            </TableBody>
                        </Table>
                    </div>
                </CardContent>
            </Card>

            {/* Delete Confirmation Dialog */}
            <AlertDialog open={!!itemToDelete} onOpenChange={(open) => !open && setItemToDelete(null)}>
                <AlertDialogContent>
                    <AlertDialogHeader>
                        <AlertDialogTitle className="flex items-center gap-2 text-red-600">
                            <AlertCircle className="h-5 w-5" />
                            Xác nhận xóa
                        </AlertDialogTitle>
                        <AlertDialogDescription>
                            Bạn có chắc chắn muốn xóa sản phẩm <span className="font-bold text-foreground">"{itemToDelete?.name}"</span> không?
                            <br />
                            Hành động này không thể hoàn tác.
                        </AlertDialogDescription>
                    </AlertDialogHeader>
                    <AlertDialogFooter>
                        <AlertDialogCancel disabled={isDeleting}>Hủy bỏ</AlertDialogCancel>
                        <AlertDialogAction
                            onClick={(e) => {
                                e.preventDefault();
                                performDelete();
                            }}
                            className="bg-red-600 hover:bg-red-700 text-white"
                            disabled={isDeleting}
                        >
                            {isDeleting ? "Đang xử lý..." : "Xóa vĩnh viễn"}
                        </AlertDialogAction>
                    </AlertDialogFooter>
                </AlertDialogContent>
            </AlertDialog>
        </div>
    );
};

export default ProductsListPage;
