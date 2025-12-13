import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";
import { useNavigate } from "react-router-dom";
import { ArrowLeft, Loader2, X, Plus } from "lucide-react";
import { toast } from "sonner";

// API Services
import { cinemaApi, type CinemaCreateDto } from "@/services/apiCinema";
import { PATHS } from "@/config/paths";

// Shadcn UI
import { Button } from "@/components/ui/button";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
  FormDescription,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Select, SelectTrigger, SelectValue, SelectContent, SelectItem } from "@/components/ui/select";
import { Badge } from "@/components/ui/badge";

// Helper function to generate slug from name
const generateSlug = (name: string): string => {
  return name
    .toLowerCase()
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "") // Remove diacritics
    .replace(/[^a-z0-9]+/g, "-") // Replace non-alphanumeric with dash
    .replace(/^-+|-+$/g, ""); // Remove leading/trailing dashes
};

// Schema validation
const cinemaSchema = z.object({
  name: z.string().min(1, "Tên rạp không được để trống").max(255, "Tên rạp tối đa 255 ký tự"),
  slug: z.string().regex(/^[a-z0-9-]+$/, "Slug chỉ được chứa chữ thường, số và dấu gạch ngang").optional().or(z.literal("")),
  description: z.string().optional(),
  address: z.string().max(500, "Địa chỉ tối đa 500 ký tự").optional(),
  city: z.string().max(100, "Thành phố tối đa 100 ký tự").optional(),
  phone: z.string().max(50, "Số điện thoại tối đa 50 ký tự").optional(),
  email: z.string().email("Email không hợp lệ").max(255, "Email tối đa 255 ký tự").optional().or(z.literal("")),
  website: z.string().url("Website phải là một đường dẫn hợp lệ").max(500, "Website tối đa 500 ký tự").optional().or(z.literal("")),
  latitude: z.number().min(-90, "Vĩ độ phải từ -90 đến 90").max(90, "Vĩ độ phải từ -90 đến 90").optional().nullable(),
  longitude: z.number().min(-180, "Kinh độ phải từ -180 đến 180").max(180, "Kinh độ phải từ -180 đến 180").optional().nullable(),
  bannerUrl: z.string().url("Banner URL không hợp lệ").optional().or(z.literal("")),
  totalScreens: z.number().min(0, "Số lượng phòng chiếu không hợp lệ").max(100, "Số lượng phòng chiếu không hợp lệ").optional(),
  facilities: z.array(z.string()).optional(),
  isActive: z.boolean(),
});

type CinemaFormValues = z.infer<typeof cinemaSchema>;

