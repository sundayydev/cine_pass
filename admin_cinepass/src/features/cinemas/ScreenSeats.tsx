import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { ArrowLeft, Plus, Trash2, Grid3x3, Pencil, X } from "lucide-react";
import { toast } from "sonner";

// API Services
import { screenApi, type ScreenResponseDto } from "@/services/apiScreen";
import { seatApi, type SeatResponseDto, type SeatCreateDto, type SeatUpdateDto } from "@/services/apiSeat";
import { seatTypeApi, type SeatTypeResponseDto } from "@/services/apiSeatType";
import { PATHS } from "@/config/paths";

// Shadcn UI
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
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
// Dialog will be created inline if not available

const ScreenSeatsPage = () => {
  const { cinemaId, roomId } = useParams<{ cinemaId: string; roomId: string }>();
  const navigate = useNavigate();

  const [screen, setScreen] = useState<ScreenResponseDto | null>(null);
  const [seats, setSeats] = useState<SeatResponseDto[]>([]);
  const [seatTypes, setSeatTypes] = useState<SeatTypeResponseDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isCreatingSeat, setIsCreatingSeat] = useState(false);
  const [isEditingSeat, setIsEditingSeat] = useState(false);
  const [editingSeat, setEditingSeat] = useState<SeatResponseDto | null>(null);

  // Form state for creating seat
  const [seatForm, setSeatForm] = useState({
    seatRow: "",
    seatNumber: "",
    seatTypeCode: "",
    isActive: true,
  });

  // Form state for editing seat
  const [editForm, setEditForm] = useState({
    seatTypeCode: "",
    isActive: true,
  });

  // Get seat type color
  const getSeatTypeColor = (seatTypeCode?: string) => {
    if (!seatTypeCode) return "bg-gray-100 border-gray-300 text-gray-700";
    
    const code = seatTypeCode.toUpperCase();
    if (code === "VIP") return "bg-purple-500 border-purple-600 text-white";
    if (code === "COUPLE") return "bg-pink-500 border-pink-600 text-white";
    if (code === "NORMAL") return "bg-blue-500 border-blue-600 text-white";
    
    // Default color for other types
    return "bg-emerald-500 border-emerald-600 text-white";
  };

  // Get seat type name
  const getSeatTypeName = (seatTypeCode?: string) => {
    if (!seatTypeCode) return "Thường";
    const seatType = seatTypes.find((t) => t.code === seatTypeCode);
    return seatType?.name || seatTypeCode;
  };

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
        setSeatForm((prev) => ({ ...prev, seatTypeCode: seatTypesData[0].code }));
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

    if (!seatForm.seatRow || !seatForm.seatNumber) {
      toast.error("Vui lòng điền đầy đủ thông tin");
      return;
    }

    try {
      setIsCreatingSeat(true);
      const seatRow = seatForm.seatRow.toUpperCase();
      const seatNumber = parseInt(seatForm.seatNumber);
      const seatCode = `${seatRow}${seatNumber}`;
      
      const dto: SeatCreateDto = {
        screenId: roomId,
        seatRow: seatRow,
        seatNumber: seatNumber,
        seatCode: seatCode,
        seatTypeCode: seatForm.seatTypeCode || undefined,
        isActive: seatForm.isActive,
      };

      await seatApi.create(dto);
      toast.success("Tạo ghế thành công");
      setSeatForm({ 
        seatRow: "", 
        seatNumber: "", 
        seatTypeCode: seatTypes[0]?.code || "", 
        isActive: true 
      });
      loadData();
    } catch (error) {
      console.error("Error creating seat:", error);
      toast.error(error instanceof Error ? error.message : "Lỗi khi tạo ghế");
    } finally {
      setIsCreatingSeat(false);
    }
  };

  const handleEditSeat = (seat: SeatResponseDto) => {
    setEditingSeat(seat);
    setEditForm({
      seatTypeCode: seat.seatTypeCode || "",
      isActive: seat.isActive,
    });
    setIsEditingSeat(true);
  };

  const handleUpdateSeat = async () => {
    if (!editingSeat) return;

    try {
      setIsEditingSeat(false);
      const dto: SeatUpdateDto = {
        seatTypeCode: editForm.seatTypeCode || undefined,
        isActive: editForm.isActive,
      };

      await seatApi.update(editingSeat.id, dto);
      toast.success("Cập nhật ghế thành công");
      loadData();
      setEditingSeat(null);
    } catch (error) {
      console.error("Error updating seat:", error);
      toast.error(error instanceof Error ? error.message : "Lỗi khi cập nhật ghế");
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
    if (!acc[seat.seatRow]) {
      acc[seat.seatRow] = [];
    }
    acc[seat.seatRow].push(seat);
    return acc;
  }, {} as Record<string, SeatResponseDto[]>);

  // Sort rows and seat numbers
  const sortedRows = Object.keys(seatsByRow).sort();
  sortedRows.forEach((row) => {
    seatsByRow[row].sort((a, b) => a.seatNumber - b.seatNumber);
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
              <p className="text-lg font-semibold">{screen.totalSeats}</p>
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
              <Label htmlFor="seatRow">Hàng (Row) *</Label>
              <Input
                id="seatRow"
                placeholder="Ví dụ: A"
                value={seatForm.seatRow}
                onChange={(e) => setSeatForm({ ...seatForm, seatRow: e.target.value })}
                maxLength={10}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="seatNumber">Số Ghế *</Label>
              <Input
                id="seatNumber"
                type="number"
                placeholder="Ví dụ: 1"
                value={seatForm.seatNumber}
                onChange={(e) => setSeatForm({ ...seatForm, seatNumber: e.target.value })}
                min="1"
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="seatType">Loại Ghế</Label>
              <Select
                value={seatForm.seatTypeCode}
                onValueChange={(value) => setSeatForm({ ...seatForm, seatTypeCode: value })}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Chọn loại ghế (tùy chọn)" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="null" disabled>Không có</SelectItem>
                  {seatTypes.map((type) => (
                    <SelectItem key={type.code} value={type.code}>
                      {type.name || type.code} - {(type.surchargeRate * 100).toFixed(0)}%
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
                  <div className="w-10 text-center font-semibold text-lg">{row}</div>
                  <div className="flex-1 flex gap-1 flex-wrap">
                    {seatsByRow[row].map((seat) => {
                      const seatColor = getSeatTypeColor(seat.seatTypeCode);
                      return (
                        <div
                          key={seat.id}
                          className="relative group"
                        >
                          <button
                            type="button"
                            onClick={() => handleEditSeat(seat)}
                            className={`w-12 h-12 rounded-lg border-2 flex flex-col items-center justify-center text-xs font-semibold transition-all cursor-pointer hover:scale-110 hover:shadow-lg ${
                              !seat.isActive
                                ? "opacity-50 grayscale"
                                : seatColor
                            }`}
                            title={`${seat.seatCode} - ${getSeatTypeName(seat.seatTypeCode)}${!seat.isActive ? " (Ngừng hoạt động)" : ""}`}
                          >
                            <span className="text-[10px] leading-tight">{seat.seatNumber}</span>
                            {seat.seatTypeCode && (
                              <span className="text-[8px] leading-tight opacity-90">
                                {seat.seatTypeCode}
                              </span>
                            )}
                          </button>
                          <Button
                            variant="ghost"
                            size="icon"
                            className="absolute -top-2 -right-2 h-6 w-6 opacity-0 group-hover:opacity-100 transition-opacity bg-destructive text-destructive-foreground hover:bg-destructive/90 z-10"
                            onClick={(e) => {
                              e.stopPropagation();
                              handleDeleteSeat(seat.id);
                            }}
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
              <div className="mt-6 pt-4 border-t space-y-3">
                <div className="flex flex-wrap gap-4">
                  <div className="flex items-center gap-2">
                    <div className="w-8 h-8 rounded-lg border-2 bg-blue-500 border-blue-600"></div>
                    <span className="text-sm font-medium">NORMAL - Ghế Thường</span>
                  </div>
                  <div className="flex items-center gap-2">
                    <div className="w-8 h-8 rounded-lg border-2 bg-purple-500 border-purple-600"></div>
                    <span className="text-sm font-medium">VIP - Ghế VIP</span>
                  </div>
                  <div className="flex items-center gap-2">
                    <div className="w-8 h-8 rounded-lg border-2 bg-pink-500 border-pink-600"></div>
                    <span className="text-sm font-medium">COUPLE - Ghế Đôi</span>
                  </div>
                  <div className="flex items-center gap-2">
                    <div className="w-8 h-8 rounded-lg border-2 bg-gray-100 border-gray-300"></div>
                    <span className="text-sm font-medium">Không có loại</span>
                  </div>
                </div>
                <div className="flex flex-wrap gap-4 pt-2 border-t">
                  <div className="flex items-center gap-2">
                    <div className="w-6 h-6 rounded border-2 bg-emerald-500 border-emerald-600"></div>
                    <span className="text-sm">Đang hoạt động</span>
                  </div>
                  <div className="flex items-center gap-2">
                    <div className="w-6 h-6 rounded border-2 bg-gray-300 border-gray-400 opacity-50 grayscale"></div>
                    <span className="text-sm">Ngừng hoạt động</span>
                  </div>
                  <div className="flex items-center gap-2 text-muted-foreground">
                    <Pencil className="w-4 h-4" />
                    <span className="text-sm">Click vào ghế để chỉnh sửa</span>
                  </div>
                </div>
              </div>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Edit Seat Modal */}
      {isEditingSeat && editingSeat && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
          <Card className="w-full max-w-md mx-4">
            <CardHeader>
              <div className="flex items-center justify-between">
                <div>
                  <CardTitle>Chỉnh Sửa Ghế</CardTitle>
                  <CardDescription>
                    Chỉnh sửa thông tin ghế {editingSeat.seatCode}
                  </CardDescription>
                </div>
                <Button
                  variant="ghost"
                  size="icon"
                  onClick={() => {
                    setIsEditingSeat(false);
                    setEditingSeat(null);
                  }}
                >
                  <X className="h-4 w-4" />
                </Button>
              </div>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                <div className="space-y-2">
                  <Label>Thông tin ghế</Label>
                  <div className="p-3 bg-muted rounded-lg">
                    <p className="text-sm">
                      <span className="font-semibold">Mã ghế:</span> {editingSeat.seatCode}
                    </p>
                    <p className="text-sm">
                      <span className="font-semibold">Hàng:</span> {editingSeat.seatRow}
                    </p>
                    <p className="text-sm">
                      <span className="font-semibold">Số ghế:</span> {editingSeat.seatNumber}
                    </p>
                  </div>
                </div>

                <div className="space-y-2">
                  <Label htmlFor="editSeatType">Loại Ghế</Label>
                  <Select
                    value={editForm.seatTypeCode}
                    onValueChange={(value) => setEditForm({ ...editForm, seatTypeCode: value })}
                  >
                    <SelectTrigger id="editSeatType">
                      <SelectValue placeholder="Chọn loại ghế" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="null">Không có loại</SelectItem>
                      {seatTypes.map((type) => (
                        <SelectItem key={type.code} value={type.code}>
                          <div className="flex items-center gap-2">
                            <div className={`w-4 h-4 rounded ${getSeatTypeColor(type.code).split(' ')[0]}`}></div>
                            <span>{type.name || type.code}</span>
                            <span className="text-muted-foreground">
                              ({(type.surchargeRate * 100).toFixed(0)}%)
                            </span>
                          </div>
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>

                <div className="space-y-2">
                  <Label htmlFor="editIsActive">Trạng Thái</Label>
                  <Select
                    value={editForm.isActive ? "active" : "inactive"}
                    onValueChange={(value) => setEditForm({ ...editForm, isActive: value === "active" })}
                  >
                    <SelectTrigger id="editIsActive">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="active">Đang hoạt động</SelectItem>
                      <SelectItem value="inactive">Ngừng hoạt động</SelectItem>
                    </SelectContent>
                  </Select>
                </div>

                {editForm.seatTypeCode && (
                  <div className="p-3 bg-muted rounded-lg">
                    <p className="text-sm font-semibold mb-1">Preview:</p>
                    <div className="flex items-center gap-2">
                      <div className={`w-12 h-12 rounded-lg border-2 flex flex-col items-center justify-center text-xs font-semibold ${getSeatTypeColor(editForm.seatTypeCode)}`}>
                        <span>{editingSeat.seatNumber}</span>
                        <span className="text-[8px]">{editForm.seatTypeCode}</span>
                      </div>
                      <div>
                        <p className="text-sm font-medium">{getSeatTypeName(editForm.seatTypeCode)}</p>
                        <p className="text-xs text-muted-foreground">
                          {editForm.isActive ? "Đang hoạt động" : "Ngừng hoạt động"}
                        </p>
                      </div>
                    </div>
                  </div>
                )}
              </div>
            </CardContent>
            <CardContent className="flex justify-end gap-2 pt-0">
              <Button
                variant="outline"
                onClick={() => {
                  setIsEditingSeat(false);
                  setEditingSeat(null);
                }}
              >
                Hủy
              </Button>
              <Button onClick={handleUpdateSeat}>
                Cập Nhật
              </Button>
            </CardContent>
          </Card>
        </div>
      )}
    </div>
  );
};

export default ScreenSeatsPage;

