import { lazy, Suspense, type ComponentType, type ReactNode } from 'react';
import { createBrowserRouter, Navigate, type RouteObject } from 'react-router-dom';
import { PATHS } from '../config/paths';

// Layouts
import MainLayout from '@/layouts/MainLayout';
import AuthLayout from '@/layouts/AuthLayout';
import { Spinner } from '@/components/ui/spinner';
import { ProtectedRoute } from '@/components/common/ProtectedRoute';

// Auth Pages
import ForgotPasswordPage from '@/features/auth/ForgotPassword';
import ResetPasswordPage from '@/features/auth/ResetPassword';
import DashboardPage from '@/features/dashboard';
import MovieListPage from '@/features/movies';
import CreateMoviePage from '@/features/movies/CreateMovie';
import EditMoviePage from '@/features/movies/EditMovie';
import MovieDetailPage from '@/features/movies/MovieDetail';

// Cinema Pages
import CinemasListPage from '@/features/cinemas';
import CreateCinemaPage from '@/features/cinemas/CreateCinema';
import EditCinemaPage from '@/features/cinemas/EditCinema';
import CinemaDetailPage from '@/features/cinemas/CinemaDetail';
import ScreenSeatsPage from '@/features/cinemas/ScreenSeats';

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
  // 1. PUBLIC ROUTES (Auth pages - không cần đăng nhập)
  {
    path: '/',
    element: <AuthLayout />,
    children: [
      { path: PATHS.LOGIN, element: Loadable(LoginPage) },
      { path: PATHS.FORGOT_PASSWORD, element: Loadable(ForgotPasswordPage) },
      { path: PATHS.RESET_PASSWORD, element: Loadable(ResetPasswordPage) },
    ]   
  },

  // 2. PROTECTED ROUTES (Cần đăng nhập)
  {
    path: '/',
    element: (
      <ProtectedRoute>
        <MainLayout />
      </ProtectedRoute>
    ), 
    children: [
      { 
        index: true, 
        element: <Navigate to={PATHS.DASHBOARD} replace /> 
      },
      { path: PATHS.DASHBOARD, element: Loadable(DashboardPage) },
      { path: PATHS.MOVIES, element: Loadable(MovieListPage) },
      { path: PATHS.MOVIE_CREATE, element: Loadable(CreateMoviePage) },
      { path: PATHS.MOVIE_EDIT, element: Loadable(EditMoviePage) },
      { path: PATHS.MOVIE_DETAIL, element: Loadable(MovieDetailPage) },
      { path: PATHS.CINEMAS, element: Loadable(CinemasListPage) },
      { path: PATHS.CINEMA_CREATE, element: Loadable(CreateCinemaPage) },
      { path: PATHS.CINEMA_DETAIL, element: Loadable(CinemaDetailPage) },
      { path: '/cinemas/edit/:id', element: Loadable(EditCinemaPage) },
      { path: '/cinemas/:cinemaId/screens/:roomId/seats', element: Loadable(ScreenSeatsPage) },
    ]
  },

  // 3. CATCH ALL (404)
  { path: '*', element: <div>404 Not Found</div> }
] as RouteObject[]); // Ép kiểu mảng này là RouteObject[] để TypeScript kiểm tra lỗi