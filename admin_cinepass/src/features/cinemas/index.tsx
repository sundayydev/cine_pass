import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import {
  Plus,
  Search,
  Pencil,
  Trash2,
  Eye,
  MapPin,
  Phone,
  Mail,
  Building2,
  Filter,
} from "lucide-react";
import { toast } from "sonner";

// API Services
import { cinemaApi, type CinemaResponseDto } from "@/services/apiCinema";
import { PATHS } from "@/config/paths";

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
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { useDebounce } from "@/hooks/useDebounce";

const CinemasListPage = () => {
  const navigate = useNavigate();

  // State
  const [cinemas, setCinemas] = useState<CinemaResponseDto[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState<string>("all");
  const [cityFilter, setCityFilter] = useState<string>("all");
  const debouncedSearch = useDebounce(searchTerm, 500);

  // Load cinemas
  useEffect(() => {
    loadCinemas();
  }, [statusFilter]);

  const loadCinemas = async () => {
    try {
      setIsLoading(true);
      let data: CinemaResponseDto[];

      if (statusFilter === "active") {
        data = await cinemaApi.getActive();
      } else {
        data = await cinemaApi.getAll();
      }

      // Filter by search term
      if (debouncedSearch) {
        data = data.filter(
          (cinema) =>
            cinema.name.toLowerCase().includes(debouncedSearch.toLowerCase()) ||
            cinema.address?.toLowerCase().includes(debouncedSearch.toLowerCase()) ||
            cinema.city?.toLowerCase().includes(debouncedSearch.toLowerCase())
        );
      }

      // Filter by city
      if (cityFilter !== "all") {
        data = data.filter((cinema) => cinema.city === cityFilter);
      }

      setCinemas(data);
    } catch (error) {
      console.error("Error loading cinemas:", error);
      toast.error("Lỗi khi tải danh sách rạp chiếu phim");
    } finally {
      setIsLoading(false);
    }
  };

  // Reload when search changes
  useEffect(() => {
    loadCinemas();
  }, [debouncedSearch, cityFilter]);

  // Get unique cities
  const uniqueCities = Array.from(new Set(cinemas.map((c) => c.city)));

  // Delete cinema
  const handleDelete = async (id: string, name: string) => {
    if (!confirm(`Bạn có chắc chắn muốn xóa rạp "${name}"?`)) {
      return;
    }

    try {
      await cinemaApi.delete(id);
      toast.success("Xóa rạp chiếu phim thành công");
      loadCinemas();
    } catch (error) {
      console.error("Error deleting cinema:", error);
      toast.error(error instanceof Error ? error.message : "Lỗi khi xóa rạp chiếu phim");
    }
  };

  // Format date
  const formatDate = (dateString: string) => {
    if (!dateString) return "N/A";
    return new Date(dateString).toLocaleDateString("vi-VN");
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Quản lý Rạp Chiếu Phim</h1>
          <p className="text-muted-foreground mt-1">
            Quản lý thông tin rạp chiếu phim, phòng chiếu và ghế ngồi
          </p>
        </div>
        <Button onClick={() => navigate(PATHS.CINEMA_CREATE)}>
          <Plus className="mr-2 h-4 w-4" />
          Thêm Rạp Mới
        </Button>
      </div>

      {/* Filters */}
      <Card>
        <CardHeader>
          <CardTitle className="text-lg">Bộ lọc</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex flex-col md:flex-row gap-4">
            <div className="flex-1">
              <div className="relative">
                <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                <Input
                  placeholder="Tìm kiếm theo tên, địa chỉ, thành phố..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="pl-10"
                />
              </div>
            </div>
            <Select value={statusFilter} onValueChange={setStatusFilter}>
              <SelectTrigger className="w-full md:w-[180px]">
                <SelectValue placeholder="Trạng thái" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Tất cả</SelectItem>
                <SelectItem value="active">Đang hoạt động</SelectItem>
                <SelectItem value="inactive">Ngừng hoạt động</SelectItem>
              </SelectContent>
            </Select>
            <Select value={cityFilter} onValueChange={setCityFilter}>
              <SelectTrigger className="w-full md:w-[180px]">
                <SelectValue placeholder="Thành phố" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Tất cả thành phố</SelectItem>
                {uniqueCities.map((city) => (
                  <SelectItem key={city} value={city || ""}>
                    {city}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        </CardContent>
      </Card>

      {/* Table */}
      <Card>
        <CardContent className="p-0">
          {isLoading ? (
            <div className="flex items-center justify-center py-12">
              <div className="text-muted-foreground">Đang tải...</div>
            </div>
          ) : cinemas.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-12">
              <Building2 className="h-12 w-12 text-muted-foreground mb-4" />
              <p className="text-muted-foreground">Chưa có rạp chiếu phim nào</p>
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Tên Rạp</TableHead>
                  <TableHead>Địa Chỉ</TableHead>
                  <TableHead>Thành Phố</TableHead>
                  <TableHead>Liên Hệ</TableHead>
                  <TableHead>Trạng Thái</TableHead>
                  <TableHead>Ngày Tạo</TableHead>
                  <TableHead className="text-right">Thao Tác</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {cinemas.map((cinema) => (
                  <TableRow key={cinema.id}>
                    <TableCell className="font-medium">{cinema.name}</TableCell>
                    <TableCell>
                      <div className="flex items-center gap-2">
                        <MapPin className="h-4 w-4 text-muted-foreground" />
                        <span className="max-w-[200px] truncate">{cinema.address}</span>
                      </div>
                    </TableCell>
                    <TableCell>{cinema.city}</TableCell>
                    <TableCell>
                      <div className="space-y-1">
                        {cinema.phone && (
                          <div className="flex items-center gap-2 text-sm">
                            <Phone className="h-3 w-3 text-muted-foreground" />
                            {cinema.phone}
                          </div>
                        )}
                        {cinema.email && (
                          <div className="flex items-center gap-2 text-sm">
                            <Mail className="h-3 w-3 text-muted-foreground" />
                            <span className="max-w-[150px] truncate">{cinema.email}</span>
                          </div>
                        )}
                        {!cinema.phone && !cinema.email && (
                          <span className="text-muted-foreground text-sm">-</span>
                        )}
                      </div>
                    </TableCell>
                    <TableCell>
                      {cinema.isActive ? (
                        <Badge className="bg-emerald-500/15 text-emerald-700 hover:bg-emerald-500/25 border-emerald-500/30">
                          Đang hoạt động
                        </Badge>
                      ) : (
                        <Badge variant="outline">Ngừng hoạt động</Badge>
                      )}
                    </TableCell>
                    <TableCell>{formatDate(cinema.createdAt || new Date().toISOString())}</TableCell>
                    <TableCell className="text-right">
                      <div className="flex items-center justify-end gap-2">
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => navigate(PATHS.CINEMA_DETAIL.replace(":id", cinema.id))}
                        >
                          <Eye className="h-4 w-4" />
                        </Button>
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => navigate(`/cinemas/edit/${cinema.id}`)}
                        >
                          <Pencil className="h-4 w-4" />
                        </Button>
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => handleDelete(cinema.id, cinema.name)}
                        >
                          <Trash2 className="h-4 w-4 text-destructive" />
                        </Button>
                      </div>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>
    </div>
  );
};

export default CinemasListPage;

