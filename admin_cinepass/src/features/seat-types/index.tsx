import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import {
  Plus,
  Search,
  Pencil,
  Trash2,
  Tag,
} from "lucide-react";
import { toast } from "sonner";

// API Services
import { seatTypeApi, type SeatTypeResponseDto } from "@/services/apiSeatType";
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
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { useDebounce } from "@/hooks/useDebounce";
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle } from "@/components/ui/alert-dialog";

const SeatTypesListPage = () => {
  const navigate = useNavigate();

  // State
  const [seatTypes, setSeatTypes] = useState<SeatTypeResponseDto[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [searchTerm, setSearchTerm] = useState("");
  const debouncedSearch = useDebounce(searchTerm, 500);
  const [itemToDelete, setItemToDelete] = useState<{ code: string; name: string } | null>(null);  
  const [isDeleting, setIsDeleting] = useState(false);

  // Load seat types
  useEffect(() => {
    loadSeatTypes();
  }, []);

  useEffect(() => {
    loadSeatTypes();
  }, [debouncedSearch]);

  const loadSeatTypes = async () => {
    try {
      setIsLoading(true);
      let data = await seatTypeApi.getAll();

      // Filter by search term
      if (debouncedSearch) {
        data = data.filter(
          (seatType) =>
            seatType.code.toLowerCase().includes(debouncedSearch.toLowerCase()) ||
            (seatType.name && seatType.name.toLowerCase().includes(debouncedSearch.toLowerCase()))
        );
      }

      setSeatTypes(data);
    } catch (error) {
      console.error("Error loading seat types:", error);
      toast.error("Lỗi khi tải danh sách loại ghế");
    } finally {
      setIsLoading(false);
    }
  };

  // Delete seat type
  const promptDelete = (code: string, name: string) => {
    setItemToDelete({ code, name });
  };

  const performDelete = async () => {
    if (!itemToDelete) return;

    try {
      setIsDeleting(true);
      await seatTypeApi.delete(itemToDelete.code);
      
      toast.success("Xóa loại ghế thành công");
      loadSeatTypes(); // Reload dữ liệu
      
      setItemToDelete(null); // Đóng dialog sau khi thành công
    } catch (error) {
      console.error("Error deleting seat type:", error);
      toast.error(error instanceof Error ? error.message : "Lỗi khi xóa loại ghế");
      // Lưu ý: Không đóng dialog nếu lỗi để user có thể thử lại, hoặc đóng tùy logic của bạn
    } finally {
      setIsDeleting(false);
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Quản lý Loại Ghế</h1>
          <p className="text-muted-foreground mt-1">
            Quản lý các loại ghế và tỷ lệ phụ thu
          </p>
        </div>
        <Button onClick={() => navigate("/seat-types/create")}>
          <Plus className="mr-2 h-4 w-4" />
          Thêm Loại Ghế Mới
        </Button>
      </div>

      {/* Search */}
      <Card>
        <CardHeader>
          <CardTitle className="text-lg">Tìm kiếm</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
            <Input
              placeholder="Tìm kiếm theo mã hoặc tên loại ghế..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="pl-10"
            />
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
          ) : seatTypes.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-12">
              <Tag className="h-12 w-12 text-muted-foreground mb-4" />
              <p className="text-muted-foreground">Chưa có loại ghế nào</p>
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Mã Loại</TableHead>
                  <TableHead>Tên Loại</TableHead>
                  <TableHead>Tỷ Lệ Phụ Thu</TableHead>
                  <TableHead className="text-right">Thao Tác</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {seatTypes.map((seatType) => (
                  <TableRow key={seatType.code}>
                    <TableCell className="font-medium font-mono">{seatType.code}</TableCell>
                    <TableCell>{seatType.name || "-"}</TableCell>
                    <TableCell>
                      <span className="font-semibold">
                        {(seatType.surchargeRate * 100).toFixed(0)}%
                      </span>
                      <span className="text-muted-foreground text-sm ml-2">
                        (x{seatType.surchargeRate.toFixed(2)})
                      </span>
                    </TableCell>
                    <TableCell className="text-right">
                      <div className="flex items-center justify-end gap-2">
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => navigate(`/seat-types/edit/${seatType.code}`)}
                        >
                          <Pencil className="h-4 w-4" />
                        </Button>
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => promptDelete(seatType.code, seatType.name || "")}
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
      <AlertDialog
        open={!!itemToDelete} 
        onOpenChange={(open) => !open && setItemToDelete(null)} >
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>Bạn có chắc chắn muốn xóa?</AlertDialogTitle>
          <AlertDialogDescription>
            Hành động này không thể hoàn tác. Bạn đang xóa loại ghế:{" "}
            <span className="font-bold text-red-500">
              {itemToDelete?.code} {itemToDelete?.name ? `(${itemToDelete.name})` : ""}
            </span>
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel disabled={isDeleting}>Hủy</AlertDialogCancel>
          <AlertDialogAction 
            onClick={(e) => {
              e.preventDefault(); // Ngăn dialog tự đóng ngay lập tức
              performDelete();
            }}
            className="bg-red-600 hover:bg-red-700 focus:ring-red-600"
            disabled={isDeleting}
          >
            {isDeleting ? "Đang xóa..." : "Xóa"}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
    </div>
  );
};

export default SeatTypesListPage;

