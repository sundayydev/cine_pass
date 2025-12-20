import { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import { Eye, EyeOff, Loader2 } from 'lucide-react';

// Shadcn UI Components
import { Button } from '@/components/ui/button';
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/components/ui/form';
import { Input } from '@/components/ui/input';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';

// Configs
import { PATHS } from '@/config/paths';

// Context
import { useAuth } from '@/context/AuthContext';

// 1. Định nghĩa Schema Validate với Zod
// Email phải đúng định dạng, password tối thiểu 6 ký tự
const loginSchema = z.object({
  email: z.string().email({ message: "Email không hợp lệ" }),
  password: z.string().min(6, { message: "Mật khẩu phải có ít nhất 6 ký tự" }),
});

// Tạo kiểu dữ liệu TypeScript từ schema Zod
type LoginValues = z.infer<typeof loginSchema>;

const LoginPage = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { login, isAuthenticated, isLoading: authLoading } = useAuth();

  // State để quản lý việc ẩn/hiện mật khẩu
  const [showPassword, setShowPassword] = useState(false);
  // State để hiển thị loading khi đang gọi API
  const [isLoading, setIsLoading] = useState(false);

  // Nếu đã đăng nhập, redirect về trang trước đó hoặc dashboard
  useEffect(() => {
    if (isAuthenticated) {
      const from = (location.state as { from?: Location })?.from?.pathname || PATHS.DASHBOARD;
      navigate(from, { replace: true });
    }
  }, [isAuthenticated, navigate, location]);

  // 2. Cấu hình React Hook Form
  const form = useForm<LoginValues>({
    resolver: zodResolver(loginSchema), // Kết nối Zod với React Hook Form
    defaultValues: {
      email: '',
      password: '',
    },
  });

  // 3. Hàm xử lý khi submit form (chỉ chạy khi validate thành công)
  const onSubmit = async (data: LoginValues) => {
    // Xóa lỗi cũ (nếu có) trước khi bắt đầu request mới
    form.clearErrors('root');
    setIsLoading(true);

    try {
      await login(data.email, data.password);

      // Nếu thành công -> Chuyển hướng
      // Lưu ý: Không cần set setIsLoading(false) ở đây vì component sẽ unmount hoặc chuyển trang ngay
      const from = (location.state as { from?: Location })?.from?.pathname || PATHS.DASHBOARD;
      navigate(from, { replace: true });

    } catch (error) {
      // Chỉ tắt loading khi GẶP LỖI để người dùng thử lại
      setIsLoading(false);

      console.error("Login error:", error);
      const errorMessage = error instanceof Error
        ? error.message
        : "Đăng nhập thất bại. Vui lòng kiểm tra lại thông tin.";

      form.setError('root', {
        type: 'manual',
        message: errorMessage,
      });
    }
  };

  // Hiển thị loading nếu đang kiểm tra authentication
  if (authLoading) {
    return (
      <div className="flex items-center justify-center">
        <div className="flex flex-col items-center gap-4">
          <Loader2 className="h-8 w-8 animate-spin" />
          <p className="text-sm text-muted-foreground">Đang kiểm tra...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="flex items-center justify-center w-full">
      <Card className="w-full max-w-sm shadow-lg">
        <CardHeader className="space-y-1">
          <CardTitle className="text-2xl font-bold">Đăng nhập</CardTitle>
          <CardDescription>
            Nhập email và mật khẩu để truy cập hệ thống quản trị.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">

              {/* Email Field - Disable khi loading */}
              <FormField
                control={form.control}
                name="email"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Email</FormLabel>
                    <FormControl>
                      <Input
                        type="email"
                        placeholder="admin@cinepro.com"
                        disabled={isLoading} // Chặn nhập liệu khi đang load
                        {...field}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              {/* Password Field - Disable khi loading */}
              <FormField
                control={form.control}
                name="password"
                render={({ field }) => (
                  <FormItem>
                    <div className="flex items-center justify-between">
                      <FormLabel>Mật khẩu</FormLabel>
                      <Link
                        to={PATHS.FORGOT_PASSWORD}
                        // Disable link khi đang load bằng pointer-events-none
                        className={`text-sm font-medium text-primary hover:underline ${isLoading ? 'pointer-events-none opacity-50' : ''}`}
                      >
                        Quên mật khẩu?
                      </Link>
                    </div>
                    <FormControl>
                      <div className="relative">
                        <Input
                          type={showPassword ? 'text' : 'password'}
                          placeholder="••••••••"
                          disabled={isLoading} // Chặn nhập liệu
                          {...field}
                        />
                        <Button
                          type="button"
                          variant="ghost"
                          size="sm"
                          className="absolute right-0 top-0 h-full px-3 py-2 hover:bg-transparent"
                          onClick={() => setShowPassword((prev) => !prev)}
                          disabled={isLoading} // Chặn toggle pass khi đang load
                        >
                          {showPassword ? (
                            <EyeOff className="h-4 w-4 text-gray-500" />
                          ) : (
                            <Eye className="h-4 w-4 text-gray-500" />
                          )}
                        </Button>
                      </div>
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              {/* Hiển thị lỗi chung (Root Error) đẹp hơn một chút */}
              {form.formState.errors.root && (
                <div className="p-3 rounded-md bg-destructive/15 text-destructive text-sm font-medium flex items-center gap-2">
                  {/* Bạn có thể thêm icon AlertTriangle ở đây nếu muốn */}
                  {form.formState.errors.root.message}
                </div>
              )}

              {/* --- PHẦN 2: VIẾT LẠI BUTTON LOADING --- */}
              <Button type="submit" className="w-full" disabled={isLoading}>
                {isLoading ? (
                  <div className="flex items-center justify-center gap-2">
                    <Loader2 className="h-4 w-4 animate-spin" />
                    <span>Đang đăng nhập...</span>
                  </div>
                ) : (
                  'Đăng nhập'
                )}
              </Button>

            </form>
          </Form>
        </CardContent>
      </Card>
    </div>
  );
};

export default LoginPage;