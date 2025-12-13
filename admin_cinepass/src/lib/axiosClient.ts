// src/lib/axiosClient.ts
import axios from 'axios';

// 1. Tạo instance với config mặc định
const axiosClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 10000, // Timeout sau 10s
});

// 2. REQUEST INTERCEPTOR: Tự động gắn Token trước khi gửi đi
axiosClient.interceptors.request.use(
  (config) => {
    // Lấy token từ LocalStorage
    const token = localStorage.getItem('accessToken');
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// 3. RESPONSE INTERCEPTOR: Xử lý phản hồi và lỗi chung
axiosClient.interceptors.response.use(
  (response) => {
    // Trả về data trực tiếp (bỏ qua wrapper của axios là response.data)
    // Tùy vào format backend của bạn, có thể return response.data
    return response.data;
  },
  (error) => {
    // Xử lý các lỗi global (Ví dụ: 401 - Hết hạn token)
    if (error.response && error.response.status === 401) {
      // Xóa token và redirect về login
      localStorage.removeItem('accessToken');
      localStorage.removeItem('user');
      window.location.href = '/login';
    }
    
    // Ném lỗi ra để component tự xử lý tiếp (hiện thông báo...)
    return Promise.reject(error);
  }
);

export default axiosClient;