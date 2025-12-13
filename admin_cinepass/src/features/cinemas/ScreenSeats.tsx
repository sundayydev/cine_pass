import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { ArrowLeft, Plus, Trash2, Grid3x3 } from "lucide-react";
import { toast } from "sonner";

// API Services
import { screenApi, type ScreenResponseDto } from "@/services/apiScreen";
import { seatApi, type SeatResponseDto, type SeatCreateDto } from "@/services/apiSeat";
import { seatTypeApi, type SeatTypeResponseDto } from "@/services/apiSeatType";
import { PATHS } from "@/config/paths";

// Shadcn UI
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Spinner } from "@/components/ui/spinner";

const ScreenSeatsPage = () => {
  const { cinemaId, roomId } = useParams<{ cinemaId: string; roomId: string }>();
  const navigate = useNavigate();

  const [screen, setScreen] = useState<ScreenResponseDto | null>(null);
  const [seats, setSeats] = useState<SeatResponseDto[]>([]);
  const [seatTypes, setSeatTypes] = useState<SeatTypeResponseDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isCreatingSeat, setIsCreatingSeat] = useState(false);

  // Form state for creating seat
  const [seatForm, setSeatForm] = useState({
    row: "",
    column: "",
    seatTypeCode: "",
    isActive: true,
  });

  useEffect(() => {
    if (roomId) {
      loadData();
    }
  }, [roomId]);

  const loadData = async () => {
    if (!roomId) return;

    try {
      setIsLoading(true);
      const [screenData, seatsData, seatTypesData] = await Promise.all([
        screenApi.getById(roomId),
        seatApi.getByScreenId(roomId),
        seatTypeApi.getAll(),
      ]);
      setScreen(screenData);
      setSeats(seatsData);
      setSeatTypes(seatTypesData);
      if (seatTypesData.length > 0 && !seatForm.seatTypeCode) {
        setSeatForm({ ...seatForm, seatTypeCode: seatTypesData[0].code });
      }
    } catch (error) {
      console.error("Error loading data:", error);
      toast.error("Lỗi khi tải thông tin");
      navigate(PATHS.CINEMAS);
    } finally {
      setIsLoading(false);
    }
  };

  const handleCreateSeat = async () => {
    if (!roomId) return;

    if (!seatForm.row || !seatForm.column || !seatForm.seatTypeCode) {
      toast.error("Vui lòng điền đầy đủ thông tin");
      return;
    }

    try {
      setIsCreatingSeat(true);
      const dto: SeatCreateDto = {
        screenId: roomId,
        row: seatForm.row.toUpperCase(),
        column: parseInt(seatForm.column),
        seatTypeCode: seatForm.seatTypeCode,
        isActive: seatForm.isActive,
      };

      await seatApi.create(dto);
      toast.success("Tạo ghế thành công");
      setSeatForm({ row: "", column: "", seatTypeCode: seatTypes[0]?.code || "", isActive: true });
      loadData();
    } catch (error) {
      console.error("Error creating seat:", error);
      toast.error(error instanceof Error ? error.message : "Lỗi khi tạo ghế");
    } finally {
      setIsCreatingSeat(false);
    }
  };

  const handleDeleteSeat = async (seatId: string) => {
    if (!confirm("Bạn có chắc chắn muốn xóa ghế này?")) {
      return;
    }

    try {
      await seatApi.delete(seatId);
      toast.success("Xóa ghế thành công");
      loadData();
    } catch (error) {
      console.error("Error deleting seat:", error);
      toast.error(error instanceof Error ? error.message : "Lỗi khi xóa ghế");
    }
  };

  // Group seats by row
  const seatsByRow = seats.reduce((acc, seat) => {
    if (!acc[seat.row]) {
      acc[seat.row] = [];
    }
    acc[seat.row].push(seat);
    return acc;
  }, {} as Record<string, SeatResponseDto[]>);

  // Sort rows and columns
  const sortedRows = Object.keys(seatsByRow).sort();
  sortedRows.forEach((row) => {
    seatsByRow[row].sort((a, b) => a.column - b.column);
  });

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <Spinner />
      </div>
    );
  }

  if (!screen) {
    return null;
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <Button
          variant="ghost"
          size="icon"
          onClick={() => navigate(PATHS.CINEMA_DETAIL.replace(":id", cinemaId || ""))}
        >
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <div className="flex-1">
          <h1 className="text-3xl font-bold tracking-tight">{screen.name}</h1>
          <p className="text-muted-foreground mt-1">Quản lý ghế ngồi trong phòng chiếu</p>
        </div>
      </div>

      {/* Screen Info */}
      <Card>
        <CardHeader>
          <CardTitle>Thông Tin Phòng Chiếu</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div>
              <p className="text-sm text-muted-foreground">Tên Phòng</p>
              <p className="text-lg font-semibold">{screen.name}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Sức Chứa</p>
              <p className="text-lg font-semibold">{screen.capacity}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Số Ghế Đã Tạo</p>
              <p className="text-lg font-semibold">{seats.length}</p>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Create Seat Form */}
      <Card>
        <CardHeader>
          <CardTitle>Thêm Ghế Mới</CardTitle>
          <CardDescription>Thêm ghế vào phòng chiếu</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            <div className="space-y-2">
              <Label htmlFor="row">Hàng (Row) *</Label>
              <Input
                id="row"
                placeholder="Ví dụ: A"
                value={seatForm.row}
                onChange={(e) => setSeatForm({ ...seatForm, row: e.target.value })}
                maxLength={2}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="column">Cột (Column) *</Label>
              <Input
                id="column"
                type="number"
                placeholder="Ví dụ: 1"
                value={seatForm.column}
                onChange={(e) => setSeatForm({ ...seatForm, column: e.target.value })}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="seatType">Loại Ghế *</Label>
              <Select
                value={seatForm.seatTypeCode}
                onValueChange={(value) => setSeatForm({ ...seatForm, seatTypeCode: value })}
              >
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {seatTypes.map((type) => (
                    <SelectItem key={type.code} value={type.code}>
                      {type.name} - {type.price.toLocaleString("vi-VN")}đ
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="flex items-end">
              <Button onClick={handleCreateSeat} disabled={isCreatingSeat} className="w-full">
                {isCreatingSeat ? (
                  <Spinner className="mr-2 h-4 w-4" />
                ) : (
                  <Plus className="mr-2 h-4 w-4" />
                )}
                Thêm Ghế
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Seats Grid */}
      <Card>
        <CardHeader>
          <CardTitle>Sơ Đồ Ghế</CardTitle>
          <CardDescription>Bố trí ghế trong phòng chiếu</CardDescription>
        </CardHeader>
        <CardContent>
          {seats.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-12">
              <Grid3x3 className="h-12 w-12 text-muted-foreground mb-4" />
              <p className="text-muted-foreground">Chưa có ghế nào được tạo</p>
            </div>
          ) : (
            <div className="space-y-4">
              {/* Screen indicator */}
              <div className="text-center py-2 bg-muted rounded-lg mb-4">
                <p className="text-sm font-medium">Màn hình</p>
              </div>

              {/* Seats by row */}
              {sortedRows.map((row) => (
                <div key={row} className="flex items-center gap-2">
                  <div className="w-8 text-center font-semibold">{row}</div>
                  <div className="flex-1 flex gap-1 flex-wrap">
                    {seatsByRow[row].map((seat) => {
                      const seatType = seatTypes.find((t) => t.code === seat.seatTypeCode);
                      return (
                        <div
                          key={seat.id}
                          className="relative group"
                          title={`${seat.row}${seat.column} - ${seatType?.name || seat.seatTypeCode}`}
                        >
                          <div
                            className={`w-10 h-10 rounded border-2 flex items-center justify-center text-xs font-medium transition-all ${
                              seat.isActive
                                ? "bg-emerald-100 border-emerald-500 text-emerald-700 hover:bg-emerald-200"
                                : "bg-gray-100 border-gray-300 text-gray-500"
                            }`}
                          >
                            {seat.column}
                          </div>
                          <Button
                            variant="ghost"
                            size="icon"
                            className="absolute -top-2 -right-2 h-5 w-5 opacity-0 group-hover:opacity-100 transition-opacity bg-destructive text-destructive-foreground hover:bg-destructive/90"
                            onClick={() => handleDeleteSeat(seat.id)}
                          >
                            <Trash2 className="h-3 w-3" />
                          </Button>
                        </div>
                      );
                    })}
                  </div>
                </div>
              ))}

              {/* Legend */}
              <div className="mt-6 pt-4 border-t flex flex-wrap gap-4">
                <div className="flex items-center gap-2">
                  <div className="w-6 h-6 rounded border-2 bg-emerald-100 border-emerald-500"></div>
                  <span className="text-sm">Ghế đang hoạt động</span>
                </div>
                <div className="flex items-center gap-2">
                  <div className="w-6 h-6 rounded border-2 bg-gray-100 border-gray-300"></div>
                  <span className="text-sm">Ghế ngừng hoạt động</span>
                </div>
              </div>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
};

export default ScreenSeatsPage;

