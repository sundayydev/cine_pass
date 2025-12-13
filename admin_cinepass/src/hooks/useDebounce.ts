import { useEffect, useState } from 'react';

/**
 * Custom hook để trì hoãn việc cập nhật giá trị (Debounce)
 * * @param value Giá trị cần debounce (thường là state từ input)
 * @param delay Thời gian chờ (mặc định 500ms)
 * @returns Giá trị đã được debounce
 */
export function useDebounce<T>(value: T, delay: number = 500): T {
  // 1. State lưu trữ giá trị đã debounce
  const [debouncedValue, setDebouncedValue] = useState<T>(value);

  useEffect(() => {
    // 2. Tạo timer để cập nhật giá trị sau khoảng delay
    const timer = setTimeout(() => {
      setDebouncedValue(value);
    }, delay);

    // 3. Cleanup function: 
    // Nếu 'value' hoặc 'delay' thay đổi trước khi hết thời gian chờ,
    // timer cũ sẽ bị xóa và timer mới được tạo.
    // Đây chính là cơ chế giúp ngăn chặn gọi API liên tục.
    return () => {
      clearTimeout(timer);
    };
  }, [value, delay]);

  return debouncedValue;
}