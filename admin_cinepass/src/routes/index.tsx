import { lazy, Suspense, type ComponentType, type ReactNode } from 'react';
import { createBrowserRouter, Navigate, type RouteObject } from 'react-router-dom';
import { PATHS } from '../config/paths';

// Layouts
import MainLayout from '@/layouts/MainLayout';
import AuthLayout from '@/layouts/AuthLayout';
import { Spinner } from '@/components/ui/spinner';

// Auth Pages
import ForgotPasswordPage from '@/features/auth/ForgotPassword';
import ResetPasswordPage from '@/features/auth/ResetPassword';
import DashboardPage from '@/features/dashboard';
import MovieListPage from '@/features/movies';

// --- Lazy Load Pages ---
// Auth
const LoginPage = lazy(() => import('@/features/auth/Login'));

// --- Loadable Component ---
const Loadable = (Component: ComponentType<any>): ReactNode => (
  <Suspense fallback={<Spinner />}>
    <Component />
  </Suspense>
);

// --- Router Config ---
export const router = createBrowserRouter([
  // 1. PUBLIC ROUTES
  {
    path: '/',
    element: <AuthLayout />,
    children: [
      { path: PATHS.LOGIN, element: Loadable(LoginPage) },
      { path: PATHS.FORGOT_PASSWORD, element: Loadable(ForgotPasswordPage) },
      { path: PATHS.RESET_PASSWORD, element: Loadable(ResetPasswordPage) },
    ]   
  },

  // 2. PROTECTED ROUTES
  {
    path: '/',
    element: <MainLayout />, 
    children: [
      { 
        index: true, 
        element: <Navigate to={PATHS.DASHBOARD} replace /> 
      },
      { path: PATHS.DASHBOARD, element: Loadable(DashboardPage) },
      { path: PATHS.MOVIES, element: Loadable(MovieListPage) },
    ]
  },

  // 3. CATCH ALL (404)
  { path: '*', element: <div>404 Not Found</div> }
] as RouteObject[]); // Ép kiểu mảng này là RouteObject[] để TypeScript kiểm tra lỗi