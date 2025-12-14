import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";
import { useNavigate } from "react-router-dom";
import { ArrowLeft, Loader2 } from "lucide-react";
import { toast } from "sonner";

// API Services
import { seatTypeApi, type SeatTypeCreateDto } from "@/services/apiSeatType";
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
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";

// Schema validation
const seatTypeSchema = z.object({
  code: z.string().min(1, "Mã loại ghế không được để trống").max(50, "Mã loại ghế tối đa 50 ký tự"),
  name: z.string().max(255, "Tên loại ghế tối đa 255 ký tự").optional(),
  surchargeRate: z.number().min(0.1, "Tỷ lệ phụ thu phải từ 0.1 đến 10.0").max(10.0, "Tỷ lệ phụ thu phải từ 0.1 đến 10.0"),
});

type SeatTypeFormValues = z.infer<typeof seatTypeSchema>;

const CreateSeatTypePage = () => {
  const navigate = useNavigate();
  const [isLoading, setIsLoading] = useState(false);

  const form = useForm<SeatTypeFormValues>({
    resolver: zodResolver(seatTypeSchema),
    defaultValues: {
      code: "",
      name: "",
      surchargeRate: 1.0,
    },
  });

  const onSubmit = async (data: SeatTypeFormValues) => {
    setIsLoading(true);
    try {
      const dto: SeatTypeCreateDto = {
        code: data.code,
        name: data.name || undefined,
        surchargeRate: data.surchargeRate,
      };

      await seatTypeApi.create(dto);
      toast.success("Tạo loại ghế thành công");
      navigate("/seat-types");
    } catch (error) {
      console.error("Error creating seat type:", error);
      toast.error(error instanceof Error ? error.message : "Lỗi khi tạo loại ghế");
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="icon" onClick={() => navigate("/seat-types")}>
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Thêm Loại Ghế Mới</h1>
          <p className="text-muted-foreground mt-1">Điền thông tin để tạo loại ghế mới</p>
        </div>
      </div>

      {/* Form */}
      <Card>
        <CardHeader>
          <CardTitle>Thông tin Loại Ghế</CardTitle>
          <CardDescription>Vui lòng điền đầy đủ thông tin bên dưới</CardDescription>
        </CardHeader>
        <CardContent>
          <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                {/* Mã loại */}
                <FormField
                  control={form.control}
                  name="code"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Mã Loại *</FormLabel>
                      <FormControl>
                        <Input 
                          placeholder="Ví dụ: VIP, NORMAL, COUPLE" 
                          {...field}
                          className="font-mono"
                          onChange={(e) => {
                            // Auto uppercase
                            field.onChange(e.target.value.toUpperCase());
                          }}
                        />
                      </FormControl>
                      <FormDescription>
                        Mã định danh duy nhất cho loại ghế (tối đa 50 ký tự)
                      </FormDescription>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                {/* Tên loại */}
                <FormField
                  control={form.control}
                  name="name"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Tên Loại</FormLabel>
                      <FormControl>
                        <Input placeholder="Ví dụ: Ghế VIP, Ghế Thường" {...field} />
                      </FormControl>
                      <FormDescription>
                        Tên hiển thị của loại ghế (tùy chọn)
                      </FormDescription>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </div>

              {/* Tỷ lệ phụ thu */}
              <FormField
                control={form.control}
                name="surchargeRate"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Tỷ Lệ Phụ Thu *</FormLabel>
                    <FormControl>
                      <Input 
                        type="number" 
                        step="0.01"
                        min="0.1"
                        max="10.0"
                        placeholder="1.0" 
                        {...field}
                        onChange={(e) => {
                          const value = e.target.value;
                          field.onChange(value === "" ? 1.0 : parseFloat(value));
                        }}
                        value={field.value ?? 1.0}
                      />
                    </FormControl>
                    <FormDescription>
                      Tỷ lệ nhân giá vé (ví dụ: 1.0 = giá gốc, 1.5 = tăng 50%, 2.0 = tăng 100%)
                      <br />
                      Giá trị từ 0.1 đến 10.0
                    </FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />

              {/* Preview */}
              {form.watch("surchargeRate") && (
                <div className="rounded-lg border p-4 bg-muted/50">
                  <p className="text-sm font-medium mb-2">Xem trước:</p>
                  <div className="space-y-1 text-sm">
                    <p>
                      <span className="text-muted-foreground">Tỷ lệ phụ thu:</span>{" "}
                      <span className="font-semibold">
                        {(form.watch("surchargeRate") * 100).toFixed(0)}%
                      </span>
                      {" "}(x{form.watch("surchargeRate").toFixed(2)})
                    </p>
                    <p className="text-muted-foreground">
                      Ví dụ: Nếu giá vé gốc là 100,000đ, giá vé với loại ghế này sẽ là{" "}
                      <span className="font-semibold text-foreground">
                        {(100000 * form.watch("surchargeRate")).toLocaleString("vi-VN")}đ
                      </span>
                    </p>
                  </div>
                </div>
              )}

              {/* Actions */}
              <div className="flex items-center justify-end gap-4">
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => navigate("/seat-types")}
                  disabled={isLoading}
                >
                  Hủy
                </Button>
                <Button type="submit" disabled={isLoading}>
                  {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                  Tạo Loại Ghế
                </Button>
              </div>
            </form>
          </Form>
        </CardContent>
      </Card>
    </div>
  );
};

export default CreateSeatTypePage;

