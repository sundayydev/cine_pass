import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { format } from 'date-fns';
import { vi } from 'date-fns/locale';
import { CalendarIcon, Clock, Film, MapPin, DollarSign, Save, X, ArrowLeft } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Form, FormControl, FormDescription, FormField, FormItem, FormLabel, FormMessage } from '@/components/ui/form';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Popover, PopoverContent, PopoverTrigger } from '@/components/ui/popover';
import { Calendar } from '@/components/ui/calendar';
import { Switch } from '@/components/ui/switch';
import { toast } from 'sonner';
import { cn } from '@/lib/utils';
import { showtimeApi, ShowtimeUpdatePayload, Showtime } from '@/services/apiShowtime';
import { movieApi } from '@/services/apiMovie';
import { screenApi } from '@/services/apiScreen';
import { cinemaApi } from '@/services/apiCinema';

// Validation schema
const editShowtimeSchema = z.object({
  movieId: z.string().min(1, 'Vui lòng chọn phim'),
  cinemaId: z.string().min(1, 'Vui lòng chọn rạp chiếu'),
  screenId: z.string().min(1, 'Vui lòng chọn phòng chiếu'),
  startDate: z.date({
    required_error: 'Vui lòng chọn ngày chiếu',
  }),
  startTime: z.string().min(1, 'Vui lòng chọn giờ chiếu'),
  durationMinutes: z.number().min(1, 'Thời lượng phải lớn hơn 0'),
  basePrice: z.number().min(0, 'Giá cơ bản phải >= 0'),
  isActive: z.boolean(),
}).refine((data) => {
  // Check if selected screen belongs to selected cinema
  return true; // This will be validated on the backend
}, {
  message: "Phòng chiếu không thuộc rạp đã chọn",
  path: ["screenId"],
});

type EditShowtimeForm = z.infer<typeof editShowtimeSchema>;

