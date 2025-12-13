// 1. Type cho các tham số phân trang & lọc
export interface PaginationParams {
    page?: number;
    limit?: number;
    search?: string;
    sortBy?: string;
    order?: 'asc' | 'desc';
  }
  
  // 2. Type chung cho phản hồi dạng danh sách (List) từ Backend
  // Tùy backend của bạn trả về format nào, hãy sửa lại cho khớp
  export interface ListResponse<T> {
    data: T[];
    pagination: {
      page: number;
      limit: number;
      totalRows: number;
      totalPages: number;
    };
  }
  
  // 3. Type chung cho phản hồi thành công đơn giản
  export interface SuccessResponse {
    success: boolean;
    message: string;
  }