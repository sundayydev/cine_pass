import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import {
  Plus,
  Search,
  Pencil,
  Trash2,
  Armchair,
  Crown,
  Heart,
  Accessibility,
  Sofa,
  AlertCircle,
  Sparkles
} from "lucide-react";
import { toast } from "sonner";

// API Services
import { seatTypeApi, type SeatTypeResponseDto } from "@/services/apiSeatType";

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
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle } from "@/components/ui/alert-dialog";
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "@/components/ui/tooltip";

// Hooks
import { useDebounce } from "@/hooks/useDebounce";
import { cn } from "@/lib/utils";

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

  const promptDelete = (code: string, name: string) => {
    setItemToDelete({ code, name });
  };

  const performDelete = async () => {
    if (!itemToDelete) return;
    try {
      setIsDeleting(true);
      await seatTypeApi.delete(itemToDelete.code);
      toast.success("Xóa loại ghế thành công");
      loadSeatTypes();
      setItemToDelete(null);
    } catch (error: any) {
      toast.error(error.message || "Lỗi khi xóa loại ghế");
    } finally {
      setIsDeleting(false);
    }
  };

  // --- Helper: Get Visuals based on Seat Type Code/Name ---
  const getSeatVisuals = (code: string, name: string = "") => {
    const text = (code + " " + name).toUpperCase();

    if (text.includes("VIP") || text.includes("GOLD") || text.includes("PRIME")) {
      return {
        icon: Crown,
        color: "text-amber-500",
        bg: "bg-amber-500/10 border-amber-200",
        label: "Hạng sang"
      };
    }
    if (text.includes("COUPLE") || text.includes("DOUBLE") || text.includes("LOVE")) {
      return {
        icon: Heart,
        color: "text-rose-500",
        bg: "bg-rose-500/10 border-rose-200",
        label: "Ghế đôi"
      };
    }
    if (text.includes("BED") || text.includes("SOFA") || text.includes("SWEET")) {
      return {
        icon: Sofa,
        color: "text-indigo-500",
        bg: "bg-indigo-500/10 border-indigo-200",
        label: "Sofa/Giường"
      };
    }
    if (text.includes("DISABLE") || text.includes("WHEEL")) {
      return {
        icon: Accessibility,
        color: "text-blue-500",
        bg: "bg-blue-500/10 border-blue-200",
        label: "Hỗ trợ"
      };
    }
    // Default / Standard
    return {
      icon: Armchair,
      color: "text-slate-500",
      bg: "bg-slate-100 border-slate-200 dark:bg-slate-800 dark:border-slate-700",
      label: "Tiêu chuẩn"
    };
  };

  return (
    <div className="space-y-8 p-1">
      {/* Header Section */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-3xl font-bold tracking-tight bg-gradient-to-r from-primary to-purple-600 bg-clip-text text-transparent">
            Cấu hình Loại Ghế
          </h1>
          <p className="text-muted-foreground mt-1 text-sm">
            Quản lý các hạng ghế, icon hiển thị và tỷ lệ giá vé phụ thu.
          </p>
        </div>
        <Button
          onClick={() => navigate("/seat-types/create")}
          className="shadow-lg shadow-primary/20 transition-all hover:scale-105"
        >
          <Plus className="mr-2 h-4 w-4" />
          Thêm Loại Ghế
        </Button>
      </div>

      {/* Main Content */}
      <Card className="border-none shadow-md bg-card/50 backdrop-blur-sm">
        <CardHeader className="pb-3">
          <div className="flex items-center justify-between">
            <CardTitle className="text-xl">Danh sách</CardTitle>
            <div className="relative w-full max-w-sm">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
              <Input
                placeholder="Tìm theo mã hoặc tên..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-10 bg-background"
              />
            </div>
          </div>
        </CardHeader>

        <CardContent className="p-0">
          <div className="rounded-md border bg-background">
            <Table>
              <TableHeader>
                <TableRow className="bg-muted/50 hover:bg-muted/50">
                  <TableHead className="w-[80px] text-center">Icon</TableHead>
                  <TableHead className="w-[150px]">Mã Code</TableHead>
                  <TableHead>Tên hiển thị</TableHead>
                  <TableHead>Phụ thu (Surcharge)</TableHead>
                  <TableHead className="text-right pr-6">Hành động</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {isLoading ? (
                  <TableRow>
                    <TableCell colSpan={5} className="h-32 text-center text-muted-foreground">
                      <div className="flex items-center justify-center gap-2">
                        <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-primary"></div>
                        Đang tải dữ liệu...
                      </div>
                    </TableCell>
                  </TableRow>
                ) : seatTypes.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={5} className="h-48 text-center">
                      <div className="flex flex-col items-center justify-center gap-2">
                        <div className="h-12 w-12 rounded-full bg-muted flex items-center justify-center">
                          <Armchair className="h-6 w-6 text-muted-foreground/50" />
                        </div>
                        <p className="text-muted-foreground font-medium">Chưa có loại ghế nào</p>
                        <p className="text-xs text-muted-foreground">Hãy tạo loại ghế mới để bắt đầu thiết lập sơ đồ phòng chiếu.</p>
                      </div>
                    </TableCell>
                  </TableRow>
                ) : (
                  seatTypes.map((seatType) => {
                    const visual = getSeatVisuals(seatType.code, seatType.name || "");
                    const Icon = visual.icon;

                    return (
                      <TableRow key={seatType.code} className="group hover:bg-muted/30 transition-colors">
                        {/* Column: Icon Visualization */}
                        <TableCell className="text-center">
                          <TooltipProvider>
                            <Tooltip>
                              <TooltipTrigger asChild>
                                <div className={cn("mx-auto h-10 w-10 rounded-lg flex items-center justify-center border transition-all group-hover:scale-110", visual.bg)}>
                                  <Icon className={cn("h-5 w-5", visual.color)} />
                                </div>
                              </TooltipTrigger>
                              <TooltipContent>
                                <p>{visual.label}</p>
                              </TooltipContent>
                            </Tooltip>
                          </TooltipProvider>
                        </TableCell>

                        {/* Column: Code */}
                        <TableCell>
                          <Badge variant="outline" className="font-mono text-xs px-2 py-1 uppercase tracking-wider">
                            {seatType.code}
                          </Badge>
                        </TableCell>

                        {/* Column: Name */}
                        <TableCell>
                          <div className="font-medium text-foreground">
                            {seatType.name || <span className="text-muted-foreground italic">Chưa đặt tên</span>}
                          </div>
                        </TableCell>

                        {/* Column: Surcharge */}
                        <TableCell>
                          <div className="flex items-center gap-2">
                            {seatType.surchargeRate > 0 ? (
                              <Badge className="bg-emerald-100 text-emerald-700 hover:bg-emerald-100 border-emerald-200 dark:bg-emerald-900/30 dark:text-emerald-400 dark:border-emerald-800">
                                <Sparkles className="w-3 h-3 mr-1" />
                                +{(seatType.surchargeRate * 100).toFixed(0)}%
                              </Badge>
                            ) : (
                              <Badge variant="secondary" className="text-muted-foreground bg-slate-100 dark:bg-slate-800">
                                Tiêu chuẩn
                              </Badge>
                            )}
                          </div>
                        </TableCell>

                        {/* Column: Actions */}
                        <TableCell className="text-right pr-6">
                          <div className="flex items-center justify-end gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                            <Button
                              variant="ghost"
                              size="icon"
                              className="h-8 w-8 text-muted-foreground hover:text-primary hover:bg-primary/10"
                              onClick={() => navigate(`/seat-types/edit/${seatType.code}`)}
                            >
                              <Pencil className="h-4 w-4" />
                            </Button>
                            <Button
                              variant="ghost"
                              size="icon"
                              className="h-8 w-8 text-muted-foreground hover:text-red-600 hover:bg-red-50"
                              onClick={() => promptDelete(seatType.code, seatType.name || "")}
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
              Bạn có chắc chắn muốn xóa loại ghế <span className="font-bold text-foreground">{itemToDelete?.code}</span> không?
              <br />
              Dữ liệu liên quan đến sơ đồ ghế trong các phòng chiếu có thể bị ảnh hưởng.
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

export default SeatTypesListPage;