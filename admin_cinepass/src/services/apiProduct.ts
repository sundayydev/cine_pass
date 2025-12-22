import axiosClient from '@/lib/axiosClient';

// ==================== TYPES ====================

/**
 * Generic API Response wrapper
 */
export interface ApiResponseDto<T> {
  success: boolean;
  message?: string;
  data?: T;
  errors?: string[];
}

/**
 * DTO cho tạo sản phẩm mới
 */
export interface ProductCreateDto {
  name: string;
  description?: string;
  price: number;
  imageUrl?: string;
  category: string; // ProductCategory enum: "Food", "Drink", "Combo"
  isActive?: boolean;
}

/**
 * DTO cho cập nhật sản phẩm
 */
export interface ProductUpdateDto {
  name?: string;
  description?: string;
  price?: number;
  imageUrl?: string;
  category?: string; // ProductCategory enum: "Food", "Drink", "Combo"
  isActive?: boolean;
}

/**
 * Response DTO cho sản phẩm
 */
export interface ProductResponseDto {
  id: string;
  name: string;
  description?: string;
  price: number;
  imageUrl?: string;
  category: string; //
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

// ==================== HELPERS ====================

/**
 * Chuyển category string sang number cho backend
 */
const categoryToNumber = (category: string): number => {
  switch (category) {
    case 'Food': return 0;
    case 'Drink': return 1;
    case 'Combo': return 2;
    default: return 0;
  }
};

// ==================== API FUNCTIONS ====================

/**
 * Lấy danh sách tất cả sản phẩm
 * GET /api/products
 */
export const getAll = async (): Promise<ProductResponseDto[]> => {
  const response = (await axiosClient.get(
    '/api/products'
  )) as ApiResponseDto<ProductResponseDto[]>;

  if (!response.success || !response.data) {
    throw new Error(response.message || 'Lỗi khi lấy danh sách sản phẩm');
  }

  return response.data;
};

/**
 * Lấy danh sách sản phẩm đang hoạt động
 * GET /api/products/active
 */
export const getActive = async (): Promise<ProductResponseDto[]> => {
  const response = (await axiosClient.get(
    '/api/products/active'
  )) as ApiResponseDto<ProductResponseDto[]>;

  if (!response.success || !response.data) {
    throw new Error(response.message || 'Lỗi khi lấy danh sách sản phẩm');
  }

  return response.data;
};

/**
 * Lấy danh sách sản phẩm theo danh mục
 * GET /api/products/category/{category}
 */
export const getByCategory = async (category: string): Promise<ProductResponseDto[]> => {
  const response = (await axiosClient.get(
    `/api/products/category/${category}`
  )) as ApiResponseDto<ProductResponseDto[]>;

  if (!response.success || !response.data) {
    throw new Error(response.message || 'Lỗi khi lấy danh sách sản phẩm');
  }

  return response.data;
};

/**
 * Tìm kiếm sản phẩm
 * GET /api/products/search?searchTerm={searchTerm}
 */
export const search = async (searchTerm: string): Promise<ProductResponseDto[]> => {
  if (!searchTerm || searchTerm.trim() === '') {
    throw new Error('Từ khóa tìm kiếm không được để trống');
  }

  const response = (await axiosClient.get(
    '/api/products/search',
    {
      params: { searchTerm },
    }
  )) as ApiResponseDto<ProductResponseDto[]>;

  if (!response.success || !response.data) {
    throw new Error(response.message || 'Lỗi khi tìm kiếm sản phẩm');
  }

  return response.data;
};

/**
 * Lấy thông tin sản phẩm theo ID
 * GET /api/products/{id}
 */
export const getById = async (id: string): Promise<ProductResponseDto> => {
  const response = (await axiosClient.get(
    `/api/products/${id}`
  )) as ApiResponseDto<ProductResponseDto>;

  if (!response.success || !response.data) {
    throw new Error(response.message || `Không tìm thấy sản phẩm có ID ${id}`);
  }

  return response.data;
};

/**
 * Tạo sản phẩm mới
 * POST /api/products
 */
export const create = async (dto: ProductCreateDto): Promise<ProductResponseDto> => {
  // Chuyển category từ string sang number cho backend
  const requestBody = {
    ...dto,
    category: categoryToNumber(dto.category)
  };

  const response = (await axiosClient.post(
    '/api/products',
    requestBody
  )) as ApiResponseDto<ProductResponseDto>;

  if (!response.success || !response.data) {
    throw new Error(response.message || 'Lỗi khi tạo sản phẩm');
  }

  return response.data;
};

/**
 * Cập nhật thông tin sản phẩm
 * PUT /api/products/{id}
 */
export const update = async (id: string, dto: ProductUpdateDto): Promise<ProductResponseDto> => {
  // Chuyển category từ string sang number nếu có
  const requestBody = {
    ...dto,
    category: dto.category ? categoryToNumber(dto.category) : undefined
  };

  const response = (await axiosClient.put(
    `/api/products/${id}`,
    requestBody
  )) as ApiResponseDto<ProductResponseDto>;

  if (!response.success || !response.data) {
    throw new Error(response.message || `Không tìm thấy sản phẩm có ID ${id}`);
  }

  return response.data;
};

/**
 * Xóa sản phẩm
 * DELETE /api/products/{id}
 */
export const deleteProduct = async (id: string): Promise<void> => {
  const response = (await axiosClient.delete(
    `/api/products/${id}`
  )) as ApiResponseDto<unknown>;

  if (response && typeof response === 'object' && 'success' in response) {
    const errorResponse = response as ApiResponseDto<unknown>;
    if (!errorResponse.success) {
      throw new Error(errorResponse.message || `Không tìm thấy sản phẩm có ID ${id}`);
    }
  }
};

// ==================== EXPORT OBJECT ====================

/**
 * Object chứa tất cả các API functions cho Product
 */
export const productApi = {
  getAll,
  getActive,
  getByCategory,
  search,
  getById,
  create,
  update,
  delete: deleteProduct,
};

