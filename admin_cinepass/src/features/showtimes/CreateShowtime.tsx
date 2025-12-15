import React, { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { format } from 'date-fns';
import { vi } from 'date-fns/locale';
import { CalendarIcon, Clock, Film, MapPin, DollarSign, Save, X } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Form, FormControl, FormDescription, FormField, FormItem, FormLabel, FormMessage } from '@/components/ui/form';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Popover, PopoverContent, PopoverTrigger } from '@/components/ui/popover';
import { Calendar } from '@/components/ui/calendar';
import { Textarea } from '@/components/ui/textarea';
import { toast } from 'sonner';
import { cn } from '@/lib/utils';
import { showtimeApi } from '@/services/apiShowtime';
import type { ShowtimeCreatePayload } from '@/services/apiShowtime';
import { movieApi } from '@/services/apiMovie';
import { screenApi } from '@/services/apiScreen';
import { cinemaApi } from '@/services/apiCinema';

// Validation schema
const createShowtimeSchema = z.object({
  movieId: z.string().min(1, 'Vui lòng chọn phim'),
  cinemaId: z.string().min(1, 'Vui lòng chọn rạp chiếu'),
  screenId: z.string().min(1, 'Vui lòng chọn phòng chiếu'),
  startDate: z.date({
    message: 'Vui lòng chọn ngày chiếu',
  }),
  startTime: z.string().min(1, 'Vui lòng chọn giờ chiếu'),
  durationMinutes: z.number().min(1, 'Thời lượng phải lớn hơn 0'),
  basePrice: z.number().min(0, 'Giá cơ bản phải >= 0'),
}).refine((data) => {
  // Check if selected screen belongs to selected cinema
  return true; // This will be validated on the backend
}, {
  message: "Phòng chiếu không thuộc rạp đã chọn",
  path: ["screenId"],
});

type CreateShowtimeForm = z.infer<typeof createShowtimeSchema>;

const CreateShowtime = () => {
  const [selectedCinemaId, setSelectedCinemaId] = useState<string>('');
  const queryClient = useQueryClient();

  const form = useForm<CreateShowtimeForm>({
    resolver: zodResolver(createShowtimeSchema),
    defaultValues: {
      movieId: '',
      cinemaId: '',
      screenId: '',
      startDate: undefined,
      startTime: '',
      durationMinutes: 0,
      basePrice: 0,
    },
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

  // Create mutation
  const createMutation = useMutation({
    mutationFn: (payload: ShowtimeCreatePayload) => showtimeApi.create(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['showtimes'] });
      toast.success('Tạo lịch chiếu thành công');
      form.reset();
      setSelectedCinemaId('');
    },
    onError: (error: any) => {
      toast.error(`Lỗi khi tạo lịch chiếu: ${error.message}`);
    },
  });

  const onSubmit = (data: CreateShowtimeForm) => {
    // Combine date and time
    const startDateTime = new Date(data.startDate);
    const [hours, minutes] = data.startTime.split(':').map(Number);
    startDateTime.setHours(hours, minutes, 0, 0);

    // Calculate end time
    const endDateTime = new Date(startDateTime);
    endDateTime.setMinutes(endDateTime.getMinutes() + data.durationMinutes);

    const payload: ShowtimeCreatePayload = {
      movieId: data.movieId,
      screenId: data.screenId,
      startTime: startDateTime.toISOString(),
      basePrice: data.basePrice,
    };

    createMutation.mutate(payload);
  };

  const handleCinemaChange = (cinemaId: string) => {
    setSelectedCinemaId(cinemaId);
    form.setValue('cinemaId', cinemaId);
    form.setValue('screenId', ''); // Reset screen selection
  };

  const selectedMovie = movies.find(movie => movie.id === form.watch('movieId'));

  return (
    <div className="p-6 max-w-4xl mx-auto">
      <div className="mb-6">
        <h1 className="text-3xl font-bold tracking-tight">Tạo lịch chiếu mới</h1>
        <p className="text-muted-foreground">
          Thêm lịch chiếu phim mới vào hệ thống
        </p>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Film className="h-5 w-5" />
            Thông tin lịch chiếu
          </CardTitle>
          <CardDescription>
            Điền đầy đủ thông tin để tạo lịch chiếu mới
          </CardDescription>
        </CardHeader>
        <CardContent>
          <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
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
                        {moviesLoading ? (
                          <SelectItem value="null" disabled>Đang tải...</SelectItem>
                        ) : (
                          movies.map((movie) => (
                            <SelectItem key={movie.id} value={movie.id}>
                              {movie.title}
                            </SelectItem>
                          ))
                        )}
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
                        {cinemasLoading ? (
                          <SelectItem value="null" disabled>Đang tải...</SelectItem>
                        ) : (
                          cinemas.map((cinema: any) => (
                            <SelectItem key={cinema.id} value={cinema.id}>
                              {cinema.name}
                            </SelectItem>
                          ))
                        )}
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
                        {screensLoading ? (
                          <SelectItem value="null" disabled>Đang tải...</SelectItem>
                        ) : (
                          screens.map((screen) => (
                            <SelectItem key={screen.id} value={screen.id}>
                              {screen.name} ({screen.totalSeats} ghế)
                            </SelectItem>
                          ))
                        )}
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
                              date < new Date() || date < new Date("1900-01-01")
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
                  disabled={createMutation.isPending}
                  className="flex items-center gap-2"
                >
                  {createMutation.isPending ? (
                    <>
                      <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white"></div>
                      Đang tạo...
                    </>
                  ) : (
                    <>
                      <Save className="h-4 w-4" />
                      Tạo lịch chiếu
                    </>
                  )}
                </Button>
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => form.reset()}
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

export default CreateShowtime;