const EditShowtime = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [selectedCinemaId, setSelectedCinemaId] = useState<string>('');
  const queryClient = useQueryClient();

  const form = useForm<EditShowtimeForm>({
    resolver: zodResolver(editShowtimeSchema),
    defaultValues: {
      movieId: '',
      cinemaId: '',
      screenId: '',
      startDate: undefined,
      startTime: '',
      durationMinutes: 0,
      basePrice: 0,
      isActive: true,
    },
  });

  // Fetch showtime data
  const { data: showtime, isLoading: showtimeLoading, error: showtimeError } = useQuery({
    queryKey: ['showtime', id],
    queryFn: () => showtimeApi.getById(id!),
    enabled: !!id,
  });

  // Fetch movies
  const { data: movies = [], isLoading: moviesLoading } = useQuery({
    queryKey: ['movies'],
    queryFn: movieApi.getAll,
  });

  // Fetch cinemas
  const { data: cinemas = [], isLoading: cinemasLoading } = useQuery({
    queryKey: ['cinemas'],
    queryFn: cinemaApi.getAll,
  });

  // Fetch screens for selected cinema
  const { data: screens = [], isLoading: screensLoading } = useQuery({
    queryKey: ['screens', selectedCinemaId],
    queryFn: () => selectedCinemaId ? screenApi.getByCinemaId(selectedCinemaId) : Promise.resolve([]),
    enabled: !!selectedCinemaId,
  });

  // Update mutation
  const updateMutation = useMutation({
    mutationFn: (payload: { id: string; data: ShowtimeUpdatePayload }) =>
      showtimeApi.update(payload.id, payload.data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['showtimes'] });
      queryClient.invalidateQueries({ queryKey: ['showtime', id] });
      toast.success('Cập nhật lịch chiếu thành công');
    },
    onError: (error: any) => {
      toast.error(`Lỗi khi cập nhật lịch chiếu: ${error.message}`);
    },
  });

  // Populate form when showtime data is loaded
  useEffect(() => {
    if (showtime && movies.length > 0 && cinemas.length > 0) {
      const startDateTime = new Date(showtime.startTime);
      const selectedMovie = movies.find(movie => movie.id === showtime.movieId);
      const selectedScreen = screens.find(screen => screen.id === showtime.screenId);
      const selectedCinema = cinemas.find((cinema: any) => cinema.id === selectedScreen?.cinemaId);

      form.reset({
        movieId: showtime.movieId,
        cinemaId: selectedCinema?.id || '',
        screenId: showtime.screenId,
        startDate: startDateTime,
        startTime: format(startDateTime, 'HH:mm'),
        durationMinutes: selectedMovie?.durationMinutes || 0,
        basePrice: showtime.basePrice,
        isActive: showtime.isActive,
      });

      setSelectedCinemaId(selectedCinema?.id || '');
    }
  }, [showtime, movies, cinemas, screens, form]);

  const onSubmit = (data: EditShowtimeForm) => {
    if (!id) return;

    // Combine date and time
    const startDateTime = new Date(data.startDate);
    const [hours, minutes] = data.startTime.split(':').map(Number);
    startDateTime.setHours(hours, minutes, 0, 0);

    // Calculate end time based on duration
    const selectedMovie = movies.find(movie => movie.id === data.movieId);
    const endDateTime = new Date(startDateTime);
    endDateTime.setMinutes(endDateTime.getMinutes() + (selectedMovie?.durationMinutes || data.durationMinutes));

    const payload: ShowtimeUpdatePayload = {
      movieId: data.movieId,
      screenId: data.screenId,
      startTime: startDateTime.toISOString(),
      basePrice: data.basePrice,
      isActive: data.isActive,
    };

    updateMutation.mutate({ id, data: payload });
  };

  const handleCinemaChange = (cinemaId: string) => {
    setSelectedCinemaId(cinemaId);
    form.setValue('cinemaId', cinemaId);
    form.setValue('screenId', ''); // Reset screen selection
  };

  const selectedMovie = movies.find(movie => movie.id === form.watch('movieId'));

  if (showtimeError) {
    return (
      <div className="p-6">
        <Card>
          <CardHeader>
            <CardTitle className="text-red-600">Lỗi tải dữ liệu</CardTitle>
            <CardDescription>
              Không thể tải thông tin lịch chiếu. Vui lòng thử lại sau.
            </CardDescription>
          </CardHeader>
          <CardContent>
            <Button onClick={() => navigate('/showtimes')} variant="outline">
              <ArrowLeft className="h-4 w-4 mr-2" />
              Quay lại danh sách
            </Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  if (showtimeLoading || moviesLoading || cinemasLoading) {
    return (
      <div className="p-6">
        <div className="flex items-center justify-center py-8">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
          <span className="ml-2">Đang tải...</span>
        </div>
      </div>
    );
  }

  return (
    <div className="p-6 max-w-4xl mx-auto">
      <div className="mb-6">
        <div className="flex items-center gap-4 mb-4">
          <Button
            variant="outline"
            size="sm"
            onClick={() => navigate('/showtimes')}
          >
            <ArrowLeft className="h-4 w-4 mr-2" />
            Quay lại
          </Button>
        </div>
        <h1 className="text-3xl font-bold tracking-tight">Chỉnh sửa lịch chiếu</h1>
        <p className="text-muted-foreground">
          Cập nhật thông tin lịch chiếu
        </p>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Film className="h-5 w-5" />
            Thông tin lịch chiếu
          </CardTitle>
          <CardDescription>
            Chỉnh sửa thông tin lịch chiếu
          </CardDescription>
        </CardHeader>
        <CardContent>
          <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
              {/* Active Status */}
              <FormField
                control={form.control}
                name="isActive"
                render={({ field }) => (
                  <FormItem className="flex flex-row items-center justify-between rounded-lg border p-4">
                    <div className="space-y-0.5">
                      <FormLabel className="text-base">Trạng thái hoạt động</FormLabel>
                      <FormDescription>
                        Bật/tắt trạng thái hoạt động của lịch chiếu này
                      </FormDescription>
                    </div>
                    <FormControl>
                      <Switch
                        checked={field.value}
                        onCheckedChange={field.onChange}
                      />
                    </FormControl>
                  </FormItem>
                )}
              />

              {/* Movie Selection */}
              <FormField
                control={form.control}
                name="movieId"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Phim</FormLabel>
                    <Select onValueChange={field.onChange} value={field.value}>
                      <FormControl>
                        <SelectTrigger>
                          <SelectValue placeholder="Chọn phim" />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        {movies.map((movie) => (
                          <SelectItem key={movie.id} value={movie.id}>
                            {movie.title}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                    <FormMessage />
                  </FormItem>
                )}
              />

              {/* Movie Details */}
              {selectedMovie && (
                <Card className="bg-muted/50">
                  <CardContent className="pt-4">
                    <div className="grid grid-cols-1 md:grid-cols-3 gap-4 text-sm">
                      <div>
                        <span className="font-medium">Thời lượng:</span> {selectedMovie.durationMinutes} phút
                      </div>
                      <div>
                        <span className="font-medium">Thể loại:</span> {selectedMovie.category}
                      </div>
                      <div>
                        <span className="font-medium">Trạng thái:</span> {selectedMovie.status}
                      </div>
                    </div>
                  </CardContent>
                </Card>
              )}

              {/* Cinema Selection */}
              <FormField
                control={form.control}
                name="cinemaId"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Rạp chiếu</FormLabel>
                    <Select onValueChange={handleCinemaChange} value={field.value}>
                      <FormControl>
                        <SelectTrigger>
                          <SelectValue placeholder="Chọn rạp chiếu" />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        {cinemas.map((cinema: any) => (
                          <SelectItem key={cinema.id} value={cinema.id}>
                            {cinema.name}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                    <FormMessage />
                  </FormItem>
                )}
              />

              {/* Screen Selection */}
              <FormField
                control={form.control}
                name="screenId"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Phòng chiếu</FormLabel>
                    <Select onValueChange={field.onChange} value={field.value} disabled={!selectedCinemaId}>
                      <FormControl>
                        <SelectTrigger>
                          <SelectValue placeholder={selectedCinemaId ? "Chọn phòng chiếu" : "Chọn rạp trước"} />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        {screens.map((screen) => (
                          <SelectItem key={screen.id} value={screen.id}>
                            {screen.name} ({screen.totalSeats} ghế)
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                    <FormMessage />
                  </FormItem>
                )}
              />

              {/* Date and Time */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <FormField
                  control={form.control}
                  name="startDate"
                  render={({ field }) => (
                    <FormItem className="flex flex-col">
                      <FormLabel>Ngày chiếu</FormLabel>
                      <Popover>
                        <PopoverTrigger asChild>
                          <FormControl>
                            <Button
                              variant="outline"
                              className={cn(
                                "w-full pl-3 text-left font-normal",
                                !field.value && "text-muted-foreground"
                              )}
                            >
                              {field.value ? (
                                format(field.value, "dd/MM/yyyy", { locale: vi })
                              ) : (
                                <span>Chọn ngày</span>
                              )}
                              <CalendarIcon className="ml-auto h-4 w-4 opacity-50" />
                            </Button>
                          </FormControl>
                        </PopoverTrigger>
                        <PopoverContent className="w-auto p-0" align="start">
                          <Calendar
                            mode="single"
                            selected={field.value}
                            onSelect={field.onChange}
                            disabled={(date) =>
                              date < new Date("1900-01-01")
                            }
                            initialFocus
                          />
                        </PopoverContent>
                      </Popover>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="startTime"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Giờ chiếu</FormLabel>
                      <FormControl>
                        <Input
                          type="time"
                          {...field}
                          className="flex items-center"
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </div>

              {/* Duration and Price */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <FormField
                  control={form.control}
                  name="durationMinutes"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Thời lượng (phút)</FormLabel>
                      <FormControl>
                        <Input
                          type="number"
                          {...field}
                          onChange={(e) => field.onChange(parseInt(e.target.value) || 0)}
                          placeholder="120"
                        />
                      </FormControl>
                      <FormDescription>
                        Thời lượng của phim (phút)
                      </FormDescription>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="basePrice"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Giá cơ bản (VNĐ)</FormLabel>
                      <FormControl>
                        <Input
                          type="number"
                          {...field}
                          onChange={(e) => field.onChange(parseInt(e.target.value) || 0)}
                          placeholder="75000"
                        />
                      </FormControl>
                      <FormDescription>
                        Giá vé cơ bản cho suất chiếu này
                      </FormDescription>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </div>

              {/* Submit Buttons */}
              <div className="flex items-center gap-4 pt-4">
                <Button
                  type="submit"
                  disabled={updateMutation.isPending}
                  className="flex items-center gap-2"
                >
                  {updateMutation.isPending ? (
                    <>
                      <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white"></div>
                      Đang cập nhật...
                    </>
                  ) : (
                    <>
                      <Save className="h-4 w-4" />
                      Cập nhật lịch chiếu
                    </>
                  )}
                </Button>
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => navigate('/showtimes')}
                  className="flex items-center gap-2"
                >
                  <X className="h-4 w-4" />
                  Hủy
                </Button>
              </div>
            </form>
          </Form>
        </CardContent>
      </Card>
    </div>
  );
};

export default EditShowtime;
