import { useState } from "react";
import { useNavigate } from "react-router-dom";
import {
  useForm,
  type Control,
  type FieldValues,
  type Resolver,
  type SubmitHandler,
} from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";
import { format } from "date-fns";
import { CalendarIcon, Loader2, ArrowLeft, Save } from "lucide-react";

// API & Config
import { movieApi } from "@/services/apiMovie"; // Đường dẫn tới file api bạn vừa cung cấp
import { PATHS } from "@/config/paths";
import type { MoviePayload, MovieCategory } from "@/types/moveType";
import { MOVIE_CATEGORIES } from "@/constants/movieCategory";

// Shadcn UI Components
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import {
  Form,
  FormControl,
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { Calendar } from "@/components/ui/calendar";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { toast } from "sonner";
import { cn } from "@/lib/utils";

// 1. Định nghĩa Schema Validation (Zod)
const formSchema = z.object({
  title: z.string().min(1, "Tên phim không được để trống"),
  durationMinutes: z.coerce.number().min(1, "Thời lượng phải lớn hơn 0"),
  description: z.string().optional(),
  trailerUrl: z.string().url().optional().or(z.literal("")),
  releaseDate: z.date(),
  status: z.enum(["COMING_SOON", "NOW_SHOWING", "ENDED", "CANCELLED"]),
  category: z.enum([
    "MOVIE",
    "SERIES",
    "DOCUMENTARY",
    "ANIMATION",
    "ACTION",
    "COMEDY",
    "DRAMA",
    "HORROR",
    "ROMANCE",
    "SCIFI",
    "THRILLER",
    "WAR",
    "WESTERN",
    "MUSICAL",
    "FAMILY",
    "FANTASY",
    "ADVENTURE",
    "BIOGRAPHY",
    "HISTORY",
    "SPORT",
    "RELIGIOUS",
    "OTHER",
  ]),
  //Todo: Upload poster
});

type FormValues = z.infer<typeof formSchema>;

const CreateMoviePage = () => {
  const navigate = useNavigate();
  const [isLoading, setIsLoading] = useState(false);

  // 2. Setup Form
  const form = useForm<FormValues>({
    resolver: zodResolver(formSchema) as Resolver<FormValues>,
    defaultValues: {
      title: "",
      durationMinutes: 0,
      description: "",
      trailerUrl: "",
      status: "COMING_SOON",
      category: "MOVIE",
      releaseDate: new Date(),
    },
  });

  // 3. Xử lý Submit
  const onSubmit = async (values: FormValues) => {
    setIsLoading(true);
    try {
      // Format date thành YYYY-MM-DD cho PostgreSQL date type
      // PostgreSQL date type chỉ cần format YYYY-MM-DD, không cần time và timezone
      const formatDateForPostgres = (date: Date): string => {
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, "0");
        const day = String(date.getDate()).padStart(2, "0");
        return `${year}-${month}-${day}`;
      };

      // Chuẩn bị payload đúng chuẩn Interface MoviePayload
      const payload: MoviePayload = {
        title: values.title,
        durationMinutes: values.durationMinutes,
        description: values.description,
        trailerUrl: values.trailerUrl || undefined,
        releaseDate: formatDateForPostgres(values.releaseDate), // Format YYYY-MM-DD cho PostgreSQL
        category: values.category as MovieCategory,
        status: values.status,
        // posterFile: ... (Chờ backend hỗ trợ upload)
      };

      console.log("Submitting Payload:", payload);

      // Gọi API từ file bạn cung cấp
      await movieApi.create(payload);

      toast.success("Đã tạo phim mới thành công!", {});

      // Quay về trang danh sách
      navigate(PATHS.MOVIES);
    } catch (error: any) {
      console.error("Create Error:", error);

      // Lấy thông báo lỗi chi tiết từ backend
      let errorMessage = "Không thể tạo phim. Vui lòng thử lại.";

      if (error.response?.data) {
        const responseData = error.response.data;
        // Backend trả về format: { success: false, message: "...", errors: [...] }
        if (responseData.message) {
          errorMessage = responseData.message;
        }
        if (
          responseData.errors &&
          Array.isArray(responseData.errors) &&
          responseData.errors.length > 0
        ) {
          // Nếu có danh sách lỗi validation, hiển thị tất cả
          const errorsList = responseData.errors.join(", ");
          errorMessage = `${errorMessage} ${errorsList}`;
        }
      } else if (error.message) {
        errorMessage = error.message;
      }

      toast.error("Lỗi: " + errorMessage, {
        duration: 5000,
      });
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="flex-1 space-y-4 p-4 md:p-8 pt-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <Button
          variant="outline"
          size="icon"
          onClick={() => navigate(PATHS.MOVIES)}
        >
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <div>
          <h2 className="text-2xl font-bold tracking-tight">Thêm Phim Mới</h2>
          <p className="text-sm text-muted-foreground">
            Nhập thông tin chi tiết cho phim.
          </p>
        </div>
      </div>

      <Form {...form}>
        <form
          onSubmit={form.handleSubmit(onSubmit as SubmitHandler<FieldValues>)}
          className="space-y-8"
        >
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            {/* CỘT TRÁI: Thông tin chính (chiếm 2 phần) */}
            <div className="md:col-span-2 space-y-6">
              <Card>
                <CardHeader>
                  <CardTitle>Thông tin chung</CardTitle>
                </CardHeader>
                <CardContent className="space-y-4">
                  {/* Tên phim */}
                  <FormField
                    control={form.control as unknown as Control<FormValues>}
                    name="title"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>
                          Tên phim <span className="text-red-500">*</span>
                        </FormLabel>
                        <FormControl>
                          <Input
                            placeholder="Ví dụ: Đào, Phở và Piano"
                            {...field}
                          />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    {/* Thời lượng */}
                    <FormField
                      control={form.control as unknown as Control<FormValues>}
                      name="durationMinutes"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>
                            Thời lượng (phút){" "}
                            <span className="text-red-500">*</span>
                          </FormLabel>
                          <FormControl>
                            <Input
                              type="number"
                              min={1}
                              placeholder="120"
                              {...field}
                            />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />

                    {/* Trạng thái */}
                    <FormField
                      control={form.control as unknown as Control<FormValues>}
                      name="status"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Trạng thái</FormLabel>
                          <Select
                            onValueChange={field.onChange}
                            defaultValue={field.value}
                          >
                            <FormControl>
                              <SelectTrigger>
                                <SelectValue placeholder="Chọn trạng thái" />
                              </SelectTrigger>
                            </FormControl>
                            <SelectContent>
                              <SelectItem value="COMING_SOON">
                                Sắp chiếu
                              </SelectItem>
                              <SelectItem value="NOW_SHOWING">
                                Đang chiếu
                              </SelectItem>
                              <SelectItem value="ENDED">Đã kết thúc</SelectItem>
                              <SelectItem value="CANCELLED">Đã hủy</SelectItem>
                            </SelectContent>
                          </Select>
                          <FormMessage />
                        </FormItem>
                      )}
                    />
                  </div>

                  {/* Thể loại phim */}
                  <FormField
                    control={form.control}
                    name="category"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Thể loại phim</FormLabel>
                        <Select
                          onValueChange={field.onChange}
                          defaultValue={field.value}
                        >
                          <FormControl>
                            <SelectTrigger>
                              <SelectValue placeholder="Chọn thể loại" />
                            </SelectTrigger>
                          </FormControl>
                          <SelectContent className="max-h-72">
                            {MOVIE_CATEGORIES.map((cat: { value: string; label: string; }) => (
                              <SelectItem key={cat.value} value={cat.value}>
                                {cat.label}
                              </SelectItem>
                            ))}
                          </SelectContent>
                        </Select>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  {/* Mô tả */}
                  <FormField
                    control={form.control as unknown as Control<FormValues>}
                    name="description"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Mô tả nội dung</FormLabel>
                        <FormControl>
                          <Textarea
                            placeholder="Nhập tóm tắt nội dung phim..."
                            className="min-h-[120px]"
                            {...field}
                          />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                </CardContent>
              </Card>
            </div>

            {/* CỘT PHẢI: Metadata & Media (chiếm 1 phần) */}
            <div className="space-y-6">
              <Card>
                <CardHeader>
                  <CardTitle>Lịch & Media</CardTitle>
                </CardHeader>
                <CardContent className="space-y-4">
                  {/* Ngày khởi chiếu */}
                  <FormField
                    control={form.control as unknown as Control<FormValues>}
                    name="releaseDate"
                    render={({ field }) => (
                      <FormItem className="flex flex-col">
                        <FormLabel>Ngày khởi chiếu</FormLabel>
                        <Popover>
                          <PopoverTrigger asChild>
                            <FormControl>
                              <Button
                                variant={"outline"}
                                className={cn(
                                  "w-full pl-3 text-left font-normal",
                                  !field.value && "text-muted-foreground"
                                )}
                              >
                                {field.value ? (
                                  format(field.value, "dd/MM/yyyy")
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
                              disabled={(date) => date < new Date("1900-01-01")}
                              initialFocus
                            />
                          </PopoverContent>
                        </Popover>
                        <FormDescription>
                          Ngày phim bắt đầu chiếu tại rạp.
                        </FormDescription>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  {/* Trailer URL */}
                  <FormField
                    control={form.control as unknown as Control<FormValues>}
                    name="trailerUrl"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Link Trailer (Youtube)</FormLabel>
                        <FormControl>
                          <Input
                            placeholder="https://youtube.com/..."
                            {...field}
                          />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  {/* Poster Placeholder */}
                  <div className="rounded-md border border-dashed p-4 text-center bg-muted/20">
                    <p className="text-sm text-muted-foreground mb-2">
                      Poster Phim
                    </p>
                    <div className="flex items-center justify-center h-32 bg-muted rounded">
                      <span className="text-xs text-gray-500">
                        Chức năng upload đang phát triển
                      </span>
                    </div>
                  </div>
                </CardContent>
              </Card>
            </div>
          </div>

          {/* Action Buttons */}
          <div className="flex justify-end gap-4">
            <Button
              type="button"
              variant="outline"
              onClick={() => navigate(PATHS.MOVIES)}
            >
              Hủy bỏ
            </Button>
            <Button type="submit" disabled={isLoading}>
              {isLoading ? (
                <>
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" /> Đang lưu...
                </>
              ) : (
                <>
                  <Save className="mr-2 h-4 w-4" /> Lưu phim
                </>
              )}
            </Button>
          </div>
        </form>
      </Form>
    </div>
  );
};

export default CreateMoviePage;
