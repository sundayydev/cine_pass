import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { ArrowLeft, Plus, Trash2, Grid3x3, Pencil, X, Monitor, Armchair, Sparkles } from "lucide-react";
import { toast } from "sonner";
import { cn } from "@/lib/utils"; // Giả sử bạn có utility này từ shadcn

// API Services
import { screenApi, type ScreenResponseDto } from "@/services/apiScreen";
import { seatApi, type SeatResponseDto, type SeatCreateDto, type SeatUpdateDto, type SeatGenerateDto } from "@/services/apiSeat";
import { seatTypeApi, type SeatTypeResponseDto } from "@/services/apiSeatType";
import { PATHS } from "@/config/paths";

// Shadcn UI
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle, CardFooter } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Badge } from "@/components/ui/badge";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Spinner } from "@/components/ui/spinner";
import { Separator } from "@/components/ui/separator";

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

  // Form state for auto-generating seats
  const [generateForm, setGenerateForm] = useState({
    rows: 10,
    seatsPerRow: 12,
    defaultSeatTypeCode: "",
  });
  const [isGenerating, setIsGenerating] = useState(false);

  // --- HELPERS ---

  // Check if seat is Couple type
  const isCoupleSeat = (seatTypeCode?: string) => {
    return seatTypeCode?.toUpperCase().includes("COUPLE");
  };

  // Get seat styling based on type
  const getSeatStyle = (seatTypeCode?: string, isActive: boolean = true) => {
    if (!isActive) return "bg-gray-200 text-gray-400 border-gray-300 cursor-not-allowed";

    const code = seatTypeCode?.toUpperCase() || "NORMAL";

    if (code.includes("VIP")) return "bg-amber-500 border-amber-600 text-white shadow-amber-200";
    if (code.includes("COUPLE")) return "bg-pink-500 border-pink-600 text-white shadow-pink-200";

    // Normal/Default
    return "bg-sky-500 border-sky-600 text-white shadow-sky-200";
  };

  const getSeatTypeName = (seatTypeCode?: string) => {
    if (!seatTypeCode) return "Thường";
    const seatType = seatTypes.find((t) => t.code === seatTypeCode);
    return seatType?.name || seatTypeCode;
  };

  // --- API LOAD ---
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

      // Set default select value if needed
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

  // --- HANDLERS ---
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
      toast.success(`Đã thêm ghế ${seatCode}`);
      // Reset form logic...
      loadData();
    } catch (error: any) {
      toast.error(error.message || "Lỗi khi tạo ghế");
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
    } catch (error: any) {
      toast.error(error.message || "Lỗi cập nhật");
    }
  };

  const handleDeleteSeat = async (seatId: string) => {
    if (!confirm("Bạn có chắc chắn muốn xóa ghế này?")) return;
    try {
      await seatApi.delete(seatId);
      toast.success("Đã xóa ghế");
      loadData();
    } catch (error: any) {
      toast.error(error.message || "Lỗi xóa ghế");
    }
  };

  const handleGenerateSeats = async () => {
    if (!roomId) return;
    if (!confirm(`Hành động này sẽ XÓA TOÀN BỘ ghế cũ và tạo mới ${generateForm.rows * generateForm.seatsPerRow} ghế. Tiếp tục?`)) return;

    try {
      setIsGenerating(true);
      const dto: SeatGenerateDto = {
        screenId: roomId,
        rows: generateForm.rows,
        seatsPerRow: generateForm.seatsPerRow,
        defaultSeatTypeCode: generateForm.defaultSeatTypeCode === 'none' ? undefined : generateForm.defaultSeatTypeCode,
      };
      await seatApi.generateSeats(dto);
      toast.success("Tạo sơ đồ ghế thành công!");
      loadData();
    } catch (error: any) {
      toast.error(error.message || "Lỗi tạo ghế tự động");
    } finally {
      setIsGenerating(false);
    }
  };

  // --- DATA PROCESSING ---
  const seatsByRow = seats.reduce((acc, seat) => {
    if (!acc[seat.seatRow]) acc[seat.seatRow] = [];
    acc[seat.seatRow].push(seat);
    return acc;
  }, {} as Record<string, SeatResponseDto[]>);

  const sortedRows = Object.keys(seatsByRow).sort();
  sortedRows.forEach((row) => {
    seatsByRow[row].sort((a, b) => a.seatNumber - b.seatNumber);
  });

  if (isLoading) return <div className="h-screen flex items-center justify-center"><Spinner className="w-10 h-10" /></div>;
  if (!screen) return null;

  return (
    <div className="min-h-screen bg-slate-50/50 pb-20">
      {/* --- HEADER --- */}
      <div className="bg-white border-b sticky top-0 z-30 px-6 py-4 shadow-sm flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" onClick={() => navigate(PATHS.CINEMA_DETAIL.replace(":id", cinemaId || ""))}>
            <ArrowLeft className="h-5 w-5" />
          </Button>
          <div>
            <h1 className="text-2xl font-bold flex items-center gap-2">
              {screen.name}
              <Badge variant="outline" className="text-muted-foreground font-normal">{seats.length} ghế</Badge>
            </h1>
          </div>
        </div>
      </div>

      <div className="container mx-auto max-w-7xl p-6 grid grid-cols-1 lg:grid-cols-12 gap-6">

        {/* --- LEFT COLUMN: CONFIGURATION (3/12) --- */}
        <div className="lg:col-span-3 space-y-6">

          {/* Info Card */}
          <Card>
            <CardHeader className="pb-3">
              <CardTitle className="text-base">Thông số phòng</CardTitle>
            </CardHeader>
            <CardContent className="space-y-2 text-sm">
              <div className="flex justify-between">
                <span className="text-muted-foreground">Tổng sức chứa:</span>
                <span className="font-medium">{screen.totalSeats}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">Số hàng ghế:</span>
                <span className="font-medium">{sortedRows.length}</span>
              </div>
            </CardContent>
          </Card>

          {/* Tools Tabs */}
          <Tabs defaultValue="create" className="w-full">
            <TabsList className="w-full grid grid-cols-2 mb-2">
              <TabsTrigger value="create">Tạo Lẻ</TabsTrigger>
              <TabsTrigger value="auto">Tự Động</TabsTrigger>
            </TabsList>

            {/* TAB: CREATE SINGLE */}
            <TabsContent value="create">
              <Card>
                <CardHeader>
                  <CardTitle className="text-sm flex items-center gap-2"><Plus className="w-4 h-4" /> Thêm ghế thủ công</CardTitle>
                </CardHeader>
                <CardContent className="space-y-3">
                  <div className="grid grid-cols-2 gap-2">
                    <div className="space-y-1">
                      <Label className="text-xs">Hàng (A-Z)</Label>
                      <Input
                        placeholder="A"
                        value={seatForm.seatRow}
                        onChange={e => setSeatForm({ ...seatForm, seatRow: e.target.value.toUpperCase() })}
                        maxLength={3}
                      />
                    </div>
                    <div className="space-y-1">
                      <Label className="text-xs">Số ghế</Label>
                      <Input
                        type="number"
                        placeholder="1"
                        value={seatForm.seatNumber}
                        onChange={e => setSeatForm({ ...seatForm, seatNumber: e.target.value })}
                      />
                    </div>
                  </div>
                  <div className="space-y-1">
                    <Label className="text-xs">Loại ghế</Label>
                    <Select value={seatForm.seatTypeCode} onValueChange={v => setSeatForm({ ...seatForm, seatTypeCode: v })}>
                      <SelectTrigger><SelectValue placeholder="Chọn loại" /></SelectTrigger>
                      <SelectContent>
                        {seatTypes.map(t => (
                          <SelectItem key={t.code} value={t.code}>{t.name} ({t.code})</SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                  <Button onClick={handleCreateSeat} disabled={isCreatingSeat} className="w-full mt-2">
                    {isCreatingSeat ? <Spinner className="w-4 h-4" /> : "Thêm ghế"}
                  </Button>
                </CardContent>
              </Card>
            </TabsContent>

            {/* TAB: AUTO GENERATE */}
            <TabsContent value="auto">
              <Card className="border-indigo-100 bg-indigo-50/30">
                <CardHeader>
                  <CardTitle className="text-sm flex items-center gap-2"><Grid3x3 className="w-4 h-4 text-indigo-600" /> Tạo sơ đồ tự động</CardTitle>
                </CardHeader>
                <CardContent className="space-y-3">
                  <div className="grid grid-cols-2 gap-2">
                    <div className="space-y-1">
                      <Label className="text-xs">Số hàng</Label>
                      <Input type="number" value={generateForm.rows} onChange={e => setGenerateForm({ ...generateForm, rows: +e.target.value })} />
                    </div>
                    <div className="space-y-1">
                      <Label className="text-xs">Số ghế/hàng</Label>
                      <Input type="number" value={generateForm.seatsPerRow} onChange={e => setGenerateForm({ ...generateForm, seatsPerRow: +e.target.value })} />
                    </div>
                  </div>
                  <div className="space-y-1">
                    <Label className="text-xs">Loại mặc định</Label>
                    <Select value={generateForm.defaultSeatTypeCode} onValueChange={v => setGenerateForm({ ...generateForm, defaultSeatTypeCode: v })}>
                      <SelectTrigger><SelectValue placeholder="Mặc định" /></SelectTrigger>
                      <SelectContent>
                        <SelectItem value="none">Không chọn</SelectItem>
                        {seatTypes.map(t => (
                          <SelectItem key={t.code} value={t.code}>{t.name}</SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                  <Button variant="destructive" onClick={handleGenerateSeats} disabled={isGenerating} className="w-full mt-2">
                    {isGenerating ? <Spinner className="w-4 h-4" /> : "Tạo lại sơ đồ"}
                  </Button>
                  <p className="text-[10px] text-muted-foreground text-center">Lưu ý: Sẽ xóa toàn bộ dữ liệu ghế cũ.</p>
                </CardContent>
              </Card>
            </TabsContent>
          </Tabs>
        </div>

        {/* --- RIGHT COLUMN: SEAT MAP (9/12) --- */}
        <div className="lg:col-span-9">
          <Card className="h-full border-none shadow-md bg-white">
            <CardHeader className="pb-2 border-b">
              <div className="flex justify-between items-center">
                <CardTitle>Sơ đồ ghế ngồi</CardTitle>

                {/* Legend */}
                <div className="flex gap-4 text-xs">
                  <div className="flex items-center gap-1.5"><div className="w-3 h-3 rounded bg-sky-500"></div> Thường</div>
                  <div className="flex items-center gap-1.5"><div className="w-3 h-3 rounded bg-amber-500"></div> VIP</div>
                  <div className="flex items-center gap-1.5"><div className="w-8 h-3 rounded bg-pink-500"></div> Đôi (Couple)</div>
                  <div className="flex items-center gap-1.5"><div className="w-3 h-3 rounded bg-gray-200"></div> Bảo trì</div>
                </div>
              </div>
            </CardHeader>

            <CardContent className="p-8 bg-slate-50 min-h-[600px] flex flex-col items-center">

              {/* 1. SCREEN VISUAL */}
              <div className="w-full max-w-2xl mb-12 relative group">
                <div className="h-2 bg-slate-300 rounded-[50%] shadow-[0_15px_15px_-5px_rgba(0,0,0,0.1)] w-full mx-auto transform -scale-x-100"></div>
                <div className="absolute -top-6 left-0 right-0 text-center">
                  <span className="text-xs font-bold text-slate-400 uppercase tracking-[0.3em] flex items-center justify-center gap-2">
                    <Monitor className="w-4 h-4" /> Màn hình chiếu
                  </span>
                </div>
                {/* Light beam effect */}
                <div className="absolute top-2 left-[10%] right-[10%] h-16 bg-gradient-to-b from-slate-200/50 to-transparent clip-path-trapezoid opacity-0 group-hover:opacity-100 transition-opacity duration-500"></div>
              </div>

              {/* 2. SEAT MAP CONTAINER */}
              <div className="w-full overflow-x-auto pb-12 custom-scrollbar">
                {seats.length === 0 ? (
                  <div className="flex flex-col items-center justify-center py-20 text-muted-foreground">
                    <Armchair className="w-16 h-16 mb-4 opacity-20" />
                    <p>Chưa có ghế nào được thiết lập.</p>
                    <Button variant="link" onClick={() => (document.querySelector('[value="auto"]') as HTMLElement)?.click()}>
                      Tạo tự động ngay
                    </Button>
                  </div>
                ) : (
                  <div className="flex flex-col gap-3 min-w-max px-8">
                    {sortedRows.map(row => (
                      <div key={row} className="flex items-center justify-center gap-3 group/row hover:bg-slate-100/50 rounded-lg py-1 px-2 transition-colors">

                        {/* Row Label (Left) */}
                        <div className="w-8 text-right font-bold text-slate-400 text-sm">{row}</div>

                        {/* Seats in Row */}
                        <div className="flex items-center gap-2">
                          {seatsByRow[row].map(seat => {
                            const isCouple = isCoupleSeat(seat.seatTypeCode);

                            return (
                              <div key={seat.id} className="relative group/seat">
                                <button
                                  onClick={() => handleEditSeat(seat)}
                                  className={cn(
                                    "relative flex items-center justify-center text-[10px] font-bold transition-all duration-200 hover:-translate-y-1 hover:shadow-lg hover:ring-2 ring-offset-1 ring-primary/50",
                                    // Styling shape
                                    "h-9 rounded-t-lg rounded-b-md border-b-4",
                                    // Width logic based on type
                                    isCouple ? "w-20 tracking-widest" : "w-9",
                                    // Color logic
                                    getSeatStyle(seat.seatTypeCode, seat.isActive)
                                  )}
                                >
                                  {seat.seatNumber}

                                  {/* Tooltip on hover */}
                                  <div className="absolute -top-8 left-1/2 -translate-x-1/2 bg-black text-white text-[10px] py-1 px-2 rounded opacity-0 group-hover/seat:opacity-100 whitespace-nowrap z-10 pointer-events-none transition-opacity">
                                    {seat.seatCode} - {getSeatTypeName(seat.seatTypeCode)}
                                  </div>
                                </button>

                                {/* Quick Delete Action */}
                                <button
                                  onClick={(e) => { e.stopPropagation(); handleDeleteSeat(seat.id); }}
                                  className="absolute -top-2 -right-2 bg-white text-red-500 rounded-full p-0.5 shadow-sm border border-red-100 opacity-0 group-hover/seat:opacity-100 transition-opacity hover:bg-red-50 hover:scale-110"
                                >
                                  <X className="w-3 h-3" />
                                </button>
                              </div>
                            );
                          })}
                        </div>

                        {/* Row Label (Right - Optional balance) */}
                        <div className="w-8 text-left font-bold text-slate-400 text-sm">{row}</div>
                      </div>
                    ))}
                  </div>
                )}
              </div>

              {/* Screen Center Line Indicator (Subtle) */}
              <div className="absolute top-0 bottom-0 left-1/2 w-px border-l border-dashed border-slate-300 pointer-events-none opacity-30"></div>
            </CardContent>
          </Card>
        </div>
      </div>

      {/* --- EDIT MODAL --- */}
      {isEditingSeat && editingSeat && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm animate-in fade-in">
          <Card className="w-full max-w-sm shadow-2xl border-none">
            <CardHeader className="bg-slate-50 border-b pb-4">
              <div className="flex items-center justify-between">
                <div>
                  <CardTitle className="text-lg">Hiệu chỉnh ghế {editingSeat.seatCode}</CardTitle>
                  <CardDescription>Cập nhật trạng thái và loại ghế</CardDescription>
                </div>
                <Button variant="ghost" size="icon" className="h-8 w-8 rounded-full" onClick={() => setIsEditingSeat(false)}>
                  <X className="w-4 h-4" />
                </Button>
              </div>
            </CardHeader>
            <CardContent className="pt-6 space-y-4">
              <div className="space-y-2">
                <Label>Loại ghế</Label>
                <Select value={editForm.seatTypeCode} onValueChange={v => setEditForm({ ...editForm, seatTypeCode: v })}>
                  <SelectTrigger><SelectValue placeholder="Chọn loại" /></SelectTrigger>
                  <SelectContent>
                    <SelectItem value="null">Thường (Mặc định)</SelectItem>
                    {seatTypes.map(t => (
                      <SelectItem key={t.code} value={t.code}>
                        <div className="flex items-center gap-2">
                          <div className={cn("w-3 h-3 rounded-full", t.code.includes("VIP") ? "bg-amber-500" : t.code.includes("COUPLE") ? "bg-pink-500" : "bg-sky-500")}></div>
                          {t.name}
                        </div>
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              <div className="space-y-2">
                <Label>Trạng thái</Label>
                <Select value={editForm.isActive ? "active" : "inactive"} onValueChange={v => setEditForm({ ...editForm, isActive: v === "active" })}>
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="active"><span className="flex items-center gap-2"><span className="w-2 h-2 rounded-full bg-emerald-500"></span> Đang hoạt động</span></SelectItem>
                    <SelectItem value="inactive"><span className="flex items-center gap-2"><span className="w-2 h-2 rounded-full bg-gray-400"></span> Bảo trì / Ẩn</span></SelectItem>
                  </SelectContent>
                </Select>
              </div>

              {/* Preview */}
              <div className="mt-4 p-4 bg-slate-50 rounded-lg flex flex-col items-center gap-2 border border-dashed">
                <span className="text-xs text-muted-foreground uppercase font-bold">Xem trước</span>
                <div className={cn(
                  "flex items-center justify-center text-xs font-bold shadow-md transition-all",
                  "h-10 rounded-t-lg rounded-b-md border-b-4",
                  isCoupleSeat(editForm.seatTypeCode) ? "w-24" : "w-10",
                  getSeatStyle(editForm.seatTypeCode, editForm.isActive)
                )}>
                  {editingSeat.seatNumber}
                </div>
              </div>
            </CardContent>
            <CardFooter className="bg-slate-50 border-t pt-4 flex justify-between">
              <Button variant="ghost" className="text-red-500 hover:text-red-600 hover:bg-red-50" onClick={() => { setIsEditingSeat(false); handleDeleteSeat(editingSeat.id); }}>
                <Trash2 className="w-4 h-4 mr-2" /> Xóa ghế
              </Button>
              <Button onClick={handleUpdateSeat}>Lưu thay đổi</Button>
            </CardFooter>
          </Card>
        </div>
      )}
    </div>
  );
};

export default ScreenSeatsPage;