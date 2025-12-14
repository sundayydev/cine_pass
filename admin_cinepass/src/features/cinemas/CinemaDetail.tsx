import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { ArrowLeft, Plus, Pencil, Trash2, Users, MapPin, Phone, Mail, Building2, Globe, Image, MapPinned, Armchair } from "lucide-react";
import { toast } from "sonner";

// API Services
import { cinemaApi, type CinemaResponseDto } from "@/services/apiCinema";
import { screenApi, type ScreenResponseDto } from "@/services/apiScreen";
import { PATHS } from "@/config/paths";

// Shadcn UI
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Spinner } from "@/components/ui/spinner";

const CinemaDetailPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const [cinema, setCinema] = useState<CinemaResponseDto | null>(null);
  const [screens, setScreens] = useState<ScreenResponseDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isCreateScreenOpen, setIsCreateScreenOpen] = useState(false);
  const [isCreatingScreen, setIsCreatingScreen] = useState(false);

  // Form state for creating screen
  const [screenForm, setScreenForm] = useState({
    name: "",
    totalSeats: "",
    seatMapLayout: "",
  });
  
  // Seat map editor state
  const [seatMapRows, setSeatMapRows] = useState(10);
  const [seatMapCols, setSeatMapCols] = useState(15);
  const [seatMap, setSeatMap] = useState<boolean[][]>([]);
  const [showSeatMapEditor, setShowSeatMapEditor] = useState(false);

  useEffect(() => {
    if (id) {
      loadData();
    }
  }, [id]);

  const loadData = async () => {
    if (!id) return;

    try {
      setIsLoading(true);
      const [cinemaData, screensData] = await Promise.all([
        cinemaApi.getById(id),
        screenApi.getByCinemaId(id),
      ]);
      setCinema(cinemaData);
      setScreens(screensData);
    } catch (error) {
      console.error("Error loading data:", error);
      toast.error("Lỗi khi tải thông tin");
      navigate(PATHS.CINEMAS);
    } finally {
      setIsLoading(false);
    }
  };

  // Initialize seat map
  const initializeSeatMap = () => {
    const newMap: boolean[][] = Array(seatMapRows)
      .fill(null)
      .map(() => Array(seatMapCols).fill(false));
    setSeatMap(newMap);
  };

  // Toggle seat in map
  const toggleSeat = (row: number, col: number) => {
    const newMap = seatMap.map((r, rIdx) =>
      rIdx === row
        ? r.map((seat, cIdx) => (cIdx === col ? !seat : seat))
        : r
    );
    setSeatMap(newMap);
    updateSeatMapLayout(newMap);
  };

  // Update seat map layout JSON
  const updateSeatMapLayout = (map: boolean[][]) => {
    const layout: (string | null)[][] = map.map((row, rowIdx) =>
      row.map((hasSeat, colIdx) => {
        if (!hasSeat) return null;
        const rowLabel = String.fromCharCode(65 + rowIdx); // A, B, C, ...
        return `${rowLabel}${colIdx + 1}`;
      })
    );
    setScreenForm({ ...screenForm, seatMapLayout: JSON.stringify(layout) });
  };

  // Load seat map from JSON
  const loadSeatMapFromLayout = (layoutJson: string) => {
    try {
      if (!layoutJson) {
        initializeSeatMap();
        return;
      }
      const layout: (string | null)[][] = JSON.parse(layoutJson);
      const rows = layout.length;
      const cols = layout[0]?.length || 0;
      
      setSeatMapRows(rows);
      setSeatMapCols(cols);
      
      const newMap: boolean[][] = layout.map((row) =>
        row.map((seat) => seat !== null)
      );
      setSeatMap(newMap);
    } catch (error) {
      console.error("Error parsing seat map layout:", error);
      initializeSeatMap();
    }
  };

  const handleCreateScreen = async () => {
    if (!id) return;

    if (!screenForm.name || !screenForm.totalSeats) {
      toast.error("Vui lòng điền đầy đủ thông tin");
      return;
    }

    try {
      setIsCreatingScreen(true);
      await screenApi.create({
        cinemaId: id,
        name: screenForm.name,
        totalSeats: parseInt(screenForm.totalSeats),
        seatMapLayout: screenForm.seatMapLayout || undefined,
      });
      toast.success("Tạo phòng chiếu thành công");
      setIsCreateScreenOpen(false);
      setScreenForm({ name: "", totalSeats: "", seatMapLayout: "" });
      setShowSeatMapEditor(false);
      setSeatMap([]);
      loadData();
    } catch (error) {
      console.error("Error creating screen:", error);
      toast.error(error instanceof Error ? error.message : "Lỗi khi tạo phòng chiếu");
    } finally {
      setIsCreatingScreen(false);
    }
  };

  const handleDeleteScreen = async (screenId: string, screenName: string) => {
    if (!confirm(`Bạn có chắc chắn muốn xóa phòng "${screenName}"?`)) {
      return;
    }

    try {
      await screenApi.delete(screenId);
      toast.success("Xóa phòng chiếu thành công");
      loadData();
    } catch (error) {
      console.error("Error deleting screen:", error);
      toast.error(error instanceof Error ? error.message : "Lỗi khi xóa phòng chiếu");
    }
  };

  const formatDate = (dateString: string) => {
    if (!dateString) return "N/A";
    return new Date(dateString).toLocaleDateString("vi-VN");
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <Spinner />
      </div>
    );
  }

  if (!cinema) {
    return null;
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="icon" onClick={() => navigate(PATHS.CINEMAS)}>
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <div className="flex-1">
          <h1 className="text-3xl font-bold tracking-tight">{cinema.name}</h1>
          <p className="text-muted-foreground mt-1">Chi tiết rạp chiếu phim và quản lý phòng chiếu</p>
        </div>
        <Button onClick={() => navigate(`/cinemas/edit/${cinema.id}`)}>
          <Pencil className="mr-2 h-4 w-4" />
          Chỉnh Sửa
        </Button>
      </div>

      {/* Cinema Info */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <Card>
          <CardHeader>
            <CardTitle>Thông Tin Rạp Chiếu Phim</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            {cinema.description && (
              <div>
                <p className="text-sm font-medium mb-1">Mô Tả</p>
                <p className="text-sm text-muted-foreground">{cinema.description}</p>
              </div>
            )}
            {cinema.address && (
              <div className="flex items-start gap-3">
                <MapPin className="h-5 w-5 text-muted-foreground mt-0.5" />
                <div>
                  <p className="text-sm font-medium">Địa Chỉ</p>
                  <p className="text-sm text-muted-foreground">{cinema.address}</p>
                </div>
              </div>
            )}
            {cinema.city && (
              <div className="flex items-start gap-3">
                <Building2 className="h-5 w-5 text-muted-foreground mt-0.5" />
                <div>
                  <p className="text-sm font-medium">Thành Phố</p>
                  <p className="text-sm text-muted-foreground">{cinema.city}</p>
                </div>
              </div>
            )}
            {(cinema.latitude !== null && cinema.latitude !== undefined) || (cinema.longitude !== null && cinema.longitude !== undefined) ? (
              <div className="flex items-start gap-3">
                <MapPinned className="h-5 w-5 text-muted-foreground mt-0.5" />
                <div>
                  <p className="text-sm font-medium">Tọa Độ</p>
                  <p className="text-sm text-muted-foreground">
                    {cinema.latitude !== null && cinema.latitude !== undefined && cinema.longitude !== null && cinema.longitude !== undefined
                      ? `${cinema.latitude}, ${cinema.longitude}`
                      : cinema.latitude !== null && cinema.latitude !== undefined
                      ? `Lat: ${cinema.latitude}`
                      : `Lng: ${cinema.longitude}`}
                  </p>
                </div>
              </div>
            ) : null}
            {cinema.phone && (
              <div className="flex items-start gap-3">
                <Phone className="h-5 w-5 text-muted-foreground mt-0.5" />
                <div>
                  <p className="text-sm font-medium">Số Điện Thoại</p>
                  <p className="text-sm text-muted-foreground">{cinema.phone}</p>
                </div>
              </div>
            )}
            {cinema.email && (
              <div className="flex items-start gap-3">
                <Mail className="h-5 w-5 text-muted-foreground mt-0.5" />
                <div>
                  <p className="text-sm font-medium">Email</p>
                  <p className="text-sm text-muted-foreground">{cinema.email}</p>
                </div>
              </div>
            )}
            {cinema.website && (
              <div className="flex items-start gap-3">
                <Globe className="h-5 w-5 text-muted-foreground mt-0.5" />
                <div>
                  <p className="text-sm font-medium">Website</p>
                  <a 
                    href={cinema.website} 
                    target="_blank" 
                    rel="noopener noreferrer"
                    className="text-sm text-blue-600 hover:underline"
                  >
                    {cinema.website}
                  </a>
                </div>
              </div>
            )}
            {cinema.bannerUrl && (
              <div className="flex items-start gap-3">
                <Image className="h-5 w-5 text-muted-foreground mt-0.5" />
                <div>
                  <p className="text-sm font-medium">Banner</p>
                  <a 
                    href={cinema.bannerUrl} 
                    target="_blank" 
                    rel="noopener noreferrer"
                    className="text-sm text-blue-600 hover:underline"
                  >
                    Xem hình ảnh
                  </a>
                </div>
              </div>
            )}
            {cinema.facilities && cinema.facilities.length > 0 && (
              <div>
                <p className="text-sm font-medium mb-2">Tiện Ích</p>
                <div className="flex flex-wrap gap-2">
                  {cinema.facilities.map((facility, index) => (
                    <Badge key={index} variant="secondary">
                      {facility}
                    </Badge>
                  ))}
                </div>
              </div>
            )}
            <div className="flex items-center gap-2">
              <p className="text-sm font-medium">Trạng Thái:</p>
              {cinema.isActive ? (
                <Badge className="bg-emerald-500/15 text-emerald-700 hover:bg-emerald-500/25 border-emerald-500/30">
                  Đang hoạt động
                </Badge>
              ) : (
                <Badge variant="outline">Ngừng hoạt động</Badge>
              )}
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Thống Kê</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="flex items-center justify-between">
              <p className="text-sm text-muted-foreground">Slug</p>
              <p className="text-sm font-mono">{cinema.slug || "-"}</p>
            </div>
            <div className="flex items-center justify-between">
              <p className="text-sm text-muted-foreground">Tổng số phòng chiếu</p>
              <p className="text-2xl font-bold">{cinema.totalScreens ?? screens.length}</p>
            </div>
            <div className="flex items-center justify-between">
              <p className="text-sm text-muted-foreground">Số phòng đã tạo</p>
              <p className="text-2xl font-bold">{screens.length}</p>
            </div>
            <div className="flex items-center justify-between">
              <p className="text-sm text-muted-foreground">Tổng sức chứa</p>
              <p className="text-2xl font-bold">
                {screens.reduce((sum, screen) => sum + screen.totalSeats, 0)}
              </p>
            </div>
            {cinema.createdAt && (
              <div className="flex items-center justify-between">
                <p className="text-sm text-muted-foreground">Ngày tạo</p>
                <p className="text-sm">{formatDate(cinema.createdAt)}</p>
              </div>
            )}
            {cinema.updatedAt && (
              <div className="flex items-center justify-between">
                <p className="text-sm text-muted-foreground">Ngày cập nhật</p>
                <p className="text-sm">{formatDate(cinema.updatedAt)}</p>
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Screens Management */}
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle>Phòng Chiếu</CardTitle>
              <CardDescription>Quản lý các phòng chiếu trong rạp</CardDescription>
            </div>
            <Button onClick={() => setIsCreateScreenOpen(true)}>
              <Plus className="mr-2 h-4 w-4" />
              Thêm Phòng
            </Button>
          </div>
        </CardHeader>
        <CardContent>
          {screens.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-12">
              <Users className="h-12 w-12 text-muted-foreground mb-4" />
              <p className="text-muted-foreground">Chưa có phòng chiếu nào</p>
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Tên Phòng</TableHead>
                  <TableHead>Sức Chứa</TableHead>
                  <TableHead>Bản Đồ Ghế</TableHead>
                  <TableHead>Ngày Cập Nhật</TableHead>
                  <TableHead>Ngày Tạo</TableHead>
                  <TableHead className="text-center w-1/8">Thao Tác</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {screens.map((screen) => (
                  <TableRow key={screen.id}>
                    <TableCell className="font-medium">{screen.name}</TableCell>
                    <TableCell>{screen.totalSeats}</TableCell>
                    <TableCell>{screen.seatMapLayout || "-"}</TableCell>
                    <TableCell>{screen.updatedAt ? formatDate(screen.updatedAt) : "-"}</TableCell>
                    <TableCell>{screen.createdAt ? formatDate(screen.createdAt) : "-"}</TableCell>
                    <TableCell className="w-1/8">
                      <div className="flex items-center justify-center gap-2">
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => navigate(`/cinemas/${id}/screens/${screen.id}/seats`)}
                        >
                          <Armchair className="h-4 w-4 text-green-500 hover:text-green-900" />
                        </Button>
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => handleDeleteScreen(screen.id, screen.name)}
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

      {/* Create Screen Form */}
      {isCreateScreenOpen && (
        <Card>
          <CardHeader>
            <CardTitle>Thêm Phòng Chiếu Mới</CardTitle>
            <CardDescription>Điền thông tin để tạo phòng chiếu mới trong rạp này</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="name">Tên Phòng *</Label>
                <Input
                  id="name"
                  placeholder="Ví dụ: Phòng 1"
                  value={screenForm.name}
                  onChange={(e) => setScreenForm({ ...screenForm, name: e.target.value })}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="totalSeats">Sức Chứa *</Label>
                <Input
                  id="totalSeats"
                  type="number"
                  placeholder="Ví dụ: 100"
                  value={screenForm.totalSeats}
                  onChange={(e) => setScreenForm({ ...screenForm, totalSeats: e.target.value })}
                />
              </div>
              <div className="space-y-2">
                <div className="flex items-center justify-between">
                  <Label htmlFor="seatMapLayout">Bản Đồ Ghế</Label>
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    onClick={() => {
                      if (showSeatMapEditor) {
                        loadSeatMapFromLayout(screenForm.seatMapLayout);
                      } else {
                        if (screenForm.seatMapLayout) {
                          loadSeatMapFromLayout(screenForm.seatMapLayout);
                        } else {
                          initializeSeatMap();
                        }
                      }
                      setShowSeatMapEditor(!showSeatMapEditor);
                    }}
                  >
                    {showSeatMapEditor ? "Ẩn Bản Đồ" : "Hiện Bản Đồ"}
                  </Button>
                </div>
                
                {showSeatMapEditor ? (
                  <div className="space-y-4 border rounded-lg p-4">
                    {/* Controls */}
                    <div className="grid grid-cols-2 gap-4">
                      <div className="space-y-2">
                        <Label htmlFor="rows">Số Hàng</Label>
                        <Input
                          id="rows"
                          type="number"
                          min="1"
                          max="26"
                          value={seatMapRows}
                          onChange={(e) => {
                            const rows = parseInt(e.target.value) || 1;
                            setSeatMapRows(rows);
                            const newMap: boolean[][] = Array(rows)
                              .fill(null)
                              .map(() => Array(seatMapCols).fill(false));
                            setSeatMap(newMap);
                            updateSeatMapLayout(newMap);
                          }}
                        />
                      </div>
                      <div className="space-y-2">
                        <Label htmlFor="cols">Số Cột</Label>
                        <Input
                          id="cols"
                          type="number"
                          min="1"
                          max="50"
                          value={seatMapCols}
                          onChange={(e) => {
                            const cols = parseInt(e.target.value) || 1;
                            setSeatMapCols(cols);
                            const newMap: boolean[][] = Array(seatMapRows)
                              .fill(null)
                              .map(() => Array(cols).fill(false));
                            setSeatMap(newMap);
                            updateSeatMapLayout(newMap);
                          }}
                        />
                      </div>
                    </div>

                    {/* Seat Map Grid */}
                    <div className="space-y-2">
                      <Label>Bấm vào ô để thêm/xóa ghế</Label>
                      <div className="border rounded-lg p-4 bg-muted/30 overflow-auto max-h-[400px]">
                        {/* Screen indicator */}
                        <div className="text-center py-2 bg-muted rounded-lg mb-4">
                          <p className="text-sm font-medium">Màn hình</p>
                        </div>
                        
                        {/* Seat grid */}
                        <div className="space-y-1">
                          {/* Column headers */}
                          <div className="flex gap-1 mb-2">
                            <div className="w-8"></div>
                            {Array.from({ length: seatMapCols }, (_, i) => (
                              <div
                                key={i}
                                className="w-8 h-8 flex items-center justify-center text-xs font-semibold"
                              >
                                {i + 1}
                              </div>
                            ))}
                          </div>
                          
                          {/* Rows */}
                          {seatMap.map((row, rowIdx) => (
                            <div key={rowIdx} className="flex items-center gap-1">
                              <div className="w-8 h-8 flex items-center justify-center text-sm font-semibold">
                                {String.fromCharCode(65 + rowIdx)}
                              </div>
                              {row.map((hasSeat, colIdx) => (
                                <button
                                  key={colIdx}
                                  type="button"
                                  onClick={() => toggleSeat(rowIdx, colIdx)}
                                  className={`w-8 h-8 rounded border-2 transition-all ${
                                    hasSeat
                                      ? "bg-emerald-500 border-emerald-600 hover:bg-emerald-600"
                                      : "bg-gray-200 border-gray-300 hover:bg-gray-300"
                                  }`}
                                  title={`${String.fromCharCode(65 + rowIdx)}${colIdx + 1}`}
                                />
                              ))}
                            </div>
                          ))}
                        </div>
                      </div>
                      
                      {/* Legend */}
                      <div className="flex gap-4 text-sm">
                        <div className="flex items-center gap-2">
                          <div className="w-6 h-6 rounded border-2 bg-emerald-500 border-emerald-600"></div>
                          <span>Có ghế</span>
                        </div>
                        <div className="flex items-center gap-2">
                          <div className="w-6 h-6 rounded border-2 bg-gray-200 border-gray-300"></div>
                          <span>Không có ghế</span>
                        </div>
                      </div>
                    </div>
                  </div>
                ) : (
                  <Textarea
                    id="seatMapLayout"
                    placeholder="JSON layout hoặc bấm 'Hiện Bản Đồ' để vẽ trực quan..."
                    value={screenForm.seatMapLayout}
                    onChange={(e) => setScreenForm({ ...screenForm, seatMapLayout: e.target.value })}
                    className="font-mono text-xs"
                    rows={4}
                  />
                )}
              </div>
              <div className="flex items-center justify-end gap-4">
                <Button
                  variant="outline"
                  onClick={() => {
                    setIsCreateScreenOpen(false);
                    setScreenForm({ name: "", totalSeats: "", seatMapLayout: "" });
                    setShowSeatMapEditor(false);
                    setSeatMap([]);
                  }}
                  disabled={isCreatingScreen}
                >
                  Hủy
                </Button>
                <Button onClick={handleCreateScreen} disabled={isCreatingScreen}>
                  {isCreatingScreen && <Spinner className="mr-2 h-4 w-4" />}
                  Tạo Phòng
                </Button>
              </div>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
};

export default CinemaDetailPage;

