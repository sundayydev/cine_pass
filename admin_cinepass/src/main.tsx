import React from 'react';
import ReactDOM from 'react-dom/client';
import App from './App';
import './index.css';

// 1. Import QueryClient và Provider
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';

// (Tùy chọn) Import Devtools để soi data API rất tiện khi debug
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';

// 2. Tạo một instance của QueryClient
// Bạn có thể cấu hình defaultOptions ở đây nếu muốn
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false, // Tắt tính năng tự fetch lại khi click sang tab khác rồi quay lại
      retry: 1, // Số lần thử lại nếu API lỗi
    },
  },
});

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    {/* 3. Bọc toàn bộ App trong QueryClientProvider */}
    <QueryClientProvider client={queryClient}>
      <App />
      
      {/* (Tùy chọn) Nút logo React Query ở góc màn hình để debug */}
      <ReactQueryDevtools initialIsOpen={false} />
    </QueryClientProvider>
  </React.StrictMode>,
);