import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import {
  Plus,
  Search,
  Pencil,
  Trash2,
  Eye,
  Film,
  Calendar,
  Clock,
  Filter,
} from "lucide-react";

// Hooks & API
import { useMovies, useDeleteMovie } from "@/hooks/useMovies.ts";
import { useDebounce } from "@/hooks/useDebounce.ts";
import { PATHS } from "@/config/paths";
import type { Movie } from "@/types/moveType";

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
import { Badge } from "@/components/ui/badge";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Card } from "@/components/ui/card";
import { CATEGORY_LABEL_MAP, MOVIE_CATEGORIES } from "@/constants/movieCategory";

const MovieListPage = () => {
  const navigate = useNavigate();

  // 1. Quản lý State cho bộ lọc
  const [page, setPage] = useState(1);
  const [itemsPerPage] = useState(10);
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState<string>("all");
  const [categoryFilter, setCategoryFilter] = useState<string>("all");
  // Debounce search
  const debouncedSearch = useDebounce(searchTerm, 500);

  // 2. Gọi API
  const { data, isLoading, isError } = useMovies({
    page: page,
    limit: itemsPerPage,
    search: debouncedSearch,
  });

  // Hook xóa phim
  const deleteMovieMutation = useDeleteMovie();

  // Helper: Format ngày tháng
  const formatDate = (dateString: string) => {
    if (!dateString) return "N/A";
    return new Date(dateString).toLocaleDateString("vi-VN");
  };

  // Helper: Render Badge trạng thái
  const renderStatusBadge = (status: string) => {
    const normalizedStatus = status?.toUpperCase();

    switch (normalizedStatus) {
      case "SHOWING":
      case "NOW_SHOWING":
        return (
          <Badge className="bg-emerald-500/15 text-emerald-700 hover:bg-emerald-500/25 border-emerald-500/30 font-medium">
            Đang chiếu
          </Badge>
        );

      case "COMING_SOON":
        return (
          <Badge className="bg-amber-500/15 text-amber-700 hover:bg-amber-500/25 border-amber-500/30 font-medium">
            Sắp chiếu
          </Badge>
        );

      case "ENDED":
        return (
          <Badge className="bg-slate-500/15 text-slate-600 hover:bg-slate-500/25 border-slate-500/30 font-medium">
            Đã kết thúc
          </Badge>
        );

      default:
        return (
          <Badge variant="outline" className="font-medium">
            {status}
          </Badge>
        );
    }
  };

  const handleEdit = (id: string) => {
    navigate(PATHS.MOVIE_EDIT.replace(":id", id));
  };

  const handleDelete = (id: string, title: string) => {
    if (window.confirm(`Bạn có chắc chắn muốn xóa phim "${title}"?`)) {
      deleteMovieMutation.mutate(id);
    }
  };

  // Lấy danh sách phim an toàn
  const allMovies = data || [];

  // Filter theo status ở frontend
  const filteredMovies = allMovies.filter(movie => {
    const statusMatch =
      statusFilter === "all"
        ? true
        : movie.status?.toUpperCase() === statusFilter;
  
    const categoryMatch =
      categoryFilter === "all"
        ? true
        : movie.category?.toUpperCase() === categoryFilter;
  
    return statusMatch && categoryMatch;
  });
  

  // Tính toán phân trang
  const totalItems = filteredMovies.length;
  const totalPages = Math.max(1, Math.ceil(totalItems / itemsPerPage));
  const startIndex = (page - 1) * itemsPerPage;
  const endIndex = startIndex + itemsPerPage;

  // Lấy danh sách phim cho trang hiện tại
  const movieList = filteredMovies.slice(startIndex, endIndex);

  // Reset về trang 1 khi search hoặc filter thay đổi
  useEffect(() => {
    setPage(1);
  }, [debouncedSearch, statusFilter]);

  // Reset về trang 1 nếu trang hiện tại vượt quá tổng số trang
  useEffect(() => {
    if (page > totalPages && totalPages > 0) {
      setPage(1);
    }
  }, [page, totalPages]);

  return (
    <div className="flex-1 space-y-4 p-4 md:p-8 pt-6">
      <div className="flex flex-col gap-6 sm:flex-row sm:items-center sm:justify-between">
        <div className="space-y-2">
          <div className="flex items-center gap-3">
            <div className="flex h-12 w-12 items-center justify-center rounded-2xl bg-primary">
              <Film className="h-6 w-6 text-primary-foreground" />
            </div>
            <div>
              <h1 className="bg-gradient-to-r from-foreground to-foreground/70 bg-clip-text text-4xl font-bold tracking-tight text-transparent">
                Quản lý Phim
              </h1>
              <p className="mt-1 text-sm text-muted-foreground">
                Quản lý danh sách phim, suất chiếu và trạng thái của bạn
              </p>
            </div>
          </div>
        </div>
        <Button
          onClick={() => navigate(PATHS.MOVIE_CREATE)}
          size="lg"
          className="gap-2shadow-lg"
        >
          <Plus className="h-5 w-5" /> Thêm phim mới
        </Button>
      </div>

      <Card className="border-border/50 bg-card/50 p-6 shadow-xl backdrop-blur-sm">
        <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
          <div className="relative flex-1">
            <Search className="absolute left-3.5 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
            <Input
              placeholder="Tìm kiếm theo tên phim..."
              className="h-11 border-border/50 bg-background pl-10 shadow-sm transition-shadow focus-visible:shadow-md"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </div>

          <div className="flex items-center gap-2">
            <Filter className="h-4 w-4 text-muted-foreground" />
            <Select
              value={statusFilter}
              onValueChange={(val) => setStatusFilter(val)}
            >
              <SelectTrigger className="h-11 w-[200px] border-border/50 bg-background shadow-sm">
                <SelectValue placeholder="Trạng thái" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Tất cả trạng thái</SelectItem>
                <SelectItem value="SHOWING">Đang chiếu</SelectItem>
                <SelectItem value="COMING_SOON">Sắp chiếu</SelectItem>
                <SelectItem value="ENDED">Đã kết thúc</SelectItem>
              </SelectContent>
            </Select>
            {/* Filter Category */}
            <Select value={categoryFilter} onValueChange={setCategoryFilter}>
              <SelectTrigger className="h-11 w-[200px]">
                <SelectValue placeholder="Thể loại" />
              </SelectTrigger>
              <SelectContent className="max-h-72">
                <SelectItem value="all">Tất cả thể loại</SelectItem>
                {MOVIE_CATEGORIES.map((c: { value: string; label: string; }) => (
                  <SelectItem key={c.value} value={c.value}>
                    {c.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        </div>

        <div className="mt-4 flex items-center gap-6 border-t border-border/50 pt-4">
          <div className="flex items-center gap-2 text-sm">
            <div className="h-2 w-2 rounded-full bg-emerald-500"></div>
            <span className="text-muted-foreground">
              {
                allMovies.filter(
                  (m) =>
                    m.status?.toUpperCase() === "SHOWING" ||
                    m.status?.toUpperCase() === "NOW_SHOWING"
                ).length
              }{" "}
              Đang chiếu
            </span>
          </div>
          <div className="flex items-center gap-2 text-sm">
            <div className="h-2 w-2 rounded-full bg-amber-500"></div>
            <span className="text-muted-foreground">
              {
                allMovies.filter(
                  (m) => m.status?.toUpperCase() === "COMING_SOON"
                ).length
              }{" "}
              Sắp chiếu
            </span>
          </div>
          <div className="flex items-center gap-2 text-sm">
            <div className="h-2 w-2 rounded-full bg-slate-400"></div>
            <span className="text-muted-foreground">
              {
                allMovies.filter((m) => m.status?.toUpperCase() === "ENDED")
                  .length
              }{" "}
              Đã kết thúc
            </span>
          </div>
        </div>
      </Card>

      <Card className="overflow-hidden border-border/50 shadow-xl">
        <Table>
          <TableHeader>
            <TableRow className="border-border/50 bg-muted/30 hover:bg-muted/30">
              <TableHead className="hidden w-[100px] font-semibold md:table-cell">
                Poster
              </TableHead>
              <TableHead className="min-w-[160px] font-semibold">
                Thông tin phim
              </TableHead>
              <TableHead className="hidden font-semibold md:table-cell">
                Thể loại
              </TableHead>
              <TableHead className="hidden font-semibold md:table-cell">
                Thời lượng
              </TableHead>
              <TableHead className="hidden font-semibold md:table-cell">
                Khởi chiếu
              </TableHead>
              <TableHead className="font-semibold">Trạng thái</TableHead>
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
                    <div className="h-16 w-11 animate-pulse rounded-lg bg-muted/50" />
                  </TableCell>
                  <TableCell>
                    <div className="space-y-2">
                      <div className="h-5 w-48 animate-pulse rounded bg-muted/50" />
                      <div className="h-4 w-32 animate-pulse rounded bg-muted/30" />
                    </div>
                  </TableCell>
                  <TableCell className="hidden md:table-cell">
                    <div className="h-4 w-16 animate-pulse rounded bg-muted/50" />
                  </TableCell>
                  <TableCell className="hidden md:table-cell">
                    <div className="h-4 w-24 animate-pulse rounded bg-muted/50" />
                  </TableCell>
                  <TableCell>
                    <div className="h-6 w-24 animate-pulse rounded-full bg-muted/50" />
                  </TableCell>
                  <TableCell />
                </TableRow>
              ))
            ) : isError ? (
              <TableRow>
                <TableCell colSpan={6} className="h-40 text-center">
                  <div className="flex flex-col items-center justify-center gap-2">
                    <div className="rounded-full bg-destructive/10 p-3">
                      <Film className="h-6 w-6 text-destructive" />
                    </div>
                    <p className="text-sm font-medium text-destructive">
                      Lỗi khi tải dữ liệu!
                    </p>
                    <p className="text-xs text-muted-foreground">
                      Vui lòng thử lại sau
                    </p>
                  </div>
                </TableCell>
              </TableRow>
            ) : movieList.length === 0 ? (
              <TableRow>
                <TableCell colSpan={6} className="h-40 text-center">
                  <div className="flex flex-col items-center justify-center gap-2">
                    <div className="rounded-full bg-muted/50 p-3">
                      <Search className="h-6 w-6 text-muted-foreground" />
                    </div>
                    <p className="text-sm font-medium text-foreground">
                      Không tìm thấy phim nào
                    </p>
                    <p className="text-xs text-muted-foreground">
                      Thử điều chỉnh bộ lọc của bạn
                    </p>
                  </div>
                </TableCell>
              </TableRow>
            ) : (
              movieList.map((movie: Movie) => (
                <TableRow
                  key={movie.id}
                  className="group border-border/30 transition-colors hover:bg-muted/30"
                >
                  <TableCell className="hidden md:table-cell">
                    <div className="relative overflow-hidden rounded-lg shadow-md transition-transform group-hover:scale-105 group-hover:shadow-lg">
                      <img
                        src={movie.posterUrl || "/placeholder.svg"}
                        alt={movie.title}
                        className="h-20 w-14 object-cover"
                        onError={(e) => {
                          (e.target as HTMLImageElement).src =
                            "https://placehold.co/140x200/e2e8f0/64748b?text=No+Image";
                        }}
                      />
                    </div>
                  </TableCell>
                  <TableCell>
                    <div className="flex flex-col gap-1.5">
                      <span className="text-base font-semibold leading-tight text-foreground">
                        {movie.title}
                      </span>
                      <div className="flex flex-wrap items-center gap-2 text-xs text-muted-foreground md:hidden">
                        <div className="flex items-center gap-1">
                          <Clock className="h-3 w-3" />
                          {movie.durationMinutes} phút
                        </div>
                        <span>•</span>
                        <div className="flex items-center gap-1">
                          <Calendar className="h-3 w-3" />
                          {formatDate(movie.releaseDate || "")}
                        </div>
                      </div>
                    </div>
                  </TableCell>
                  <TableCell className="hidden md:table-cell">
                    <Badge variant="secondary" className="font-medium">
                      {CATEGORY_LABEL_MAP[movie.category as string] || movie.category}
                    </Badge>
                  </TableCell>
                  <TableCell className="hidden md:table-cell">
                    <div className="flex items-center gap-2 text-sm text-foreground">
                      <Clock className="h-4 w-4 text-muted-foreground" />
                      <span className="font-medium">
                        {movie.durationMinutes}
                      </span>
                      <span className="text-muted-foreground">phút</span>
                    </div>
                  </TableCell>
                  <TableCell className="hidden md:table-cell">
                    <div className="flex items-center gap-2 text-sm">
                      <Calendar className="h-4 w-4 text-muted-foreground" />
                      <span className="font-medium text-foreground">
                        {formatDate(movie.releaseDate || "")}
                      </span>
                    </div>
                  </TableCell>
                  <TableCell>
                    {renderStatusBadge(movie.status as string)}
                  </TableCell>
                  <TableCell>
                    <div className="flex items-center justify-end gap-2">
                      {/* 1. Nút Xem: Màu XANH DƯƠNG (Blue) */}
                      <Button
                        variant="ghost"
                        size="icon"
                        onClick={() => navigate(`${PATHS.MOVIES}/${movie.id}`)}
                        className="h-8 w-8 text-blue-500 hover:text-blue-600 hover:bg-blue-50"
                        title="Xem chi tiết"
                      >
                        <Eye className="h-4 w-4" />
                      </Button>

                      {/* 2. Nút Sửa: Màu CAM (Orange) */}
                      <Button
                        variant="ghost"
                        size="icon"
                        onClick={() => handleEdit(movie.id)}
                        className="h-8 w-8 text-orange-500 hover:text-orange-600 hover:bg-orange-50"
                        title="Chỉnh sửa"
                      >
                        <Pencil className="h-4 w-4" />
                      </Button>

                      {/* 3. Nút Xóa: Màu ĐỎ (Red) */}
                      <Button
                        variant="ghost"
                        size="icon"
                        onClick={() => handleDelete(movie.id, movie.title)}
                        className="h-8 w-8 text-red-500 hover:text-red-600 hover:bg-red-50"
                        disabled={deleteMovieMutation.isPending}
                        title="Xóa phim"
                      >
                        {deleteMovieMutation.isPending ? (
                          <span className="h-4 w-4 animate-spin rounded-full border-2 border-current border-t-transparent" />
                        ) : (
                          <Trash2 className="h-4 w-4" />
                        )}
                      </Button>
                    </div>
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>

        {!isLoading && !isError && totalItems > 0 && (
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
              phim
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

export default MovieListPage;
