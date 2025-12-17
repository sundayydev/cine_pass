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
import { CalendarIcon, Loader2, ArrowLeft, Save, Upload, X, ImageIcon } from "lucide-react";

// API & Config
import { movieApi } from "@/services/apiMovie"; // Đường dẫn tới file api bạn vừa cung cấp
import { PATHS } from "@/config/paths";
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
  posterUrl: z.string().url().optional().or(z.literal("")),
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
});

type FormValues = z.infer<typeof formSchema>;

const CreateMoviePage = () => {
  const navigate = useNavigate();
  const [isLoading, setIsLoading] = useState(false);
  const [posterFile, setPosterFile] = useState<File | null>(null);

  // 2. Setup Form
  const form = useForm<FormValues>({
    resolver: zodResolver(formSchema) as Resolver<FormValues>,
    defaultValues: {
      title: "",
      durationMinutes: 0,
      description: "",
      trailerUrl: "",
      posterUrl: "",
      status: "COMING_SOON",
      category: "MOVIE",
      releaseDate: new Date(),
    },
  });

  // 3. Xử lý Submit
  const onSubmit = async (values: FormValues) => {
    setIsLoading(true);
    let createdMovieId: string | null = null;

    try {
      // Format date thành YYYY-MM-DD cho PostgreSQL date type
      const formatDateForPostgres = (date: Date): string => {
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, "0");
        const day = String(date.getDate()).padStart(2, "0");
        return `${year}-${month}-${day}`;
      };

      // Map string status to numeric enum
      const mapStatusToNumber = (status: string): number => {
        const statusMap: Record<string, number> = {
          COMING_SOON: 0,
          NOW_SHOWING: 1,
          ENDED: 2,
          CANCELLED: 2,
        };
        return statusMap[status] ?? 0;
      };

      // Map string category to numeric enum
      const mapCategoryToNumber = (category: string): number => {
        const categoryMap: Record<string, number> = {
          MOVIE: 0,
          SERIES: 1,
          DOCUMENTARY: 2,
          ANIMATION: 3,
          ACTION: 4,
          COMEDY: 5,
          DRAMA: 6,
          HORROR: 7,
          ROMANCE: 8,
          SCIFI: 9,
          THRILLER: 10,
          WAR: 11,
          WESTERN: 12,
          MUSICAL: 13,
          FAMILY: 14,
          FANTASY: 15,
          ADVENTURE: 16,
          BIOGRAPHY: 17,
          HISTORY: 18,
          SPORT: 19,
          RELIGIOUS: 20,
          OTHER: 21,
        };
        return categoryMap[category] ?? 0;
      };

      // BƯỚC 1: Tạo phim TRƯỚC (không có posterUrl)
      toast.info("Đang tạo phim...");
      const createPayload = {
        title: values.title,
        durationMinutes: values.durationMinutes,
        description: values.description,
        trailerUrl: values.trailerUrl || undefined,
        // Không gửi posterUrl ở bước này
        releaseDate: formatDateForPostgres(values.releaseDate),
        category: mapCategoryToNumber(values.category),
        status: mapStatusToNumber(values.status),
      };

      console.log("Creating movie with payload:", createPayload);
      const createdMovie = await movieApi.create(createPayload);
      createdMovieId = createdMovie.id;

      console.log("Movie created with ID:", createdMovieId);

      // BƯỚC 2: Upload poster với movieId (nếu có)
      if (posterFile && createdMovieId) {
        toast.info("Đang upload poster...");

        const { uploadApi } = await import("@/services/apiUpload");
        const uploadResult = await uploadApi.uploadEntityImage(
          posterFile,
          "movie",
          createdMovieId
        );

        console.log("Poster uploaded:", uploadResult.secureUrl);

        // BƯỚC 3: Cập nhật posterUrl cho phim
        toast.info("Đang cập nhật poster...");
        await movieApi.update(createdMovieId, {
          posterUrl: uploadResult.secureUrl,
        });

        console.log("Movie updated with posterUrl");
      }

      toast.success("Đã tạo phim mới thành công!", {});
      navigate(PATHS.MOVIES);
    } catch (error: any) {
      console.error("Create Error:", error);

      // Nếu đã tạo phim nhưng lỗi upload/update, thông báo cho user
      if (createdMovieId) {
        toast.warning(
          "Phim đã được tạo nhưng có lỗi khi upload poster. Bạn có thể chỉnh sửa phim để thêm poster sau.",
          { duration: 7000 }
        );
        navigate(PATHS.MOVIES);
        return;
      }

      // Lỗi khi tạo phim
      let errorMessage = "Không thể tạo phim. Vui lòng thử lại.";

      if (error.response?.data) {
        const responseData = error.response.data;
        if (responseData.message) {
          errorMessage = responseData.message;
        }
        if (
          responseData.errors &&
          Array.isArray(responseData.errors) &&
          responseData.errors.length > 0
        ) {
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

                  {/* Poster Upload - Save file for later upload */}
                  <FormField
                    control={form.control as unknown as Control<FormValues>}
                    name="posterUrl"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Poster Phim</FormLabel>
                        <FormControl>
                          <div className="space-y-2">
                            <div
                              className={cn(
                                "relative rounded-lg border-2 border-dashed transition-colors",
                                posterFile
                                  ? "border-solid"
                                  : "border-muted-foreground/25"
                              )}
                            >
                              <input
                                type="file"
                                accept="image/jpeg,image/jpg,image/png,image/gif,image/webp"
                                onChange={(e) => {
                                  const file = e.target.files?.[0];
                                  if (file) {
                                    // Validate file
                                    const maxSizeMB = 10;
                                    const maxBytes = maxSizeMB * 1024 * 1024;
                                    if (file.size > maxBytes) {
                                      toast.error(
                                        `Kích thước file không được vượt quá ${maxSizeMB}MB`
                                      );
                                      return;
                                    }

                                    const allowedTypes = [
                                      "image/jpeg",
                                      "image/jpg",
                                      "image/png",
                                      "image/gif",
                                      "image/webp",
                                    ];
                                    if (!allowedTypes.includes(file.type)) {
                                      toast.error(
                                        "Chỉ chấp nhận các định dạng: JPG, PNG, GIF, WEBP"
                                      );
                                      return;
                                    }

                                    // Save file for later upload
                                    setPosterFile(file);

                                    // Create preview
                                    const reader = new FileReader();
                                    reader.onloadend = () => {
                                      field.onChange(reader.result as string);
                                    };
                                    reader.readAsDataURL(file);
                                  }
                                }}
                                className="hidden"
                                id="poster-upload"
                              />

                              {posterFile ? (
                                <div className="relative group">
                                  <img
                                    src={field.value}
                                    alt="Preview"
                                    className="w-full h-auto rounded-lg object-cover"
                                  />
                                  <div className="absolute inset-0 bg-black/50 opacity-0 group-hover:opacity-100 transition-opacity rounded-lg flex items-center justify-center gap-2">
                                    <Button
                                      type="button"
                                      size="sm"
                                      variant="secondary"
                                      onClick={() => {
                                        document
                                          .getElementById("poster-upload")
                                          ?.click();
                                      }}
                                    >
                                      <Upload className="h-4 w-4 mr-2" />
                                      Thay đổi
                                    </Button>
                                    <Button
                                      type="button"
                                      size="sm"
                                      variant="destructive"
                                      onClick={() => {
                                        setPosterFile(null);
                                        field.onChange("");
                                      }}
                                    >
                                      <X className="h-4 w-4 mr-2" />
                                      Xóa
                                    </Button>
                                  </div>
                                </div>
                              ) : (
                                <label
                                  htmlFor="poster-upload"
                                  className="w-full p-8 flex flex-col items-center justify-center gap-2 text-muted-foreground hover:text-foreground hover:bg-muted/50 transition-colors rounded-lg cursor-pointer"
                                >
                                  <ImageIcon className="h-12 w-12" />
                                  <div className="text-center">
                                    <p className="text-sm font-medium">
                                      Click để chọn ảnh
                                    </p>
                                    <p className="text-xs mt-1">
                                      PNG, JPG, GIF, WEBP (tối đa 10MB)
                                    </p>
                                  </div>
                                </label>
                              )}
                            </div>
                            <p className="text-sm text-muted-foreground">
                              Ảnh poster sẽ được upload sau khi tạo phim
                            </p>
                          </div>
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
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
