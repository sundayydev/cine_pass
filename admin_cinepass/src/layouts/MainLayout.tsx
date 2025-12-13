import { useState } from 'react';
import { Outlet, NavLink } from 'react-router-dom';
import { 
  LayoutDashboard, 
  Film, 
  Tv, 
  CalendarDays, 
  Ticket, 
  Users, 
  Menu, 
  LogOut,
  Settings
} from 'lucide-react';

// Shadcn Components
import { Button } from '@/components/ui/button';
import { Sheet, SheetContent, SheetTrigger } from '@/components/ui/sheet';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Toaster } from "@/components/ui/sonner"

import { PATHS } from '@/config/paths';
import { useAuth } from '@/context/AuthContext';

// 1. Cấu hình Menu Items
const NAV_ITEMS = [
  { label: 'Dashboard', icon: LayoutDashboard, path: PATHS.DASHBOARD },
  { label: 'Phim', icon: Film, path: PATHS.MOVIES },
  { label: 'Rạp & Phòng', icon: Tv, path: PATHS.CINEMAS }, // Icon Tv đại diện cho màn hình chiếu
  { label: 'Lịch Chiếu', icon: CalendarDays, path: PATHS.SHOWTIMES },
  { label: 'Vé Đặt', icon: Ticket, path: PATHS.BOOKINGS },
  { label: 'Khách hàng', icon: Users, path: PATHS.USERS }, // Giả sử bạn có path này
];

const MainLayout = () => {
  const { logout } = useAuth();
  // State để đóng mở Sheet trên mobile (nếu cần control thủ công)
  const [isMobileOpen, setIsMobileOpen] = useState(false);

  const handleLogout = async () => {
    try {
      await logout();
      // AuthContext sẽ tự động navigate về login và clear state
    } catch (error) {
      console.error('Logout error:', error);
      // Ngay cả khi có lỗi, vẫn redirect về login
    }
  };

  // 2. Component Sidebar nội bộ (dùng chung cho Desktop và Mobile)
  const SidebarContent = () => (
    <div className="flex h-full flex-col gap-2">
      <div className="flex h-14 items-center border-b px-4 lg:h-[60px] lg:px-6">
        <a href="/" className="flex items-center gap-2 font-semibold">
          <Film className="h-6 w-6" />
          <span className="">CinePass Admin</span>
        </a>
      </div>
      <div className="flex-1 overflow-auto py-2">
        <nav className="grid items-start px-2 text-sm font-medium lg:px-4 gap-1">
          {NAV_ITEMS.map((item) => (
            <NavLink
              key={item.path}
              to={item.path}
              onClick={() => setIsMobileOpen(false)} // Đóng menu mobile khi click
              className={({ isActive }) =>
                `flex items-center gap-3 rounded-lg px-3 py-2 transition-all hover:text-primary ${
                  isActive
                    ? 'bg-muted text-primary'
                    : 'text-muted-foreground'
                }`
              }
            >
              <item.icon className="h-4 w-4" />
              {item.label}
            </NavLink>
          ))}
        </nav>
      </div>
    </div>
  );

  return (
    <div className="grid min-h-screen w-full md:grid-cols-[220px_1fr] lg:grid-cols-[280px_1fr]">
      
      {/* 3. Sidebar cho Desktop (Ẩn trên mobile) */}
      <div className="hidden border-r bg-muted/40 md:block">
        <SidebarContent />
      </div>

      {/* 4. Khu vực chính bên phải */}
      <div className="flex flex-col">
        {/* Header */}
        <header className="flex h-14 items-center gap-4 border-b bg-muted/40 px-4 lg:h-[60px] lg:px-6">
          
          {/* Nút Menu Mobile (Hiện trên mobile) */}
          <Sheet open={isMobileOpen} onOpenChange={setIsMobileOpen}>
            <SheetTrigger asChild>
              <Button
                variant="outline"
                size="icon"
                className="shrink-0 md:hidden"
              >
                <Menu className="h-5 w-5" />
                <span className="sr-only">Toggle navigation</span>
              </Button>
            </SheetTrigger>
            <SheetContent side="left" className="flex flex-col p-0 w-[240px]">
               <SidebarContent />
            </SheetContent>
          </Sheet>

          {/* Khoảng trống hoặc Breadcrumb */}
          <div className="w-full flex-1">
            {/* Có thể đặt Breadcrumb hoặc Search Input ở đây */}
          </div>

          {/* User Profile Dropdown */}
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="secondary" size="icon" className="rounded-full">
                <Avatar>
                <AvatarImage src="https://api.dicebear.com/7.x/avataaars/svg?seed=Admin" alt="Admin Avatar" />
                    <AvatarFallback>AD</AvatarFallback>
                </Avatar>
                <span className="sr-only">Toggle user menu</span>
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuLabel>Tài khoản</DropdownMenuLabel>
              <DropdownMenuSeparator />
              <DropdownMenuItem onClick={() => navigate('/settings')}>
                <Settings className="mr-2 h-4 w-4" /> Cài đặt
              </DropdownMenuItem>
              <DropdownMenuItem>Hỗ trợ</DropdownMenuItem>
              <DropdownMenuSeparator />
              <DropdownMenuItem onClick={handleLogout} className="text-red-600 focus:text-red-600">
                 <LogOut className="mr-2 h-4 w-4" /> Đăng xuất
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        </header>

        {/* Nội dung chính (Outlet) */}
        <main className="flex flex-1 flex-col gap-4 p-4 lg:gap-6 lg:p-6 overflow-auto bg-background">
          <Outlet />
        </main>
        <Toaster />
      </div>
    </div>
  );
};

export default MainLayout;