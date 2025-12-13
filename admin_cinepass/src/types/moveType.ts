// 1. Định nghĩa Enum cho Status
// Bạn cần check lại bên BE xem Enum MovieStatus trả về số (0,1,2) hay chuỗi ("NOW_SHOWING")
// Ở đây mình giả định BE trả về chuỗi (String Enum) cho dễ đọc
export type MovieStatus = 'COMING_SOON' | 'NOW_SHOWING' | 'ENDED' | 'CANCELLED';

// HOẶC nếu BE trả về số (Int Enum):
// export enum MovieStatus {
//   ComingSoon = 0,
//   NowShowing = 1,
//   Ended = 2
// }

// 2. Interface chính hiển thị dữ liệu (Response từ API)
export interface Movie {
  id: string;                    // Guid
  title: string;
  slug?: string;                 // Nullable
  durationMinutes: number;
  description?: string;          // Nullable
  posterUrl?: string;            // Nullable (URL ảnh)
  trailerUrl?: string;           // Nullable
  releaseDate?: string;          // DateTime -> ISO Date String (VD: "2025-10-20T00:00:00Z")
  status: MovieStatus;
  createdAt: string;
  
  // Navigation properties (Tùy chọn, chỉ có khi BE include vào)
  // showtimes?: Showtime[]; 
}

// 3. Interface dùng cho Form Thêm mới / Cập nhật (Request Payload)
// Khi gửi lên server, chúng ta thường gửi File ảnh thay vì URL string
export interface MoviePayload {
  title: string;
  durationMinutes: number;
  description?: string;
  releaseDate?: string | Date;   // Form có thể lưu dạng Date object
  trailerUrl?: string;
  status: MovieStatus;
  
  // Xử lý upload ảnh:
  // Khi gửi lên: dùng File
  // Khi edit nhưng không đổi ảnh: không gửi trường này
  posterFile?: File; 
}

// 4. Type cho bộ lọc (Filter)
export interface MovieFilters {
  page?: number;
  limit?: number;
  search?: string;
  status?: MovieStatus | 'all';
}