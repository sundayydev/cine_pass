// src/layouts/AuthLayout.tsx
import React from 'react';
import { Outlet } from 'react-router-dom';

// 1. Dữ liệu mẫu Poster Phim (Bạn nên thay bằng ảnh thật của dự án)
// Sử dụng ảnh có tỉ lệ dọc (portrait) ~ 2:3
const SAMPLE_POSTERS = [
    "https://vov2.vov.vn/sites/default/files/styles/large/public/2022-06/image007.jpg", //Minions: Sự trổi dậy của Gru
    "https://cdn2.fptshop.com.vn/unsafe/Uploads/images/tin-tuc/176627/Originals/poster-phim-hoat-hinh-4.jpg", // Kungfu Panda 4
    "https://cdn2.fptshop.com.vn/unsafe/Uploads/images/tin-tuc/176627/Originals/poster-phim-hoat-hinh-5.jpg", // Cuộc chiến bất tử
    "https://cdn2.fptshop.com.vn/unsafe/Uploads/images/tin-tuc/176627/Originals/poster-phim-hoat-hinh-2.jpg", // Nobita và vùng đất lý tưởng trên bầu trời
    "https://cdn2.fptshop.com.vn/unsafe/Uploads/images/tin-tuc/176627/Originals/poster-phim-hoat-hinh-3.jpg", //Gấu Đỏ Biến Hình (Turning Red)
    "https://iguov8nhvyobj.vcdn.cloud/media/catalog/product/cache/1/image/1800x/71252117777b696995f01934522c402d/t/e/teaser2_poster_vietnam.jpg", // Zootopia 2
];

const MarqueeColumn = ({ images, reverse = false }: { images: string[], reverse?: boolean }) => {
    const duplicatedImages = [...images, ...images];
  
    return (
      <div className="relative h-full overflow-hidden w-1/2 px-2">
        <div className={`flex flex-col gap-4 ${reverse ? 'animate-marquee-vertical-reverse' : 'animate-marquee-vertical'}`}>
          {duplicatedImages.map((src, index) => (
            <img
              key={index}
              src={src}
              alt="Movie Poster"
              // THAY ĐỔI 1: Bỏ grayscale, tăng độ sáng, thêm shadow màu
              className="w-full h-auto rounded-xl object-cover shadow-lg shadow-indigo-500/20 opacity-100 transition-transform duration-500"
            />
          ))}
        </div>
      </div>
    );
  };
  
  const AuthLayout = () => {
    return (
      <div className="w-full min-h-screen lg:grid lg:grid-cols-5">
        
        {/* Cột Trái: Thay nền đen bằng nền màu tím than đậm (fallback) */}
        <div className="hidden lg:block relative col-span-3 h-full overflow-hidden bg-slate-900">
          
          {/* --- LỚP 1: Nền Poster chuyển động --- */}
          <div className="absolute inset-0 flex justify-center gap-4 p-4 rotate-6 scale-125 opacity-50">
             {/* Giảm opacity của chính ảnh xuống để lớp màu phủ lên đẹp hơn */}
            <MarqueeColumn images={SAMPLE_POSTERS.slice(0, 3)} />
            <MarqueeColumn images={SAMPLE_POSTERS.slice(3, 6)} reverse />
          </div>
  
        {/* ... (Phần Poster chuyển động ở trên giữ nguyên) ... */}

        {/* --- LỚP 2: Overlay Màu Xám Trung Tính (Neutral Gray) --- */}
        {/* Sử dụng Zinc hoặc Slate để có màu xám hiện đại, không bị "đen sì" */}
        <div className="absolute inset-0 bg-zinc-800/5" /> 
        
        {/* Thêm Gradient từ Xám Nhẹ -> Xám Đậm để tạo chiều sâu */}
        <div className="absolute inset-0 bg-gradient-to-br from-gray-800/10 via-zinc-900/10 to-black/10" />
  
  
          {/* --- LỚP 3: Nội dung chữ --- */}
          <div className="relative z-20 flex flex-col justify-between h-full p-12 text-white">
            {/* Logo Branding */}
            <div className="flex items-center space-x-2">
              <div className="p-2 bg-white/10 backdrop-blur-md rounded-lg border border-white/20">
                  <svg
                  xmlns="http://www.w3.org/2000/svg"
                  viewBox="0 0 24 24"
                  fill="none"
                  stroke="currentColor"
                  strokeWidth="2"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  className="h-6 w-6 text-white"
                  >
                  <path d="M15 6v12a3 3 0 1 0 3-3H6a3 3 0 1 0 3 3V6a3 3 0 1 0-3 3h12a3 3 0 1 0-3-3" />
                  </svg>
              </div>
              <span className="font-bold text-2xl tracking-tight">CinePass Admin</span>
            </div>
            
            {/* Quote & Footer */}
            <div className="relative mt-auto max-w-lg">
              <blockquote className="space-y-4">
                <p className="text-2xl font-medium leading-relaxed tracking-wide text-white drop-shadow-md">
                  &ldquo;Đưa trải nghiệm điện ảnh lên tầm cao mới với hệ thống quản lý toàn diện & thông minh.&rdquo;
                </p>
                <footer className="flex items-center gap-4 text-sm font-medium text-indigo-100">
                  <span className="uppercase tracking-widest font-medium">Copyright © {new Date().getFullYear()} Sunday Dev</span>
                  <span>Release</span>
                </footer>
              </blockquote>
            </div>
          </div>
        </div>
  
        {/* Cột Phải: Form Content */}
        <div className="flex col-span-2 items-center justify-center py-12 bg-background">
          <div className="mx-auto grid w-[350px] gap-6 z-30">
            <Outlet />
          </div>
        </div>
      </div>
    );
  };

export default AuthLayout;