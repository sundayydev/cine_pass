import { useEffect, type ReactNode } from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '@/context/AuthContext';
import { PATHS } from '@/config/paths';
import { Spinner } from '@/components/ui/spinner';

interface ProtectedRouteProps {
  children: ReactNode;
}

/**
 * Component bảo vệ route - chỉ cho phép truy cập khi đã đăng nhập
 * Nếu chưa đăng nhập, tự động redirect về trang login
 */
export const ProtectedRoute = ({ children }: ProtectedRouteProps) => {
  const { isAuthenticated, isLoading, checkAuth } = useAuth();
  const location = useLocation();

  useEffect(() => {
    // Kiểm tra lại authentication mỗi khi route thay đổi
    checkAuth();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [location.pathname]);

  // Hiển thị loading khi đang kiểm tra authentication
  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <Spinner />
      </div>
    );
  }

  // Nếu chưa đăng nhập, redirect về trang login
  // Lưu location hiện tại để redirect lại sau khi login
  if (!isAuthenticated) {
    return <Navigate to={PATHS.LOGIN} state={{ from: location }} replace />;
  }

  // Nếu đã đăng nhập, render children
  return <>{children}</>;
};

