import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import {
    Plus,
    Search,
    Pencil,
    Trash2,
    Eye,
    User,
} from "lucide-react";
import { toast } from "sonner";

// API
import { actorApi } from "@/services/apiActor";
import type { ActorResponseDto } from "@/services/apiActor";
import { useDebounce } from "@/hooks/useDebounce";
import { PATHS } from "@/config/paths";

// UI Components
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
import { Card } from "@/components/ui/card";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";

const ActorListPage = () => {
    const navigate = useNavigate();

    const [actors, setActors] = useState<ActorResponseDto[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [searchTerm, setSearchTerm] = useState("");
    const [page, setPage] = useState(1);
    const itemsPerPage = 10;

    const debouncedSearch = useDebounce(searchTerm, 500);

    // Load actors
    useEffect(() => {
        loadActors();
    }, [debouncedSearch]);

    const loadActors = async () => {
        try {
            setIsLoading(true);
            let data: ActorResponseDto[];

            if (debouncedSearch.trim()) {
                data = await actorApi.search(debouncedSearch);
            } else {
                data = await actorApi.getAll();
            }

            setActors(data);
        } catch (error: any) {
            console.error("Error loading actors:", error);
            toast.error(error.message || "Lỗi khi tải danh sách diễn viên");
        } finally {
            setIsLoading(false);
        }
    };

    const handleDelete = async (id: string, name: string) => {
        if (!window.confirm(`Bạn có chắc chắn muốn xóa diễn viên "${name}"?`)) {
            return;
        }

        try {
            await actorApi.delete(id);
            toast.success("Xóa diễn viên thành công");
            loadActors();
        } catch (error: any) {
            console.error("Error deleting actor:", error);
            toast.error(error.message || "Lỗi khi xóa diễn viên");
        }
    };

    const formatDate = (dateString: string) => {
        if (!dateString) return "N/A";
        return new Date(dateString).toLocaleDateString("vi-VN");
    };

    // Pagination
    const totalItems = actors.length;
    const totalPages = Math.max(1, Math.ceil(totalItems / itemsPerPage));
    const startIndex = (page - 1) * itemsPerPage;
    const endIndex = startIndex + itemsPerPage;
    const actorList = actors.slice(startIndex, endIndex);

    // Reset page on search
    useEffect(() => {
        setPage(1);
    }, [debouncedSearch]);

    return (
        <div className="flex-1 space-y-4 p-4 md:p-8 pt-6">
            <div className="flex flex-col gap-6 sm:flex-row sm:items-center sm:justify-between">
                <div className="space-y-2">
                    <div className="flex items-center gap-3">
                        <div className="flex h-12 w-12 items-center justify-center rounded-2xl bg-primary">
                            <User className="h-6 w-6 text-primary-foreground" />
                        </div>
                        <div>
                            <h1 className="bg-gradient-to-r from-foreground to-foreground/70 bg-clip-text text-4xl font-bold tracking-tight text-transparent">
                                Quản lý Diễn viên
                            </h1>
                            <p className="mt-1 text-sm text-muted-foreground">
                                Quản lý danh sách diễn viên và thông tin liên quan
                            </p>
                        </div>
                    </div>
                </div>
                <Button
                    onClick={() => navigate(PATHS.ACTOR_CREATE)}
                    size="lg"
                    className="gap-2 shadow-lg"
                >
                    <Plus className="h-5 w-5" /> Thêm diễn viên mới
                </Button>
            </div>

            <Card className="border-border/50 bg-card/50 p-6 shadow-xl backdrop-blur-sm">
                <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
                    <div className="relative flex-1">
                        <Search className="absolute left-3.5 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                        <Input
                            placeholder="Tìm kiếm theo tên diễn viên..."
                            className="h-11 border-border/50 bg-background pl-10 shadow-sm transition-shadow focus-visible:shadow-md"
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                        />
                    </div>
                </div>

                <div className="mt-4 flex items-center gap-6 border-t border-border/50 pt-4">
                    <div className="flex items-center gap-2 text-sm">
                        <div className="h-2 w-2 rounded-full bg-primary"></div>
                        <span className="text-muted-foreground">
                            {actors.length} Diễn viên
                        </span>
                    </div>
                </div>
            </Card>

            <Card className="overflow-hidden border-border/50 shadow-xl">
                <Table>
                    <TableHeader>
                        <TableRow className="border-border/50 bg-muted/30 hover:bg-muted/30">
                            <TableHead className="hidden w-[100px] font-semibold md:table-cell">
                                Ảnh
                            </TableHead>
                            <TableHead className="min-w-[200px] font-semibold">
                                Tên diễn viên
                            </TableHead>
                            <TableHead className="hidden font-semibold md:table-cell">
                                Mô tả
                            </TableHead>
                            <TableHead className="hidden font-semibold md:table-cell">
                                Ngày tạo
                            </TableHead>
                            <TableHead className="text-right font-semibold">
                                Hành động
                            </TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        {isLoading ? (
                            [...Array(5)].map((_, i) => (
                                <TableRow key={i} className="border-border/30">
                                    <TableCell className="hidden md:table-cell">
                                        <div className="h-16 w-16 animate-pulse rounded-full bg-muted/50" />
                                    </TableCell>
                                    <TableCell>
                                        <div className="space-y-2">
                                            <div className="h-5 w-48 animate-pulse rounded bg-muted/50" />
                                            <div className="h-4 w-32 animate-pulse rounded bg-muted/30" />
                                        </div>
                                    </TableCell>
                                    <TableCell className="hidden md:table-cell">
                                        <div className="h-4 w-64 animate-pulse rounded bg-muted/50" />
                                    </TableCell>
                                    <TableCell className="hidden md:table-cell">
                                        <div className="h-4 w-24 animate-pulse rounded bg-muted/50" />
                                    </TableCell>
                                    <TableCell />
                                </TableRow>
                            ))
                        ) : actorList.length === 0 ? (
                            <TableRow>
                                <TableCell colSpan={5} className="h-40 text-center">
                                    <div className="flex flex-col items-center justify-center gap-2">
                                        <div className="rounded-full bg-muted/50 p-3">
                                            <Search className="h-6 w-6 text-muted-foreground" />
                                        </div>
                                        <p className="text-sm font-medium text-foreground">
                                            Không tìm thấy diễn viên nào
                                        </p>
                                        <p className="text-xs text-muted-foreground">
                                            Thử điều chỉnh từ khóa tìm kiếm
                                        </p>
                                    </div>
                                </TableCell>
                            </TableRow>
                        ) : (
                            actorList.map((actor) => (
                                <TableRow
                                    key={actor.id}
                                    className="group border-border/30 transition-colors hover:bg-muted/30"
                                >
                                    <TableCell className="hidden md:table-cell">
                                        <Avatar className="h-16 w-16 shadow-md transition-transform group-hover:scale-105 group-hover:shadow-lg">
                                            <AvatarImage src={actor.imageUrl} alt={actor.name} className="object-cover" />
                                            <AvatarFallback className="bg-primary/10 text-lg font-bold text-primary">
                                                {actor.name.substring(0, 2).toUpperCase()}
                                            </AvatarFallback>
                                        </Avatar>
                                    </TableCell>
                                    <TableCell>
                                        <div className="flex flex-col gap-1.5">
                                            <span className="text-base font-semibold leading-tight text-foreground">
                                                {actor.name}
                                            </span>
                                            {actor.slug && (
                                                <span className="text-xs text-muted-foreground font-mono">
                                                    {actor.slug}
                                                </span>
                                            )}
                                        </div>
                                    </TableCell>
                                    <TableCell className="hidden md:table-cell">
                                        <p className="text-sm text-muted-foreground line-clamp-2 max-w-md">
                                            {actor.description || "Chưa có mô tả"}
                                        </p>
                                    </TableCell>
                                    <TableCell className="hidden md:table-cell">
                                        <span className="text-sm font-medium text-foreground">
                                            {formatDate(actor.createdAt)}
                                        </span>
                                    </TableCell>
                                    <TableCell>
                                        <div className="flex items-center justify-end gap-2">
                                            <Button
                                                variant="ghost"
                                                size="icon"
                                                onClick={() => {
                                                    const slugOrId = actor.slug || actor.id;
                                                    navigate(PATHS.ACTOR_DETAIL.replace(":slug", slugOrId));
                                                }}
                                                className="h-8 w-8 text-blue-500 hover:text-blue-600 hover:bg-blue-50"
                                                title="Xem chi tiết"
                                            >
                                                <Eye className="h-4 w-4" />
                                            </Button>

                                            <Button
                                                variant="ghost"
                                                size="icon"
                                                onClick={() =>
                                                    navigate(PATHS.ACTOR_EDIT.replace(":id", actor.id))
                                                }
                                                className="h-8 w-8 text-orange-500 hover:text-orange-600 hover:bg-orange-50"
                                                title="Chỉnh sửa"
                                            >
                                                <Pencil className="h-4 w-4" />
                                            </Button>

                                            <Button
                                                variant="ghost"
                                                size="icon"
                                                onClick={() => handleDelete(actor.id, actor.name)}
                                                className="h-8 w-8 text-red-500 hover:text-red-600 hover:bg-red-50"
                                                title="Xóa diễn viên"
                                            >
                                                <Trash2 className="h-4 w-4" />
                                            </Button>
                                        </div>
                                    </TableCell>
                                </TableRow>
                            ))
                        )}
                    </TableBody>
                </Table>

                {!isLoading && totalItems > 0 && (
                    <div className="flex flex-col gap-4 border-t border-border/50 bg-muted/20 p-4 sm:flex-row sm:items-center sm:justify-between">
                        <div className="text-sm text-muted-foreground">
                            Hiển thị{" "}
                            <span className="font-semibold text-foreground">
                                {startIndex + 1}
                            </span>{" "}
                            -{" "}
                            <span className="font-semibold text-foreground">
                                {Math.min(endIndex, totalItems)}
                            </span>{" "}
                            trong tổng số{" "}
                            <span className="font-semibold text-foreground">
                                {totalItems}
                            </span>{" "}
                            diễn viên
                        </div>
                        <div className="flex items-center gap-4">
                            <div className="text-sm text-muted-foreground">
                                Trang{" "}
                                <span className="font-semibold text-foreground">{page}</span> /{" "}
                                <span className="font-semibold text-foreground">
                                    {totalPages}
                                </span>
                            </div>
                            <div className="flex gap-2">
                                <Button
                                    variant="outline"
                                    size="sm"
                                    onClick={() => setPage((p) => Math.max(1, p - 1))}
                                    disabled={page === 1 || isLoading}
                                    className="shadow-sm transition-all hover:shadow-md disabled:opacity-50"
                                >
                                    Trước
                                </Button>
                                <Button
                                    variant="outline"
                                    size="sm"
                                    onClick={() => setPage((p) => p + 1)}
                                    disabled={page >= totalPages || isLoading}
                                    className="shadow-sm transition-all hover:shadow-md disabled:opacity-50"
                                >
                                    Sau
                                </Button>
                            </div>
                        </div>
                    </div>
                )}
            </Card>
        </div>
    );
};

export default ActorListPage;
