import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { Link, useNavigate } from 'react-router-dom';
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

// Services
import { login } from '@/services/apiAuth';

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
  // State để quản lý việc ẩn/hiện mật khẩu
  const [showPassword, setShowPassword] = useState(false);
  // State để hiển thị loading khi đang gọi API
  const [isLoading, setIsLoading] = useState(false);

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
    setIsLoading(true);
    try {
      // Gọi API đăng nhập
      await login({
        email: data.email,
        password: data.password,
      });
      
      // Nếu login thành công, chuyển hướng về trang Dashboard
      navigate(PATHS.DASHBOARD);
      
    } catch (error) {
      // Xử lý lỗi
      console.error("Lỗi đăng nhập:", error);
      const errorMessage = error instanceof Error 
        ? error.message 
        : "Đăng nhập thất bại. Vui lòng kiểm tra lại thông tin.";
      
      // Hiển thị lỗi trong form
      form.setError('root', {
        type: 'manual',
        message: errorMessage,
      });
      
      // TODO: Có thể thêm toast notification ở đây
      // toast.error(errorMessage);
    } finally {
      setIsLoading(false); // Tắt trạng thái loading
    }
  };

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
          {/* Component Form của Shadcn bọc quanh thẻ form html */}
          <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
              
              {/* --- Trường Email --- */}
              <FormField
                control={form.control}
                name="email"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Email</FormLabel>
                    <FormControl>
                      <Input type="email" placeholder="admin@cinepro.com" {...field} disabled={isLoading} />
                    </FormControl>
                    <FormMessage /> {/* Hiển thị lỗi validate nếu có */}
                  </FormItem>
                )}
              />

              {/* --- Trường Password --- */}
              <FormField
                control={form.control}
                name="password"
                render={({ field }) => (
                  <FormItem>
                    <div className="flex items-center justify-between">
                        <FormLabel>Mật khẩu</FormLabel>
                        <Link 
                            to={PATHS.FORGOT_PASSWORD} 
                            className="text-sm font-medium text-primary hover:underline"
                        >
                            Quên mật khẩu?
                        </Link>
                    </div>
                    <FormControl>
                      <div className="relative">
                        <Input
                          type={showPassword ? 'text' : 'password'}
                          placeholder="••••••••"
                          {...field}
                          disabled={isLoading}
                        />
                        {/* Nút toggle ẩn/hiện mật khẩu */}
                        <Button
                          type="button"
                          variant="ghost"
                          size="sm"
                          className="absolute right-0 top-0 h-full px-3 py-2 hover:bg-transparent"
                          onClick={() => setShowPassword((prev) => !prev)}
                          disabled={isLoading}
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

              {/* --- Hiển thị lỗi từ API --- */}
              {form.formState.errors.root && (
                <div className="text-sm text-destructive">
                  {form.formState.errors.root.message}
                </div>
              )}

              {/* --- Nút Submit --- */}
              <Button type="submit" className="w-full" disabled={isLoading}>
                {isLoading ? (
                    <>
                        <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                        Đang xử lý...
                    </>
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