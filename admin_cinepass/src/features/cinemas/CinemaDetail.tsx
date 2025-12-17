import { useState, useEffect } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { format } from "date-fns";
import { vi } from "date-fns/locale"; // Để format ngày tháng tiếng Việt
import {
  ArrowLeft,
  Calendar,
  Clock,
  Pencil,
  Loader2,
  Play,
  Star,
  User,
  Film,
} from "lucide-react";

// API & Types
import { movieApi, type MovieDetailResponseDto } from "@/services/apiMovie";
import { PATHS } from "@/config/paths";

// Components
import { Button } from "@/components/ui/button";
import { toast } from "sonner";
import { getYoutubeEmbedUrl } from "@/utils"; // Giả sử bạn có hàm này, nếu chưa mình sẽ cung cấp bên dưới

// Helper để map category và status sang tiếng Việt
const translateCategory = (category: string) => {
  const map: Record<string, string> = {
    Action: "Hành động",
    Comedy: "Hài",
    Drama: "Chính kịch",
    Horror: "Kinh dị",
    Romance: "Lãng mạn",
    SciFi: "Viễn tưởng",
    Animation: "Hoạt hình",
    Adventure: "Phiêu lưu",
    // Thêm các loại khác nếu cần
  };
  return map[category] || category;
};

