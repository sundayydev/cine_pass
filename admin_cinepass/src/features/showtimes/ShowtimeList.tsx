import { useState, useMemo } from 'react';
import { format } from 'date-fns';
import { vi } from 'date-fns/locale';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Plus, Search, Edit, Trash2, Eye, Calendar, Clock, Film, MapPin, RefreshCw, AlertCircle } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Badge } from '@/components/ui/badge';
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle, AlertDialogTrigger } from '@/components/ui/alert-dialog';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { toast } from 'sonner';
import { showtimeApi } from '@/services/apiShowtime';
import type { Showtime } from '@/services/apiShowtime';
import { movieApi } from '@/services/apiMovie';
import { cinemaApi } from '@/services/apiCinema';
import { useNavigate } from 'react-router-dom';

interface ShowtimeWithDetails extends Showtime {
  movieTitle?: string;
  screenName?: string;
  cinemaName?: string;
  movie?: any;
  screen?: any;
}

const ShowtimeList = () => {
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState<'all' | 'active' | 'inactive'>('all');
  const queryClient = useQueryClient();
  const navigate = useNavigate();

  // Fetch showtimes
  const {
    data: showtimes = [],
    isLoading: showtimesLoading,
    error: showtimesError,
    refetch: refetchShowtimes
  } = useQuery({
    queryKey: ['showtimes'],
    queryFn: showtimeApi.getAll,
    retry: 3,
    staleTime: 5 * 60 * 1000, // 5 minutes
  });

  // Fetch movies for enrichment
  const { data: movies = [], isLoading: moviesLoading } = useQuery({
    queryKey: ['movies'],
    queryFn: movieApi.getAll,
    retry: 3,
    staleTime: 10 * 60 * 1000, // 10 minutes
  });

  // Fetch cinemas for enrichment
  const { data: cinemas = [], isLoading: cinemasLoading } = useQuery({
    queryKey: ['cinemas'],
    queryFn: cinemaApi.getAll,
    retry: 3,
    staleTime: 10 * 60 * 1000, // 10 minutes
  });


  // Delete mutation
  const deleteMutation = useMutation({
    mutationFn: showtimeApi.delete,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['showtimes'] });
      toast.success('Xóa lịch chiếu thành công');
    },
    onError: (error: any) => {
      toast.error(`Lỗi khi xóa lịch chiếu: ${error.message}`);
    },
  });

  // Enrich showtimes with movie and screen details
  const enrichedShowtimes = useMemo((): ShowtimeWithDetails[] => {
    if (!showtimes.length) return [];

    return showtimes.map(showtime => {
      const movie = movies.find(m => m.id === showtime.movieId);
      // For now, we'll use placeholder data for screens since the API might not be working
      // In a real implementation, you'd want to fetch screen data properly
      const screen = showtime.screen || null;
      const cinema = screen ? cinemas.find(c => c.id === screen.cinemaId) : null;

      return {
        ...showtime,
        movieTitle: movie?.title || `Phim ${showtime.movieId}`,
        screenName: screen?.name || `Phòng ${showtime.screenId}`,
        cinemaName: cinema?.name || 'Chưa xác định',
        movie,
        screen,
      };
    });
  }, [showtimes, movies, cinemas]);

  // Filter showtimes
  const filteredShowtimes = useMemo(() => {
    return enrichedShowtimes.filter(showtime => {
      const matchesSearch = !searchTerm ||
        showtime.movieTitle?.toLowerCase().includes(searchTerm.toLowerCase()) ||
        showtime.screenName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
        showtime.cinemaName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
        showtime.id.toLowerCase().includes(searchTerm.toLowerCase());

      const matchesStatus = statusFilter === 'all' ||
        (statusFilter === 'active' && showtime.isActive) ||
        (statusFilter === 'inactive' && !showtime.isActive);

      return matchesSearch && matchesStatus;
    });
  }, [enrichedShowtimes, searchTerm, statusFilter]);

  const handleDelete = async (id: string) => {
    deleteMutation.mutate(id);
  };

  const handleRefresh = () => {
    refetchShowtimes();
    queryClient.invalidateQueries({ queryKey: ['movies'] });
    queryClient.invalidateQueries({ queryKey: ['cinemas'] });
  };

  const formatDateTime = (dateString: string) => {
    try {
      return format(new Date(dateString), 'dd/MM/yyyy HH:mm', { locale: vi });
    } catch (error) {
      console.warn('Error formatting date:', dateString, error);
      return 'Invalid Date';
    }
  };

  const formatPrice = (price: number) => {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND',
    }).format(price);
  };

  const isLoading = showtimesLoading || moviesLoading || cinemasLoading;
  const hasError = showtimesError;

  // Debug logging
  console.log('ShowtimeList Debug:', {
    showtimes: showtimes?.length || 0,
    movies: movies?.length || 0,
    cinemas: cinemas?.length || 0,
    enrichedShowtimes: enrichedShowtimes?.length || 0,
    filteredShowtimes: filteredShowtimes?.length || 0,
    isLoading,
    hasError,
  });

  if (hasError) {
    return (
      <div className="p-6">
        <Card className="border-red-200 bg-red-50">
          <CardContent className="pt-6">
            <div className="flex items-center gap-3">
              <AlertCircle className="h-5 w-5 text-red-600" />
              <div className="flex-1">
                <h3 className="font-medium text-red-900">Lỗi tải dữ liệu</h3>
                <p className="text-sm text-red-700 mt-1">
                  Không thể tải danh sách lịch chiếu. Vui lòng thử lại sau.
                </p>
              </div>
              <Button
                variant="outline"
                size="sm"
                onClick={handleRefresh}
                className="border-red-300 text-red-700 hover:bg-red-100"
              >
                <RefreshCw className="h-4 w-4 mr-2" />
                Thử lại
              </Button>
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Quản lý lịch chiếu</h1>
          <p className="text-muted-foreground">
            Quản lý tất cả lịch chiếu phim trong hệ thống
          </p>
        </div>
        <div className="flex items-center gap-2">
          <Button variant="outline" onClick={handleRefresh} disabled={isLoading}>
            <RefreshCw className={`h-4 w-4 mr-2 ${isLoading ? 'animate-spin' : ''}`} />
            Làm mới
          </Button>
          <Button className="gap-2" onClick={() => navigate('/showtimes/create')}>
            <Plus className="h-4 w-4" />
            Thêm lịch chiếu
          </Button>
        </div>
      </div>

      {/* Debug Info - Temporary for debugging */}
      {import.meta.env.DEV && (
        <Card className="bg-yellow-50 border-yellow-200">
          <CardHeader>
            <CardTitle className="text-sm text-yellow-800">Debug Info</CardTitle>
          </CardHeader>
          <CardContent className="text-xs text-yellow-700">
            <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
              <div>Showtimes: {showtimes?.length || 0}</div>
              <div>Movies: {movies?.length || 0}</div>
              <div>Cinemas: {cinemas?.length || 0}</div>
              <div>Filtered: {filteredShowtimes?.length || 0}</div>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Filters */}
      <Card>
        <CardHeader>
          <CardTitle className="text-lg">Bộ lọc & Tìm kiếm</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex flex-col sm:flex-row gap-4">
            <div className="flex-1">
              <div className="relative">
                <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-muted-foreground h-4 w-4" />
                <Input
                  placeholder="Tìm kiếm theo tên phim, phòng chiếu, rạp..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="pl-10"
                />
              </div>
            </div>
            <Select value={statusFilter} onValueChange={(value: 'all' | 'active' | 'inactive') => setStatusFilter(value)}>
              <SelectTrigger className="w-full sm:w-[180px]">
                <SelectValue placeholder="Trạng thái" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Tất cả trạng thái</SelectItem>
                <SelectItem value="active">Đang hoạt động</SelectItem>
                <SelectItem value="inactive">Không hoạt động</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </CardContent>
      </Card>

      {/* Table */}
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle>Danh sách lịch chiếu</CardTitle>
              <CardDescription>
                {isLoading ? 'Đang tải...' : `Hiển thị ${filteredShowtimes.length} / ${enrichedShowtimes.length} lịch chiếu`}
              </CardDescription>
            </div>
            {filteredShowtimes.length > 0 && (
              <Badge variant="outline">
                {filteredShowtimes.length} kết quả
              </Badge>
            )}
          </div>
        </CardHeader>
        <CardContent>
          {isLoading ? (
            <div className="flex flex-col items-center justify-center py-12">
              <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-primary mb-4"></div>
              <p className="text-muted-foreground">Đang tải danh sách lịch chiếu...</p>
            </div>
          ) : !enrichedShowtimes.length ? (
            <div className="text-center py-12">
              <Film className="mx-auto h-16 w-16 text-muted-foreground/50 mb-4" />
              <h3 className="text-lg font-semibold text-muted-foreground mb-2">Chưa có lịch chiếu nào</h3>
              <p className="text-sm text-muted-foreground mb-6 max-w-md mx-auto">
                Hệ thống chưa có lịch chiếu phim nào. Hãy tạo lịch chiếu đầu tiên để bắt đầu.
              </p>
              <Button onClick={() => navigate('/showtimes/create')} className="gap-2">
                <Plus className="h-4 w-4" />
                Tạo lịch chiếu đầu tiên
              </Button>
            </div>
          ) : filteredShowtimes.length === 0 ? (
            <div className="text-center py-12">
              <Search className="mx-auto h-16 w-16 text-muted-foreground/50 mb-4" />
              <h3 className="text-lg font-semibold text-muted-foreground mb-2">Không tìm thấy kết quả</h3>
              <p className="text-sm text-muted-foreground mb-6">
                Không có lịch chiếu nào phù hợp với tiêu chí tìm kiếm của bạn.
              </p>
              <div className="flex items-center justify-center gap-2">
                <Button
                  variant="outline"
                  onClick={() => {
                    setSearchTerm('');
                    setStatusFilter('all');
                  }}
                >
                  Xóa bộ lọc
                </Button>
                <Button onClick={() => navigate('/showtimes/create')} className="gap-2">
                  <Plus className="h-4 w-4" />
                  Thêm lịch chiếu
                </Button>
              </div>
            </div>
          ) : (
            <div className="space-y-4">
              {/* Summary Stats */}
              <div className="grid grid-cols-2 md:grid-cols-4 gap-4 p-4 bg-muted/50 rounded-lg">
                <div className="text-center">
                  <div className="text-2xl font-bold text-primary">{enrichedShowtimes.length}</div>
                  <div className="text-xs text-muted-foreground">Tổng lịch chiếu</div>
                </div>
                <div className="text-center">
                  <div className="text-2xl font-bold text-green-600">
                    {enrichedShowtimes.filter(s => s.isActive).length}
                  </div>
                  <div className="text-xs text-muted-foreground">Đang hoạt động</div>
                </div>
                <div className="text-center">
                  <div className="text-2xl font-bold text-orange-600">
                    {enrichedShowtimes.filter(s => !s.isActive).length}
                  </div>
                  <div className="text-xs text-muted-foreground">Không hoạt động</div>
                </div>
                <div className="text-center">
                  <div className="text-2xl font-bold text-blue-600">
                    {new Set(enrichedShowtimes.map(s => s.movieId)).size}
                  </div>
                  <div className="text-xs text-muted-foreground">Phim khác nhau</div>
                </div>
              </div>

              {/* Table */}
              <div className="border rounded-lg overflow-hidden">
                <Table>
                  <TableHeader>
                    <TableRow className="bg-muted/50">
                      <TableHead className="font-semibold">Phim</TableHead>
                      <TableHead className="font-semibold">Phòng chiếu</TableHead>
                      <TableHead className="font-semibold">Thời gian bắt đầu</TableHead>
                      <TableHead className="font-semibold">Thời gian kết thúc</TableHead>
                      <TableHead className="font-semibold">Giá vé</TableHead>
                      <TableHead className="font-semibold">Trạng thái</TableHead>
                      <TableHead className="text-right font-semibold">Thao tác</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {filteredShowtimes.map((showtime) => (
                      <TableRow key={showtime.id} className="hover:bg-muted/30 transition-colors">
                        <TableCell>
                          <div className="flex items-center gap-3">
                            <div className="flex items-center justify-center w-8 h-8 rounded-full bg-primary/10">
                              <Film className="h-4 w-4 text-primary" />
                            </div>
                            <div>
                              <div className="font-medium">{showtime.movieTitle}</div>
                              <div className="text-xs text-muted-foreground">ID: {showtime.id.slice(0, 8)}...</div>
                            </div>
                          </div>
                        </TableCell>
                        <TableCell>
                          <div className="flex items-center gap-3">
                            <div className="flex items-center justify-center w-8 h-8 rounded-full bg-blue-100">
                              <MapPin className="h-4 w-4 text-blue-600" />
                            </div>
                            <div>
                              <div className="font-medium">{showtime.screenName}</div>
                              <div className="text-xs text-muted-foreground">{showtime.cinemaName}</div>
                            </div>
                          </div>
                        </TableCell>
                        <TableCell>
                          <div className="flex items-center gap-2">
                            <Calendar className="h-4 w-4 text-muted-foreground" />
                            <div>
                              <div className="font-medium">{formatDateTime(showtime.startTime)}</div>
                            </div>
                          </div>
                        </TableCell>
                        <TableCell>
                          <div className="flex items-center gap-2">
                            <Clock className="h-4 w-4 text-muted-foreground" />
                            <div>
                              <div className="font-medium">{formatDateTime(showtime.endTime)}</div>
                            </div>
                          </div>
                        </TableCell>
                        <TableCell>
                          <div className="font-semibold text-green-600">
                            {formatPrice(showtime.basePrice)}
                          </div>
                        </TableCell>
                        <TableCell>
                          <Badge
                            variant={showtime.isActive ? 'default' : 'secondary'}
                            className={showtime.isActive ? 'bg-green-100 text-green-800 hover:bg-green-100' : ''}
                          >
                            {showtime.isActive ? 'Hoạt động' : 'Không hoạt động'}
                          </Badge>
                        </TableCell>
                        <TableCell className="text-right">
                          <div className="flex items-center justify-end gap-1">
                            <Button
                              variant="ghost"
                              size="sm"
                              className="h-8 w-8 p-0"
                              title="Xem chi tiết"
                            >
                              <Eye className="h-4 w-4" />
                            </Button>
                            <Button
                              variant="ghost"
                              size="sm"
                              className="h-8 w-8 p-0"
                              title="Chỉnh sửa"
                              onClick={() => navigate(`/showtimes/edit/${showtime.id}`)}
                            >
                              <Edit className="h-4 w-4" />
                            </Button>
                            <AlertDialog>
                              <AlertDialogTrigger asChild>
                                <Button
                                  variant="ghost"
                                  size="sm"
                                  className="h-8 w-8 p-0 text-red-600 hover:text-red-700 hover:bg-red-50"
                                  title="Xóa"
                                >
                                  <Trash2 className="h-4 w-4" />
                                </Button>
                              </AlertDialogTrigger>
                              <AlertDialogContent>
                                <AlertDialogHeader>
                                  <AlertDialogTitle>Xác nhận xóa lịch chiếu</AlertDialogTitle>
                                  <AlertDialogDescription>
                                    Bạn có chắc chắn muốn xóa lịch chiếu "{showtime.movieTitle}" không?
                                    Hành động này không thể hoàn tác.
                                  </AlertDialogDescription>
                                </AlertDialogHeader>
                                <AlertDialogFooter>
                                  <AlertDialogCancel>Hủy</AlertDialogCancel>
                                  <AlertDialogAction
                                    onClick={() => handleDelete(showtime.id)}
                                    className="bg-red-600 hover:bg-red-700"
                                  >
                                    Xóa lịch chiếu
                                  </AlertDialogAction>
                                </AlertDialogFooter>
                              </AlertDialogContent>
                            </AlertDialog>
                          </div>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </div>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
};

export default ShowtimeList;
