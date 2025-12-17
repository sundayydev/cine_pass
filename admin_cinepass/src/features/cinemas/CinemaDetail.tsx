import { useState, useEffect } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { format } from "date-fns";
import { vi } from "date-fns/locale";
import {
  ArrowLeft,
  MapPin,
  Phone,
  Mail,
  Globe,
  Building2,
  Pencil,
  Loader2,
  MonitorPlay,
  Plus,
  Settings,
  Calendar,
  Hash,
  Info,
  Layers,
  CheckCircle2,
  XCircle,
  Armchair,
  Wifi,
  Car,
  Glasses,
  Utensils,
  Accessibility,
  Snowflake,
  CreditCard,
  Sparkles,
  Trash2,
  X,
} from "lucide-react";

// API & Types
import { cinemaApi, type CinemaResponseDto } from "@/services/apiCinema";
import { screenApi, type ScreenResponseDto, type ScreenCreateDto, type ScreenUpdateDto } from "@/services/apiScreen";
import { PATHS } from "@/config/paths";

// Components
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Separator } from "@/components/ui/separator";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle } from "@/components/ui/alert-dialog";
import { toast } from "sonner";
import { cn } from "@/lib/utils";

const CinemaDetailPage = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const [cinema, setCinema] = useState<CinemaResponseDto | null>(null);
  const [screens, setScreens] = useState<ScreenResponseDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isLoadingScreens, setIsLoadingScreens] = useState(false);

  // Screen Modal States
  const [isScreenModalOpen, setIsScreenModalOpen] = useState(false);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [editingScreen, setEditingScreen] = useState<ScreenResponseDto | null>(null);
  const [screenToDelete, setScreenToDelete] = useState<ScreenResponseDto | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Screen Form State
  const [screenForm, setScreenForm] = useState({
    name: "",
    totalSeats: 0,
  });

  // --- 1. Fetch Data ---
  useEffect(() => {
    const loadCinema = async () => {
      if (!id) {
        navigate(PATHS.CINEMAS);
        return;
      }

      try {
        setIsLoading(true);
        const cinemaData = await cinemaApi.getById(id);
        setCinema(cinemaData);

        // Load screens
        setIsLoadingScreens(true);
        try {
          const screensData = await screenApi.getByCinemaId(id);
          setScreens(screensData);
        } catch (screenError) {
          console.error("Load Screens Error:", screenError);
        } finally {
          setIsLoadingScreens(false);
        }
      } catch (error) {
        console.error("Load Cinema Error:", error);
        toast.error("Không thể tải thông tin rạp chiếu phim.");
        navigate(PATHS.CINEMAS);
      } finally {
        setIsLoading(false);
      }
    };

    loadCinema();
  }, [id, navigate]);

  // --- 2. Screen Handlers ---
  const loadScreens = async () => {
    if (!id) return;
    setIsLoadingScreens(true);
    try {
      const screensData = await screenApi.getByCinemaId(id);
      setScreens(screensData);
    } catch (error) {
      console.error("Load Screens Error:", error);
      toast.error("Không thể tải danh sách phòng chiếu.");
    } finally {
      setIsLoadingScreens(false);
    }
  };

  const handleOpenScreenModal = (screen?: ScreenResponseDto) => {
    if (screen) {
      // Edit mode
      setEditingScreen(screen);
      setScreenForm({
        name: screen.name,
        totalSeats: screen.totalSeats,
      });
    } else {
      // Create mode
      setEditingScreen(null);
      setScreenForm({
        name: "",
        totalSeats: 0,
      });
    }
    setIsScreenModalOpen(true);
  };

  const handleOpenDeleteModal = (screen: ScreenResponseDto) => {
    setScreenToDelete(screen);
    setIsDeleteModalOpen(true);
  };

  const handleSubmitScreen = async () => {
    if (!id) return;
    if (!screenForm.name.trim() || screenForm.totalSeats <= 0) {
      toast.error("Vui lòng điền đầy đủ thông tin hợp lệ.");
      return;
    }

    setIsSubmitting(true);
    try {
      if (editingScreen) {
        // Update existing screen
        await screenApi.update(editingScreen.id, {
          name: screenForm.name,
          totalSeats: screenForm.totalSeats,
        });
        toast.success(`Cập nhật phòng chiếu "${screenForm.name}" thành công!`);
      } else {
        // Create new screen
        await screenApi.create({
          cinemaId: id,
          name: screenForm.name,
          totalSeats: screenForm.totalSeats,
        });
        toast.success(`Tạo phòng chiếu "${screenForm.name}" thành công!`);
      }

      setIsScreenModalOpen(false);
      await loadScreens();

      // Reload cinema to update totalScreens count
      const cinemaData = await cinemaApi.getById(id);
      setCinema(cinemaData);
    } catch (error: any) {
      console.error("Submit Screen Error:", error);
      toast.error(error.message || "Có lỗi xảy ra khi lưu phòng chiếu.");
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDeleteScreen = async () => {
    if (!screenToDelete) return;

    setIsSubmitting(true);
    try {
      await screenApi.delete(screenToDelete.id);
      toast.success(`Đã xóa phòng chiếu "${screenToDelete.name}"`);
      setIsDeleteModalOpen(false);
      await loadScreens();

      // Reload cinema to update totalScreens count
      if (id) {
        const cinemaData = await cinemaApi.getById(id);
        setCinema(cinemaData);
      }
    } catch (error: any) {
      console.error("Delete Screen Error:", error);
      toast.error(error.message || "Không thể xóa phòng chiếu.");
    } finally {
      setIsSubmitting(false);
    }
  };

  // --- 3. Helpers ---
  const formatDate = (dateString?: string) => {
    if (!dateString) return "N/A";
    try {
      return format(new Date(dateString), "dd/MM/yyyy HH:mm", { locale: vi });
    } catch {
      return dateString;
    }
  };

  if (isLoading) {
    return (
      <div className="h-screen flex flex-col items-center justify-center gap-4 bg-slate-50 dark:bg-slate-950">
        <Loader2 className="h-10 w-10 animate-spin text-primary" />
        <p className="text-muted-foreground animate-pulse">Đang tải dữ liệu rạp...</p>
      </div>
    );
  }

  const getFacilityIcon = (name: string) => {
    const n = name.toLowerCase();
    if (n.includes("wifi") || n.includes("internet")) return <Wifi className="h-4 w-4 text-blue-500" />;
    if (n.includes("xe") || n.includes("parking") || n.includes("đỗ")) return <Car className="h-4 w-4 text-orange-500" />;
    if (n.includes("ăn") || n.includes("food") || n.includes("corn") || n.includes("nước")) return <Utensils className="h-4 w-4 text-red-500" />;
    if (n.includes("3d") || n.includes("imax") || n.includes("4dx")) return <Glasses className="h-4 w-4 text-purple-500" />;
    if (n.includes("khuyết") || n.includes("wheelchair")) return <Accessibility className="h-4 w-4 text-green-500" />;
    if (n.includes("ghế") || n.includes("seat") || n.includes("vip")) return <Armchair className="h-4 w-4 text-amber-500" />;
    if (n.includes("lạnh") || n.includes("ac")) return <Snowflake className="h-4 w-4 text-cyan-500" />;
    if (n.includes("thẻ") || n.includes("card")) return <CreditCard className="h-4 w-4 text-indigo-500" />;

    // Icon mặc định
    return <CheckCircle2 className="h-4 w-4 text-primary" />;
  };

  if (!cinema) return null;

  return (
    <div className="min-h-screen bg-slate-50 dark:bg-[#0f172a] pb-20">
      {/* --- HERO SECTION --- */}
      <div className="relative h-[280px] w-full overflow-hidden group">
        {/* Background Image with Blur */}
        <div
          className="absolute inset-0 bg-cover bg-center blur-sm scale-110 opacity-50 dark:opacity-30 transition-transform duration-700 group-hover:scale-105"
          style={{ backgroundImage: `url(${cinema.bannerUrl || "https://placehold.co/1200x400/e2e8f0/64748b?text=Cinema"})` }}
        />
        <div className="absolute inset-0 bg-gradient-to-t from-slate-50 dark:from-[#0f172a] via-transparent to-transparent" />
        <div className="absolute inset-0 bg-gradient-to-b from-black/40 to-transparent" />

        {/* Header Actions */}
        <div className="absolute top-6 left-6 right-6 flex justify-between items-start z-10">
          <Button
            variant="secondary"
            size="sm"
            className="backdrop-blur-md bg-white/20 hover:bg-white/40 text-white border-none shadow-sm"
            onClick={() => navigate(PATHS.CINEMAS)}
          >
            <ArrowLeft className="h-4 w-4 mr-2" />
            Quay lại danh sách
          </Button>

          <Button
            className="shadow-lg bg-primary hover:bg-primary/90"
            onClick={() => navigate(PATHS.CINEMA_EDIT.replace(":id", cinema.id))}
          >
            <Pencil className="h-4 w-4 mr-2" />
            Chỉnh sửa thông tin
          </Button>
        </div>
      </div>

      {/* --- MAIN CONTENT CONTAINER --- */}
      <div className="container max-w-6xl mx-auto px-4 -mt-32 relative z-20">

        {/* Cinema Profile Header */}
        <div className="flex flex-col md:flex-row items-start gap-6 mb-8">
          {/* Logo / Thumbnail */}
          <div className="relative h-40 w-40 md:h-48 md:w-48 rounded-2xl overflow-hidden border-4 border-white dark:border-[#1e293b] shadow-2xl bg-white flex-shrink-0">
            <img
              src={cinema.bannerUrl || "/placeholder.svg"}
              alt={cinema.name}
              className="h-full w-full object-cover"
              onError={(e) => (e.target as HTMLImageElement).src = "https://placehold.co/400x400/e2e8f0/64748b?text=Logo"}
            />
          </div>

          {/* Title & Badges */}
          <div className="flex-1 pt-2 md:pt-12 text-slate-800 dark:text-slate-100">
            <div className="flex flex-wrap items-center gap-3 mb-2">
              {cinema.isActive ? (
                <Badge className="bg-emerald-500/15 text-emerald-600 dark:text-emerald-400 hover:bg-emerald-500/25 border-emerald-500/20 pl-1 pr-3 py-1">
                  <span className="relative flex h-2 w-2 mr-2">
                    <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-emerald-400 opacity-75"></span>
                    <span className="relative inline-flex rounded-full h-2 w-2 bg-emerald-500"></span>
                  </span>
                  Đang hoạt động
                </Badge>
              ) : (
                <Badge variant="destructive" className="pl-1 pr-3 py-1">
                  <XCircle className="h-3 w-3 mr-2" /> Ngừng hoạt động
                </Badge>
              )}
              {cinema.city && (
                <Badge variant="outline" className="bg-background/50 backdrop-blur-sm">
                  <MapPin className="h-3 w-3 mr-1" /> {cinema.city}
                </Badge>
              )}
            </div>
            <h1 className="text-3xl md:text-4xl font-bold tracking-tight mb-2 shadow-black drop-shadow-sm">{cinema.name}</h1>
            <p className="flex items-center text-muted-foreground font-medium">
              <Building2 className="h-4 w-4 mr-1.5" />
              {cinema.totalScreens} phòng chiếu
            </p>
          </div>
        </div>

        {/* --- TABS SECTION --- */}
        <Tabs defaultValue="overview" className="space-y-6">
          <TabsList className="grid w-full md:w-[500px] grid-cols-3 bg-white dark:bg-[#1e293b] p-1 shadow-sm border border-slate-200 dark:border-slate-800 h-12 rounded-xl">
            <TabsTrigger value="overview" className="rounded-lg data-[state=active]:bg-slate-100 dark:data-[state=active]:bg-slate-800">Tổng quan</TabsTrigger>
            <TabsTrigger value="screens" className="rounded-lg data-[state=active]:bg-slate-100 dark:data-[state=active]:bg-slate-800">
              Phòng chiếu ({screens.length})
            </TabsTrigger>
            <TabsTrigger value="technical" className="rounded-lg data-[state=active]:bg-slate-100 dark:data-[state=active]:bg-slate-800">Kỹ thuật</TabsTrigger>
          </TabsList>

          {/* TAB 1: OVERVIEW */}
          <TabsContent value="overview" className="space-y-6 animate-in fade-in-50 slide-in-from-bottom-2 duration-300">
            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">

              {/* Left Column: Contact Info */}
              <div className="space-y-6 lg:col-span-2">
                <Card>
                  <CardHeader>
                    <CardTitle className="flex items-center gap-2">
                      <Info className="h-5 w-5 text-primary" /> Thông tin liên hệ
                    </CardTitle>
                  </CardHeader>
                  <CardContent className="grid gap-6">
                    <div className="grid sm:grid-cols-2 gap-4">
                      <div className="flex items-start gap-3 p-3 rounded-lg bg-slate-50 dark:bg-slate-800/50 border border-slate-100 dark:border-slate-800">
                        <div className="p-2 bg-blue-100 dark:bg-blue-900/30 text-blue-600 rounded-md">
                          <MapPin className="h-5 w-5" />
                        </div>
                        <div>
                          <p className="text-xs text-muted-foreground font-medium uppercase">Địa chỉ</p>
                          <p className="text-sm font-medium mt-0.5 truncate w-[240px]">{cinema.address || "Chưa cập nhật"}</p>
                        </div>
                      </div>

                      <div className="flex items-start gap-3 p-3 rounded-lg bg-slate-50 dark:bg-slate-800/50 border border-slate-100 dark:border-slate-800">
                        <div className="p-2 bg-green-100 dark:bg-green-900/30 text-green-600 rounded-md">
                          <Phone className="h-5 w-5" />
                        </div>
                        <div>
                          <p className="text-xs text-muted-foreground font-medium uppercase">Hotline</p>
                          <p className="text-sm font-medium mt-0.5">{cinema.phone || "Chưa cập nhật"}</p>
                        </div>
                      </div>

                      <div className="flex items-start gap-3 p-3 rounded-lg bg-slate-50 dark:bg-slate-800/50 border border-slate-100 dark:border-slate-800">
                        <div className="p-2 bg-orange-100 dark:bg-orange-900/30 text-orange-600 rounded-md">
                          <Mail className="h-5 w-5" />
                        </div>
                        <div>
                          <p className="text-xs text-muted-foreground font-medium uppercase">Email</p>
                          <p className="text-sm font-medium mt-0.5">{cinema.email || "Chưa cập nhật"}</p>
                        </div>
                      </div>

                      <div className="flex items-start gap-3 p-3 rounded-lg bg-slate-50 dark:bg-slate-800/50 border border-slate-100 dark:border-slate-800">
                        <div className="p-2 bg-purple-100 dark:bg-purple-900/30 text-purple-600 rounded-md">
                          <Globe className="h-5 w-5" />
                        </div>
                        <div>
                          <p className="text-xs text-muted-foreground font-medium uppercase">Website</p>
                          {cinema.website ? (
                            <a href={cinema.website} target="_blank" className="text-sm font-medium mt-0.5 text-primary hover:underline truncate w-[200px] block">
                              {cinema.website}
                            </a>
                          ) : (
                            <p className="text-sm font-medium mt-0.5">Chưa cập nhật</p>
                          )}
                        </div>
                      </div>
                    </div>
                  </CardContent>
                </Card>

                <Card>
                  <CardHeader>
                    <CardTitle className="flex items-center gap-2">
                      <Layers className="h-5 w-5 text-primary" /> Giới thiệu
                    </CardTitle>
                  </CardHeader>
                  <CardContent>
                    <p className="text-slate-600 dark:text-slate-300 leading-relaxed whitespace-pre-line">
                      {cinema.description || "Chưa có mô tả giới thiệu cho rạp chiếu phim này."}
                    </p>
                  </CardContent>
                </Card>
              </div>

              {/* Right Column: Facilities & Map */}
              <div className="space-y-6">
                <Card className="h-full border-none shadow-md bg-white dark:bg-[#1e293b]">
                  <CardHeader className="pb-3 border-b border-slate-100 dark:border-slate-800">
                    <CardTitle className="flex items-center gap-2 text-base font-bold uppercase tracking-wide text-slate-500 dark:text-slate-400">
                      <Sparkles className="h-4 w-4 text-primary" /> Tiện ích nổi bật
                    </CardTitle>
                  </CardHeader>
                  <CardContent className="pt-4">
                    {cinema.facilities && cinema.facilities.length > 0 ? (
                      <div className="grid grid-cols-2 gap-3">
                        {cinema.facilities.map((facility, idx) => (
                          <div
                            key={idx}
                            className="group flex items-center gap-3 p-3 rounded-lg border border-slate-100 dark:border-slate-700 bg-slate-50/50 dark:bg-slate-800/50 hover:border-primary/30 hover:bg-primary/5 transition-all duration-300"
                          >
                            <div className="flex-shrink-0 p-2 bg-white dark:bg-slate-900 rounded-md shadow-sm group-hover:scale-110 transition-transform">
                              {getFacilityIcon(facility)}
                            </div>
                            <span className="text-sm font-medium text-slate-700 dark:text-slate-200 line-clamp-2">
                              {facility}
                            </span>
                          </div>
                        ))}
                      </div>
                    ) : (
                      <div className="flex flex-col items-center justify-center py-10 text-center text-muted-foreground bg-slate-50 dark:bg-slate-800/30 rounded-xl border-2 border-dashed border-slate-200 dark:border-slate-700">
                        <Sparkles className="h-8 w-8 mb-2 text-slate-300" />
                        <span className="text-sm">Chưa cập nhật tiện ích</span>
                      </div>
                    )}
                  </CardContent>
                </Card>
              </div>
            </div>
          </TabsContent>

          {/* TAB 2: SCREENS */}
          <TabsContent value="screens" className="animate-in fade-in-50 slide-in-from-bottom-2 duration-300">
            <Card>
              <CardHeader className="flex flex-row items-center justify-between">
                <div>
                  <CardTitle>Danh sách phòng chiếu</CardTitle>
                  <CardDescription>Quản lý các phòng chiếu thuộc rạp {cinema.name}</CardDescription>
                </div>
                <Button onClick={() => handleOpenScreenModal()} size="sm" className="bg-primary shadow-md">
                  <Plus className="h-4 w-4 mr-2" /> Thêm phòng mới
                </Button>
              </CardHeader>
              <Separator />
              <CardContent className="p-6">
                {isLoadingScreens ? (
                  <div className="flex justify-center py-12"><Loader2 className="h-8 w-8 animate-spin text-primary" /></div>
                ) : screens.length > 0 ? (
                  <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
                    {screens.map((screen) => (
                      <Card key={screen.id} className="group overflow-hidden border-slate-200 dark:border-slate-800 hover:border-primary/50 hover:shadow-md transition-all duration-300">
                        <div className="h-2 bg-gradient-to-r from-indigo-500 to-purple-500 opacity-70 group-hover:opacity-100 transition-opacity" />
                        <CardContent className="p-5">
                          <div className="flex justify-between items-start mb-4">
                            <div className="p-2.5 bg-slate-100 dark:bg-slate-800 rounded-lg group-hover:bg-primary/10 group-hover:text-primary transition-colors">
                              <MonitorPlay className="h-6 w-6" />
                            </div>
                          </div>

                          <h3 className="font-bold text-lg mb-1">{screen.name}</h3>
                          <div className="flex items-center text-muted-foreground text-sm mb-6">
                            <Armchair className="h-4 w-4 mr-1.5" />
                            {screen.totalSeats} ghế ngồi
                          </div>

                          <div className="grid grid-cols-2 gap-2">
                            <Button
                              variant="outline"
                              className="w-full text-xs h-9 hover:bg-primary hover:text-white hover:border-primary transition-colors"
                              onClick={() => navigate(PATHS.ROOM_SEAT_MAP.replace(":cinemaId", cinema.id).replace(":roomId", screen.id))}
                            >
                              <Settings className="h-3 w-3 mr-1.5" /> Sơ đồ ghế
                            </Button>
                            <Button variant="ghost" className="w-full text-xs h-9" onClick={() => handleOpenScreenModal(screen)}>
                              <Pencil className="h-3 w-3 mr-1.5" /> Chỉnh sửa
                            </Button>
                          </div>
                          <Button
                            variant="ghost"
                            size="sm"
                            className="absolute top-2 right-2 h-8 w-8 p-0 opacity-0 group-hover:opacity-100 transition-opacity text-destructive hover:text-destructive hover:bg-destructive/10"
                            onClick={() => handleOpenDeleteModal(screen)}
                          >
                            <Trash2 className="h-4 w-4" />
                          </Button>
                        </CardContent>
                      </Card>
                    ))}
                  </div>
                ) : (
                  <div className="flex flex-col items-center justify-center py-16 text-center">
                    <div className="h-20 w-20 bg-slate-100 dark:bg-slate-800 rounded-full flex items-center justify-center mb-4">
                      <MonitorPlay className="h-10 w-10 text-slate-400" />
                    </div>
                    <h3 className="text-lg font-semibold text-slate-900 dark:text-slate-100">Chưa có phòng chiếu nào</h3>
                    <p className="text-slate-500 max-w-sm mt-1 mb-6">Hãy tạo phòng chiếu đầu tiên để bắt đầu thiết lập lịch chiếu cho rạp này.</p>
                    <Button onClick={() => handleOpenScreenModal()}>Tạo phòng chiếu ngay</Button>
                  </div>
                )}
              </CardContent>
            </Card>

            {/* Create/Edit Screen Modal */}
            <Dialog open={isScreenModalOpen} onOpenChange={setIsScreenModalOpen}>
              <DialogContent className="sm:max-w-[500px]">
                <DialogHeader>
                  <DialogTitle>{editingScreen ? "Chỉnh sửa phòng chiếu" : "Tạo phòng chiếu mới"}</DialogTitle>
                  <DialogDescription>
                    {editingScreen
                      ? `Cập nhật thông tin phòng chiếu "${editingScreen.name}"`
                      : `Thêm phòng chiếu mới cho rạp ${cinema.name}`
                    }
                  </DialogDescription>
                </DialogHeader>
                <div className="grid gap-4 py-4">
                  <div className="grid gap-2">
                    <Label htmlFor="screen-name">Tên phòng chiếu *</Label>
                    <Input
                      id="screen-name"
                      placeholder="VD: Phòng 1, Screen A, IMAX Hall..."
                      value={screenForm.name}
                      onChange={(e) => setScreenForm({ ...screenForm, name: e.target.value })}
                    />
                  </div>
                  <div className="grid gap-2">
                    <Label htmlFor="screen-seats">Tổng số ghế *</Label>
                    <Input
                      id="screen-seats"
                      type="number"
                      min="1"
                      placeholder="VD: 100"
                      value={screenForm.totalSeats || ""}
                      onChange={(e) => setScreenForm({ ...screenForm, totalSeats: parseInt(e.target.value) || 0 })}
                    />
                  </div>
                  <div className="bg-blue-50 dark:bg-blue-950/30 border border-blue-200 dark:border-blue-800 rounded-lg p-3">
                    <p className="text-xs text-blue-700 dark:text-blue-300 flex items-start gap-2">
                      <Info className="h-4 w-4 shrink-0 mt-0.5" />
                      <span>Sơ đồ ghế có thể được thiết lập sau khi tạo phòng chiếu thông qua chức năng "Sơ đồ ghế".</span>
                    </p>
                  </div>
                </div>
                <DialogFooter>
                  <Button variant="outline" onClick={() => setIsScreenModalOpen(false)} disabled={isSubmitting}>
                    Hủy
                  </Button>
                  <Button onClick={handleSubmitScreen} disabled={isSubmitting || !screenForm.name || screenForm.totalSeats <= 0}>
                    {isSubmitting ? (
                      <>
                        <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                        Đang xử lý...
                      </>
                    ) : editingScreen ? (
                      "Cập nhật"
                    ) : (
                      "Tạo phòng chiếu"
                    )}
                  </Button>
                </DialogFooter>
              </DialogContent>
            </Dialog>

            {/* Delete Screen Confirmation */}
            <AlertDialog open={isDeleteModalOpen} onOpenChange={setIsDeleteModalOpen}>
              <AlertDialogContent>
                <AlertDialogHeader>
                  <AlertDialogTitle>Bạn có chắc chắn muốn xóa?</AlertDialogTitle>
                  <AlertDialogDescription>
                    Phòng chiếu <strong className="text-foreground">"{screenToDelete?.name}"</strong> sẽ bị xóa vĩnh viễn.
                    Hành động này không thể hoàn tác. Tất cả lịch chiếu và dữ liệu liên quan cũng sẽ bị ảnh hưởng.
                  </AlertDialogDescription>
                </AlertDialogHeader>
                <AlertDialogFooter>
                  <AlertDialogCancel disabled={isSubmitting}>Hủy</AlertDialogCancel>
                  <AlertDialogAction
                    onClick={handleDeleteScreen}
                    disabled={isSubmitting}
                    className="bg-destructive hover:bg-destructive/90"
                  >
                    {isSubmitting ? (
                      <>
                        <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                        Đang xóa...
                      </>
                    ) : (
                      "Xóa phòng chiếu"
                    )}
                  </AlertDialogAction>
                </AlertDialogFooter>
              </AlertDialogContent>
            </AlertDialog>
          </TabsContent>

          {/* TAB 3: TECHNICAL */}
          <TabsContent value="technical" className="animate-in fade-in-50 slide-in-from-bottom-2 duration-300">
            <Card>
              <CardHeader>
                <CardTitle>Thông tin kỹ thuật & Metadata</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                  <div className="space-y-1">
                    <label className="text-xs font-medium text-muted-foreground uppercase flex items-center gap-1">
                      <Hash className="h-3 w-3" /> Cinema ID
                    </label>
                    <div className="font-mono text-sm bg-slate-100 dark:bg-slate-900 p-3 rounded-md border select-all">
                      {cinema.id}
                    </div>
                  </div>
                  <div className="space-y-1">
                    <label className="text-xs font-medium text-muted-foreground uppercase flex items-center gap-1">
                      <Globe className="h-3 w-3" /> SEO Slug
                    </label>
                    <div className="font-mono text-sm bg-slate-100 dark:bg-slate-900 p-3 rounded-md border select-all">
                      {cinema.slug}
                    </div>
                  </div>
                  <div className="space-y-1">
                    <label className="text-xs font-medium text-muted-foreground uppercase flex items-center gap-1">
                      <Calendar className="h-3 w-3" /> Ngày khởi tạo
                    </label>
                    <p className="text-sm font-medium border-b py-2">{formatDate(cinema.createdAt)}</p>
                  </div>
                  <div className="space-y-1">
                    <label className="text-xs font-medium text-muted-foreground uppercase flex items-center gap-1">
                      <Calendar className="h-3 w-3" /> Cập nhật lần cuối
                    </label>
                    <p className="text-sm font-medium border-b py-2">{formatDate(cinema.updatedAt)}</p>
                  </div>
                  <div className="md:col-span-2">
                    <Separator className="my-2" />
                    <h4 className="text-sm font-semibold mb-2">Tọa độ địa lý (GPS)</h4>
                    <div className="grid grid-cols-2 gap-4">
                      <div className="bg-slate-50 dark:bg-slate-800 p-3 rounded border">
                        <span className="text-xs text-muted-foreground block">Latitude</span>
                        <span className="font-mono">{cinema.latitude || "N/A"}</span>
                      </div>
                      <div className="bg-slate-50 dark:bg-slate-800 p-3 rounded border">
                        <span className="text-xs text-muted-foreground block">Longitude</span>
                        <span className="font-mono">{cinema.longitude || "N/A"}</span>
                      </div>
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>
          </TabsContent>
        </Tabs>
      </div >
    </div >
  );
};

export default CinemaDetailPage;