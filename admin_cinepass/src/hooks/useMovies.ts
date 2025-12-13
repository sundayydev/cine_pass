import { 
    useQuery, 
    useMutation, 
    useQueryClient, 
    keepPreviousData 
  } from '@tanstack/react-query';
  import { getAll, getById, create, update, deleteMovie, search, type MoviePayload } from '@/services/apiMovie';
  import type { PaginationParams } from '@/types/apiType';
  import type { Movie } from '@/types/moveType';
  
  // --- 1. Hook lấy danh sách phim (QUERY) ---
  // getAll() trong apiMovie.ts đã unwrap và trả về Movie[] trực tiếp
  export const useMovies = (params: PaginationParams) => {
    return useQuery<Movie[]>({
      // queryKey: Thêm params vào để tự fetch lại khi search/đổi trang
      queryKey: ['movies', params], 
      
      // queryFn: Sử dụng search endpoint nếu có searchTerm,否则 dùng getAll
      queryFn: async () => {
        if (params.search && params.search.trim() !== '') {
          return await search(params.search);
        }
        return await getAll();
      },
      
      // Giữ data cũ khi đang loading trang mới (tránh nhấp nháy bảng)
      placeholderData: keepPreviousData,
      
      // Data được coi là mới trong 5s
      staleTime: 5000, 
    });
  };
  
  // --- 2. Hook lấy chi tiết 1 phim (QUERY) ---
  export const useMovie = (id: string) => {
    return useQuery<Movie>({
      queryKey: ['movie', id],
      queryFn: async () => {
        // getById đã xử lý và trả về object Movie trực tiếp
        return await getById(id);
      },
      enabled: !!id, // Chỉ chạy khi có ID
    });
  };
  
  // --- 3. Hook Thêm mới phim (MUTATION) ---
  export const useCreateMovie = () => {
    const queryClient = useQueryClient();
  
    return useMutation({
      mutationFn: (newMovie: MoviePayload) => create(newMovie),
      onSuccess: () => {
        // Refresh lại danh sách phim để hiện phim mới
        queryClient.invalidateQueries({ queryKey: ['movies'] });
      },
    });
  };
  
  // --- 4. Hook Cập nhật phim (MUTATION) ---
  export const useUpdateMovie = () => {
    const queryClient = useQueryClient();
  
    return useMutation({
      mutationFn: ({ id, data }: { id: string; data: Partial<MoviePayload> }) => 
        update(id, data),
      onSuccess: (_response, variables) => {
        // 1. Refresh danh sách
        queryClient.invalidateQueries({ queryKey: ['movies'] });
        // 2. Refresh chi tiết phim vừa sửa (dùng variables.id để lấy ID chính xác)
        queryClient.invalidateQueries({ queryKey: ['movie', variables.id] });
      },
    });
  };
  
  // --- 5. Hook Xóa phim (MUTATION) ---
  export const useDeleteMovie = () => {
    const queryClient = useQueryClient();
  
    return useMutation({
      mutationFn: (id: string) => deleteMovie(id),
      onSuccess: () => {
        // Xóa xong reload lại bảng
        queryClient.invalidateQueries({ queryKey: ['movies'] });
      },
    });
  };