const CreateCinemaPage = () => {
  const navigate = useNavigate();
  const [isLoading, setIsLoading] = useState(false);
  const [facilityInput, setFacilityInput] = useState("");

  const form = useForm<CinemaFormValues>({
    resolver: zodResolver(cinemaSchema),
    defaultValues: {
      name: "",
      slug: "",
      description: "",
      address: "",
      city: "",
      phone: "",
      email: "",
      website: "",
      latitude: undefined,
      longitude: undefined,
      bannerUrl: "",
      totalScreens: 0,
      facilities: [],
      isActive: true,
    },
  });

  const facilities = form.watch("facilities") || [];

  const handleAddFacility = () => {
    if (facilityInput.trim()) {
      const currentFacilities = form.getValues("facilities") || [];
      if (!currentFacilities.includes(facilityInput.trim())) {
        form.setValue("facilities", [...currentFacilities, facilityInput.trim()]);
        setFacilityInput("");
      }
    }
  };

  const handleRemoveFacility = (facility: string) => {
    const currentFacilities = form.getValues("facilities") || [];
    form.setValue("facilities", currentFacilities.filter((f) => f !== facility));
  };

  const handleNameChange = (name: string) => {
    form.setValue("name", name);
    // Auto-generate slug if slug is empty
    if (!form.getValues("slug")) {
      form.setValue("slug", generateSlug(name));
    }
  };

  const onSubmit = async (data: CinemaFormValues) => {
    setIsLoading(true);
    try {
      const dto: CinemaCreateDto = {
        name: data.name,
        slug: data.slug || undefined,
        description: data.description || undefined,
        address: data.address || undefined,
        city: data.city || undefined,
        phone: data.phone || undefined,
        email: data.email || undefined,
        website: data.website || undefined,
        latitude: data.latitude ?? undefined,
        longitude: data.longitude ?? undefined,
        bannerUrl: data.bannerUrl || undefined,
        totalScreens: data.totalScreens ?? undefined,
        facilities: data.facilities && data.facilities.length > 0 ? data.facilities : undefined,
        isActive: data.isActive,
      };

      await cinemaApi.create(dto);
      toast.success("Tạo rạp chiếu phim thành công");
      navigate(PATHS.CINEMAS);
    } catch (error) {
      console.error("Error creating cinema:", error);
      toast.error(error instanceof Error ? error.message : "Lỗi khi tạo rạp chiếu phim");
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="icon" onClick={() => navigate(PATHS.CINEMAS)}>
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Thêm Rạp Chiếu Phim Mới</h1>
          <p className="text-muted-foreground mt-1">Điền thông tin để tạo rạp chiếu phim mới</p>
        </div>
      </div>

      {/* Form */}
      <Card>
        <CardHeader>
          <CardTitle>Thông tin Rạp Chiếu Phim</CardTitle>
          <CardDescription>Vui lòng điền đầy đủ thông tin bên dưới</CardDescription>
        </CardHeader>
        <CardContent>
          <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
              {/* Thông tin cơ bản */}
              <div className="space-y-4">
                <h3 className="text-lg font-semibold">Thông tin cơ bản</h3>
                
                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                  {/* Tên rạp */}
                <FormField
                  control={form.control}
                  name="name"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Tên Rạp *</FormLabel>
                        <FormControl>
                          <Input 
                            placeholder="Ví dụ: CGV Vincom" 
                            {...field}
                            onChange={(e) => {
                              handleNameChange(e.target.value);
                            }}
                          />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  {/* Slug */}
                  <FormField
                    control={form.control}
                    name="slug"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Slug</FormLabel>
                        <FormControl>
                          <Input placeholder="Tự động tạo từ tên rạp" {...field} />
                        </FormControl>
                        <FormDescription>
                          URL-friendly identifier (tự động tạo từ tên nếu để trống)
                        </FormDescription>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                </div>

                {/* Mô tả */}
                <FormField
                  control={form.control}
                  name="description"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Mô Tả</FormLabel>
                      <FormControl>
                        <Textarea 
                          placeholder="Mô tả về rạp chiếu phim..." 
                          className="min-h-[100px]"
                          {...field} 
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </div>

              {/* Địa điểm & Liên hệ */}
              <div className="space-y-4">
                <h3 className="text-lg font-semibold">Địa điểm & Liên hệ</h3>
                
                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                  {/* Địa chỉ */}
                  <FormField
                    control={form.control}
                    name="address"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Địa Chỉ</FormLabel>
                        <FormControl>
                          <Input placeholder="Ví dụ: 72 Lê Thánh Tôn, Quận 1" {...field} />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  {/* Thành phố */}
                  <FormField
                    control={form.control}
                    name="city"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Thành Phố</FormLabel>
                        <FormControl>
                          <Input placeholder="Ví dụ: Hà Nội" {...field} />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                  {/* Vĩ độ */}
                  <FormField
                    control={form.control}
                    name="latitude"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Vĩ Độ (Latitude)</FormLabel>
                        <FormControl>
                          <Input 
                            type="number" 
                            step="any"
                            placeholder="Ví dụ: 21.0285" 
                            {...field}
                            onChange={(e) => {
                              const value = e.target.value;
                              field.onChange(value === "" ? undefined : parseFloat(value));
                            }}
                            value={field.value ?? ""}
                          />
                        </FormControl>
                        <FormDescription>Giá trị từ -90 đến 90</FormDescription>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  {/* Kinh độ */}
                  <FormField
                    control={form.control}
                    name="longitude"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Kinh Độ (Longitude)</FormLabel>
                        <FormControl>
                          <Input 
                            type="number" 
                            step="any"
                            placeholder="Ví dụ: 105.8542" 
                            {...field}
                            onChange={(e) => {
                              const value = e.target.value;
                              field.onChange(value === "" ? undefined : parseFloat(value));
                            }}
                            value={field.value ?? ""}
                          />
                        </FormControl>
                        <FormDescription>Giá trị từ -180 đến 180</FormDescription>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                </div>

                <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                  {/* Số điện thoại */}
                  <FormField
                    control={form.control}
                    name="phone"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Số Điện Thoại</FormLabel>
                        <FormControl>
                          <Input placeholder="Ví dụ: 0123456789" {...field} />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  {/* Email */}
                  <FormField
                    control={form.control}
                    name="email"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Email</FormLabel>
                        <FormControl>
                          <Input type="email" placeholder="Ví dụ: contact@cinema.com" {...field} />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  {/* Website */}
                  <FormField
                    control={form.control}
                    name="website"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Website</FormLabel>
                        <FormControl>
                          <Input type="url" placeholder="Ví dụ: https://cinema.com" {...field} />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                </div>
              </div>

              {/* Hình ảnh & Thông tin phụ */}
              <div className="space-y-4">
                <h3 className="text-lg font-semibold">Hình ảnh & Thông tin phụ</h3>
                
                <FormField
                  control={form.control}
                  name="bannerUrl"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Banner URL</FormLabel>
                      <FormControl>
                        <Input type="url" placeholder="https://example.com/banner.jpg" {...field} />
                      </FormControl>
                      <FormDescription>Đường dẫn đến hình ảnh banner của rạp</FormDescription>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="totalScreens"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Tổng Số Phòng Chiếu</FormLabel>
                      <FormControl>
                        <Input 
                          type="number" 
                          min="0"
                          max="100"
                          placeholder="0" 
                          {...field}
                          onChange={(e) => {
                            const value = e.target.value;
                            field.onChange(value === "" ? 0 : parseInt(value));
                          }}
                          value={field.value ?? 0}
                        />
                      </FormControl>
                      <FormDescription>Số lượng phòng chiếu trong rạp</FormDescription>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                {/* Facilities */}
                <FormField
                  control={form.control}
                  name="facilities"
                  render={() => (
                    <FormItem>
                      <FormLabel>Tiện Ích</FormLabel>
                      <FormControl>
                        <div className="space-y-2">
                          <div className="flex gap-2">
                            <Input
                              placeholder="Nhập tiện ích và nhấn Enter"
                              value={facilityInput}
                              onChange={(e) => setFacilityInput(e.target.value)}
                              onKeyDown={(e) => {
                                if (e.key === "Enter") {
                                  e.preventDefault();
                                  handleAddFacility();
                                }
                              }}
                            />
                            <Button type="button" onClick={handleAddFacility} variant="outline">
                              <Plus className="h-4 w-4" />
                            </Button>
                          </div>
                          {facilities.length > 0 && (
                            <div className="flex flex-wrap gap-2">
                              {facilities.map((facility) => (
                                <Badge key={facility} variant="secondary" className="gap-1">
                                  {facility}
                                  <button
                                    type="button"
                                    onClick={() => handleRemoveFacility(facility)}
                                    className="ml-1 hover:bg-destructive/20 rounded-full p-0.5"
                                  >
                                    <X className="h-3 w-3" />
                                  </button>
                                </Badge>
                              ))}
                            </div>
                          )}
                        </div>
                      </FormControl>
                      <FormDescription>Thêm các tiện ích của rạp (ví dụ: WiFi, Parking, 3D, IMAX...)</FormDescription>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </div>

              {/* Trạng thái */}
              <FormField
                control={form.control}
                name="isActive"
                render={({ field }) => (
                  <FormItem className="flex flex-row items-center justify-between rounded-lg border p-4">
                    <div className="space-y-0.5">
                      <FormLabel className="text-base">Trạng Thái</FormLabel>
                      <FormDescription>
                        Chọn trạng thái hoạt động của rạp chiếu phim
                      </FormDescription>
                    </div>
                    <FormControl>
                      <Select
                        value={field.value ? "active" : "inactive"}
                        onValueChange={(value) => field.onChange(value === "active")}
                      >
                        <SelectTrigger className="w-[180px]">
                          <SelectValue />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="active">Đang hoạt động</SelectItem>
                          <SelectItem value="inactive">Ngừng hoạt động</SelectItem>
                        </SelectContent>
                      </Select>
                    </FormControl>
                  </FormItem>
                )}
              />

              {/* Actions */}
              <div className="flex items-center justify-end gap-4">
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => navigate(PATHS.CINEMAS)}
                  disabled={isLoading}
                >
                  Hủy
                </Button>
                <Button type="submit" disabled={isLoading}>
                  {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                  Tạo Rạp
                </Button>
              </div>
            </form>
          </Form>
        </CardContent>
      </Card>
    </div>
  );
};

export default CreateCinemaPage;

