// Types for Showtime UI Components

export interface ShowtimeWithDetails {
  id: string;
  movieId: string;
  screenId: string;
  startTime: string;
  endTime: string;
  basePrice: number;
  isActive: boolean;
  movieTitle?: string;
  screenName?: string;
  cinemaName?: string;
  movie?: {
    id: string;
    title: string;
    durationMinutes: number;
    category: string;
    status: string;
  };
  screen?: {
    id: string;
    name: string;
    cinemaId: string;
    totalSeats: number;
  };
}

export interface Cinema {
  id: string;
  name: string;
  address?: string;
  phone?: string;
  email?: string;
  isActive?: boolean;
  createdAt?: string;
  updatedAt?: string;
}

export interface ShowtimeFilters {
  search?: string;
  status?: 'all' | 'active' | 'inactive';
  movieId?: string;
  cinemaId?: string;
  screenId?: string;
  dateFrom?: Date;
  dateTo?: Date;
}

export interface ShowtimeFormData {
  movieId: string;
  cinemaId: string;
  screenId: string;
  startDate: Date;
  startTime: string;
  durationMinutes: number;
  basePrice: number;
  isActive?: boolean;
}

export interface ShowtimeListResponse {
  data: ShowtimeWithDetails[];
  pagination: {
    page: number;
    limit: number;
    totalRows: number;
    totalPages: number;
  };
}