const MovieDetailPage = () => {
  const navigate = useNavigate();
  const { slug } = useParams<{ slug: string }>();
  const [movie, setMovie] = useState<MovieDetailResponseDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  // --- 1. Fetch Data ---
  useEffect(() => {
    const loadMovie = async () => {
      if (!slug) {
        navigate(PATHS.MOVIES);
        return;
      }

      try {
        setIsLoading(true);
        // Kiểm tra xem slug là UUID hay slug text
        const isUUID =
          /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(
            slug
          );

        let movieData: MovieDetailResponseDto;
        if (isUUID) {
          movieData = await movieApi.getById(slug);
        } else {
          movieData = await movieApi.getBySlug(slug);
        }

        setMovie(movieData);
      } catch (error: any) {
        console.error("Load Movie Error:", error);
        toast.error("Không thể tải thông tin phim.");
        navigate(PATHS.MOVIES);
      } finally {
        setIsLoading(false);
      }
    };

    loadMovie();
  }, [slug, navigate]);

  // --- 2. Render Helpers ---
  const formatDate = (dateString?: string) => {
    if (!dateString) return "N/A";
    try {
      return format(new Date(dateString), "dd/MM/yyyy", { locale: vi });
    } catch {
      return dateString;
    }
  };

  const renderStatusBadge = (status: string) => {
    const s = status?.toLowerCase();
    if (s === "showing" || s === "nowshowing") {
      return (
        <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-emerald-50 text-emerald-700 border border-emerald-100">
          Đang chiếu
        </span>
      );
    }
    if (s === "comingsoon") {
      return (
        <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-amber-50 text-amber-700 border border-amber-100">
          Sắp chiếu
        </span>
      );
    }
    return (
      <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-slate-100 dark:bg-slate-800 text-slate-800 dark:text-slate-200 border border-slate-200 dark:border-slate-600">
        Đã kết thúc
      </span>
    );
  };

  const renderStars = (rating: number = 0) => {
    return (
      <div className="flex text-yellow-400 text-xs">
        {[1, 2, 3, 4, 5].map((star) => (
          <Star
            key={star}
            className={`w-4 h-4 ${star <= rating ? "fill-yellow-400" : "text-gray-300"
              }`}
          />
        ))}
      </div>
    );
  };

  // --- 3. Loading & Empty States ---
  if (isLoading) {
    return (
      <div className="h-full flex items-center justify-center">
        <Loader2 className="h-8 w-8 animate-spin text-indigo-600" />
      </div>
    );
  }

  if (!movie) return null;

  // --- 4. Main Render ---
  return (
    <div className="flex-1 overflow-y-auto p-6 lg:p-10 bg-slate-50 dark:bg-[#0f172a] min-h-screen text-slate-800 dark:text-slate-200">
      <div className="max-w-6xl mx-auto space-y-6">
        {/* Header Section */}
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
          <div className="flex items-center gap-4">
            <Button
              variant="outline"
              size="icon"
              className="bg-white dark:bg-slate-800 border-slate-200 dark:border-slate-700 hover:bg-slate-50 dark:hover:bg-slate-700 text-slate-600 dark:text-slate-300 h-10 w-10 shadow-sm"
              onClick={() => navigate(PATHS.MOVIES)}
            >
              <ArrowLeft className="h-5 w-5" />
            </Button>
            <div>
              <h1 className="text-2xl font-bold text-slate-900 dark:text-white">
                Chi tiết Phim
              </h1>
              <p className="text-sm text-slate-500 dark:text-slate-400">
                Thông tin chi tiết về phim
              </p>
            </div>
          </div>
          <Button
            className="bg-[#1e293b] hover:bg-slate-700 text-white px-4 py-2.5 rounded-lg flex items-center gap-2 shadow-md transition-colors text-sm font-medium"
            onClick={() => navigate(PATHS.MOVIE_EDIT.replace(":id", movie.id))}
          >
            <Pencil className="h-4 w-4" />
            Chỉnh sửa
          </Button>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
          {/* Left Column: Poster */}
          <div className="lg:col-span-4 xl:col-span-3">
            <div className="bg-white dark:bg-[#1e293b] p-3 rounded-xl border border-slate-200 dark:border-slate-700 shadow-sm h-full">
              <div className="relative w-full h-full min-h-[400px] rounded-lg overflow-hidden group bg-slate-100">
                <img
                  src={movie.posterUrl || "/placeholder.svg"}
                  alt={movie.title}
                  className="absolute inset-0 w-full h-full object-cover transition-transform duration-500 group-hover:scale-105"
                  onError={(e) => {
                    (e.target as HTMLImageElement).src =
                      "https://placehold.co/400x600/e2e8f0/64748b?text=No+Poster";
                  }}
                />
                <div className="absolute inset-0 bg-gradient-to-t from-black/60 to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-300"></div>
              </div>
            </div>
          </div>

          {/* Right Column: Details */}
          <div className="lg:col-span-8 xl:col-span-9 space-y-6">
            {/* Main Info Card */}
            <div className="bg-white dark:bg-[#1e293b] rounded-xl border border-slate-200 dark:border-slate-700 shadow-sm p-6 lg:p-8">
              <div className="mb-6">
                <h2 className="text-3xl font-bold text-slate-900 dark:text-white mb-3 leading-tight">
                  {movie.title}
                </h2>
                <div className="flex flex-wrap gap-2">
                  {renderStatusBadge(movie.status)}

                  <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-indigo-50 dark:bg-indigo-900/30 text-indigo-700 dark:text-indigo-300 border border-indigo-100 dark:border-indigo-800">
                    {translateCategory(movie.category)}
                  </span>

                  <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-rose-50 dark:bg-rose-900/30 text-rose-700 dark:text-rose-300 border border-rose-100 dark:border-rose-800">
                    {movie.ageLimit ? `${movie.ageLimit}+` : "Mọi lứa tuổi"}
                  </span>
                </div>
              </div>

              {/* Stats Grid */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-8 mb-8">
                <div className="flex items-start gap-4">
                  <div className="p-3 bg-slate-50 dark:bg-slate-800 rounded-lg text-slate-400 dark:text-slate-500">
                    <Clock className="h-6 w-6" />
                  </div>
                  <div>
                    <p className="text-sm font-medium text-slate-500 dark:text-slate-400 mb-1">
                      Thời lượng
                    </p>
                    <p className="text-lg font-semibold text-slate-900 dark:text-white">
                      {movie.durationMinutes} phút
                    </p>
                  </div>
                </div>
                <div className="flex items-start gap-4">
                  <div className="p-3 bg-slate-50 dark:bg-slate-800 rounded-lg text-slate-400 dark:text-slate-500">
                    <Calendar className="h-6 w-6" />
                  </div>
                  <div>
                    <p className="text-sm font-medium text-slate-500 dark:text-slate-400 mb-1">
                      Ngày khởi chiếu
                    </p>
                    <p className="text-lg font-semibold text-slate-900 dark:text-white">
                      {formatDate(movie.releaseDate)}
                    </p>
                  </div>
                </div>
              </div>

              {/* Description */}
              <div className="border-t border-slate-100 dark:border-slate-800 pt-6 mb-8">
                <h3 className="text-base font-semibold text-slate-900 dark:text-white mb-3">
                  Mô tả nội dung
                </h3>
                <p className="text-slate-600 dark:text-slate-300 leading-relaxed text-sm whitespace-pre-line">
                  {movie.description || "Chưa có mô tả cho phim này."}
                </p>
              </div>

              {/* Technical Metadata */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-y-6 gap-x-12 border-t border-slate-100 dark:border-slate-800 pt-6">
                <div>
                  <p className="text-xs font-medium text-slate-400 uppercase tracking-wider mb-1">
                    ID
                  </p>
                  <p
                    className="font-mono text-sm text-slate-700 dark:text-slate-300 bg-slate-50 dark:bg-slate-800/50 p-2 rounded border border-slate-100 dark:border-slate-700 truncate"
                    title={movie.id}
                  >
                    {movie.id}
                  </p>
                </div>
                <div>
                  <p className="text-xs font-medium text-slate-400 uppercase tracking-wider mb-1">
                    Slug
                  </p>
                  <p
                    className="font-mono text-sm text-slate-700 dark:text-slate-300 bg-slate-50 dark:bg-slate-800/50 p-2 rounded border border-slate-100 dark:border-slate-700 truncate"
                    title={movie.slug}
                  >
                    {movie.slug || "N/A"}
                  </p>
                </div>
                <div>
                  <p className="text-xs font-medium text-slate-400 uppercase tracking-wider mb-1">
                    Ngày tạo
                  </p>
                  <p className="text-sm text-slate-700 dark:text-slate-300">
                    {formatDate(movie.createdAt)}
                  </p>
                </div>
                <div>
                  <p className="text-xs font-medium text-slate-400 uppercase tracking-wider mb-1">
                    Trạng thái hệ thống
                  </p>
                  <div className="flex items-center gap-2">
                    <div className="h-2.5 w-2.5 rounded-full bg-green-500"></div>
                    <p className="text-sm text-slate-700 dark:text-slate-300">
                      Active
                    </p>
                  </div>
                </div>
              </div>
            </div>

            {/* Actors Section */}
            {movie.actors && movie.actors.length > 0 && (
              <div className="bg-white dark:bg-[#1e293b] rounded-xl border border-slate-200 dark:border-slate-700 shadow-sm p-6 lg:p-8">
                <div className="flex items-center justify-between mb-6">
                  <h3 className="text-lg font-bold text-slate-900 dark:text-white">
                    Diễn viên
                  </h3>
                  {/* <a href="#" className="text-sm font-medium text-indigo-600 ...">Xem tất cả</a> */}
                </div>
                <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6 gap-6">
                  {movie.actors.map((actor) => (
                    <div key={actor.id} className="text-center group">
                      <div className="relative w-24 h-24 mx-auto mb-3 rounded-full overflow-hidden ring-2 ring-transparent group-hover:ring-indigo-500 transition-all bg-slate-100">
                        <img
                          src={actor.imageUrl || "https://placehold.co/100"}
                          alt={actor.name}
                          className="w-full h-full object-cover"
                          onError={(e) => {
                            (e.target as HTMLImageElement).src =
                              "https://placehold.co/100?text=Actor";
                          }}
                        />
                      </div>
                      <p className="text-sm font-semibold text-slate-900 dark:text-white group-hover:text-indigo-600 transition-colors line-clamp-1">
                        {actor.name}
                      </p>
                      <p className="text-xs text-slate-500 dark:text-slate-400 line-clamp-1">
                        {actor.description || "Diễn viên"}
                      </p>
                    </div>
                  ))}
                </div>
              </div>
            )}

            {/* Trailer Section */}
            {movie.trailerUrl && (
              <div className="bg-white dark:bg-[#1e293b] rounded-xl border border-slate-200 dark:border-slate-700 shadow-sm p-6 lg:p-8">
                <h3 className="text-lg font-bold text-slate-900 dark:text-white mb-4">
                  Trailer
                </h3>
                <div className="relative w-full rounded-lg overflow-hidden bg-black aspect-video group">
                  {getYoutubeEmbedUrl(movie.trailerUrl) ? (
                    <iframe
                      src={getYoutubeEmbedUrl(movie.trailerUrl)!}
                      title={`Trailer ${movie.title}`}
                      className="absolute inset-0 h-full w-full"
                      frameBorder="0"
                      allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
                      allowFullScreen
                    />
                  ) : (
                    <div className="absolute inset-0 flex items-center justify-center">
                      <a
                        href={movie.trailerUrl}
                        target="_blank"
                        rel="noreferrer"
                        className="flex flex-col items-center gap-2 group cursor-pointer"
                      >
                        <div className="w-16 h-16 bg-white/20 backdrop-blur-sm rounded-full flex items-center justify-center group-hover:bg-red-600 transition-colors duration-300">
                          <Play className="text-white h-8 w-8 ml-1" />
                        </div>
                        <span className="text-white text-sm font-medium">
                          Mở link gốc
                        </span>
                      </a>
                    </div>
                  )}
                </div>
              </div>
            )}

            {/* Reviews Section */}
            <div className="bg-white dark:bg-[#1e293b] rounded-xl border border-slate-200 dark:border-slate-700 shadow-sm p-6 lg:p-8">
              <div className="flex items-center justify-between mb-6">
                <h3 className="text-lg font-bold text-slate-900 dark:text-white">
                  Đánh giá từ người xem
                </h3>
                {movie.reviews && movie.reviews.length > 0 && (
                  <div className="flex items-center gap-2">
                    <span className="text-2xl font-bold text-yellow-500">
                      {(
                        movie.reviews.reduce(
                          (acc, cur) => acc + (cur.rating || 0),
                          0
                        ) / movie.reviews.length
                      ).toFixed(1)}
                    </span>
                    <div className="text-xs text-slate-500 dark:text-slate-400">
                      <div className="flex text-yellow-500 text-sm">★★★★★</div>
                      <span>({movie.reviews.length} đánh giá)</span>
                    </div>
                  </div>
                )}
              </div>

              <div className="space-y-6">
                {movie.reviews && movie.reviews.length > 0 ? (
                  movie.reviews.slice(0, 3).map((review) => (
                    <div
                      key={review.id}
                      className="flex gap-4 pb-6 border-b border-slate-100 dark:border-slate-800 last:pb-0 last:border-0"
                    >
                      <div className="w-10 h-10 rounded-full border border-slate-200 dark:border-slate-600 flex items-center justify-center bg-slate-100 dark:bg-slate-700 text-slate-500">
                        <User className="h-6 w-6" />
                      </div>
                      <div className="flex-1">
                        <div className="flex justify-between items-start mb-1">
                          <div>
                            <p className="text-sm font-semibold text-slate-900 dark:text-white">
                              {review.userName || "Người dùng ẩn danh"}
                            </p>
                            <p className="text-xs text-slate-500 dark:text-slate-400">
                              {formatDate(review.createdAt)}
                            </p>
                          </div>
                          {renderStars(review.rating)}
                        </div>
                        <p className="text-sm text-slate-600 dark:text-slate-300 leading-relaxed">
                          {review.comment}
                        </p>
                      </div>
                    </div>
                  ))
                ) : (
                  <p className="text-sm text-slate-500 text-center italic">
                    Chưa có đánh giá nào.
                  </p>
                )}
              </div>
              {movie.reviews && movie.reviews.length > 3 && (
                <button className="w-full mt-6 py-2 text-sm text-indigo-600 dark:text-indigo-400 font-medium border border-indigo-200 dark:border-indigo-800 rounded-lg hover:bg-indigo-50 dark:hover:bg-indigo-900/30 transition-colors">
                  Xem thêm đánh giá
                </button>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default MovieDetailPage;