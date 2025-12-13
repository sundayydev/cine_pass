import { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { login as apiLogin, logout as apiLogout, getAccessToken, getRefreshToken, isAccessTokenExpired } from '@/services/apiAuth';

// ==================== TYPES ====================

interface AuthContextType {
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<void>;
  logout: () => Promise<void>;
  checkAuth: () => boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

// ==================== PROVIDER ====================

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider = ({ children }: AuthProviderProps) => {
  const [isAuthenticated, setIsAuthenticated] = useState<boolean>(false);
  const [isLoading, setIsLoading] = useState<boolean>(true);

  // Kiểm tra authentication khi component mount
  useEffect(() => {
    checkAuthStatus();
  }, []);

  /**
   * Kiểm tra trạng thái authentication
   */
  const checkAuthStatus = () => {
    setIsLoading(true);
    try {
      const token = getAccessToken();
      const isExpired = isAccessTokenExpired();
      
      if (token && !isExpired) {
        setIsAuthenticated(true);
      } else {
        setIsAuthenticated(false);
        // Xóa token hết hạn
        if (token) {
          localStorage.removeItem('accessToken');
          localStorage.removeItem('refreshToken');
          localStorage.removeItem('accessTokenExpiresAt');
          localStorage.removeItem('refreshTokenExpiresAt');
        }
      }
    } catch (error) {
      console.error('Error checking auth status:', error);
      setIsAuthenticated(false);
    } finally {
      setIsLoading(false);
    }
  };

  /**
   * Đăng nhập
   */
  const login = async (email: string, password: string): Promise<void> => {
    try {
      setIsLoading(true);
      await apiLogin({ email, password });
      setIsAuthenticated(true);
      // Component sẽ tự navigate sau khi login thành công
    } catch (error) {
      setIsAuthenticated(false);
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  /**
   * Đăng xuất
   */
  const logout = async (): Promise<void> => {
    try {
      setIsLoading(true);
      const refreshToken = getRefreshToken();
      
      if (refreshToken) {
        try {
          await apiLogout(refreshToken);
        } catch (error) {
          // Nếu logout API fail, vẫn tiếp tục clear local state
          console.error('Logout API error:', error);
        }
      }
      
      // Xóa tất cả tokens và user data
      localStorage.removeItem('accessToken');
      localStorage.removeItem('refreshToken');
      localStorage.removeItem('accessTokenExpiresAt');
      localStorage.removeItem('refreshTokenExpiresAt');
      localStorage.removeItem('user');
      
      setIsAuthenticated(false);
      // Sử dụng window.location để đảm bảo redirect hoạt động
      window.location.href = '/login';
    } catch (error) {
      console.error('Logout error:', error);
      // Vẫn clear local state ngay cả khi có lỗi
      setIsAuthenticated(false);
      window.location.href = '/login';
    } finally {
      setIsLoading(false);
    }
  };

  /**
   * Kiểm tra xem user có đang authenticated không
   */
  const checkAuth = (): boolean => {
    const token = getAccessToken();
    const isExpired = isAccessTokenExpired();
    
    if (token && !isExpired) {
      if (!isAuthenticated) {
        setIsAuthenticated(true);
      }
      return true;
    }
    
    if (isAuthenticated) {
      setIsAuthenticated(false);
    }
    return false;
  };

  const value: AuthContextType = {
    isAuthenticated,
    isLoading,
    login,
    logout,
    checkAuth,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

// ==================== HOOK ====================

/**
 * Hook để sử dụng AuthContext
 * @throws Error nếu sử dụng ngoài AuthProvider
 */
export const useAuth = (): AuthContextType => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

