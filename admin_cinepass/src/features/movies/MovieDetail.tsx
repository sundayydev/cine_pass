import { useState, useEffect } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { format } from "date-fns";
import {
  ArrowLeft,
  Calendar,
  Clock,
  Film,
  ExternalLink,
  Pencil,
  Loader2,
} from "lucide-react";

// API & Config
import { movieApi } from "@/services/apiMovie";
import { PATHS } from "@/config/paths";
import type { Movie } from "@/types/moveType";
import { CATEGORY_LABEL_MAP } from "@/constants/movieCategory";

// Shadcn UI Components
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Separator } from "@/components/ui/separator";
import { toast } from "sonner";
import { getYoutubeEmbedUrl } from "@/utils";

const MovieDetailPage = () => {
  const navigate = useNavigate();
  const { slug } = useParams<{ slug: string }>();
  const [movie, setMovie] = useState<Movie | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const loadMovie = async () => {
      if (!slug) {
        toast.error("Không tìm thấy slug phim");
        navigate(PATHS.MOVIES);
        return;
      }

      try {
        setIsLoading(true);
        // Thử lấy phim theo slug trước
        // Nếu slug là UUID (id), thì fallback sang getById
        const isUUID =
          /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(
            slug
          );

        let movieData: Movie;
        if (isUUID) {
          // Nếu là UUID, dùng getById
          movieData = await movieApi.getById(slug);
        } else {
          // Nếu là slug, dùng getBySlug
          movieData = await movieApi.getBySlug(slug);
        }

        setMovie(movieData);
      } catch (error: any) {
        console.error("Load Movie Error:", error);
        let errorMessage = "Không thể tải thông tin phim. Vui lòng thử lại.";

        if (error.response?.data) {
          const responseData = error.response.data;
          if (responseData.message) {
            errorMessage = responseData.message;
          }
        } else if (error.message) {
          errorMessage = error.message;
        }

        toast.error("Lỗi: " + errorMessage, {
          duration: 5000,
        });
        navigate(PATHS.MOVIES);
      } finally {
        setIsLoading(false);
      }
    };

    loadMovie();
  }, [slug, navigate]);

  const formatDate = (dateString?: string) => {
    if (!dateString) return "Chưa có thông tin";
    try {
      return format(new Date(dateString), "dd/MM/yyyy");
    } catch {
      return dateString;
    }
  };

  const renderStatusBadge = (status: string) => {
    const normalizedStatus = status?.toUpperCase();

    switch (normalizedStatus) {
      case "SHOWING":
      case "NOW_SHOWING":
        return (
          <Badge className="bg-emerald-500/15 text-emerald-700 hover:bg-emerald-500/25 border-emerald-500/30 font-medium">
            Đang chiếu
          </Badge>
        );

      case "COMING_SOON":
        return (
          <Badge className="bg-amber-500/15 text-amber-700 hover:bg-amber-500/25 border-amber-500/30 font-medium">
            Sắp chiếu
          </Badge>
        );

      case "ENDED":
        return (
          <Badge className="bg-slate-500/15 text-slate-600 hover:bg-slate-500/25 border-slate-500/30 font-medium">
            Đã kết thúc
          </Badge>
        );

      case "CANCELLED":
        return (
          <Badge className="bg-red-500/15 text-red-700 hover:bg-red-500/25 border-red-500/30 font-medium">
            Đã hủy
          </Badge>
        );

      default:
        return (
          <Badge variant="outline" className="font-medium">
            {status}
          </Badge>
        );
    }
  };

  if (isLoading) {
    return (
      <div className="flex-1 flex items-center justify-center p-4 md:p-8 pt-6">
        <div className="flex flex-col items-center gap-4">
          <Loader2 className="h-8 w-8 animate-spin text-primary" />
          <p className="text-sm text-muted-foreground">
            Đang tải thông tin phim...
          </p>
        </div>
      </div>
    );
  }

  if (!movie) {
    return (
      <div className="flex-1 flex items-center justify-center p-4 md:p-8 pt-6">
        <div className="flex flex-col items-center gap-4">
          <Film className="h-12 w-12 text-muted-foreground" />
          <p className="text-sm font-medium">Không tìm thấy phim</p>
          <Button variant="outline" onClick={() => navigate(PATHS.MOVIES)}>
            Quay lại danh sách
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="flex-1 space-y-4 p-4 md:p-8 pt-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button
            variant="outline"
            size="icon"
            onClick={() => navigate(PATHS.MOVIES)}
          >
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <div>
            <h2 className="text-2xl font-bold tracking-tight">Chi tiết Phim</h2>
            <p className="text-sm text-muted-foreground">
              Thông tin chi tiết về phim
            </p>
          </div>
        </div>
        <Button
          onClick={() => navigate(PATHS.MOVIE_EDIT.replace(":id", movie.id))}
          className="gap-2"
        >
          <Pencil className="h-4 w-4" />
          Chỉnh sửa
        </Button>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* CỘT TRÁI: Poster & Media (chiếm 1 phần) */}
        <div className="space-y-6">
          <Card>
            <CardContent className="p-0">
              <div className="relative aspect-[2/3] w-full overflow-hidden rounded-t-lg">
                <img
                  src={movie.posterUrl || "/placeholder.svg"}
                  alt={movie.title}
                  className="h-full w-full object-cover"
                  onError={(e) => {
                    (e.target as HTMLImageElement).src =
                      "https://placehold.co/400x600/e2e8f0/64748b?text=No+Poster";
                  }}
                />
              </div>
            </CardContent>
          </Card>
        </div>

        {/* CỘT PHẢI: Thông tin chi tiết (chiếm 2 phần) */}
        <div className="lg:col-span-2 space-y-6">
          <Card>
            <CardHeader>
              <div className="flex items-start justify-between">
                <div>
                  <CardTitle className="text-3xl mb-2">{movie.title}</CardTitle>
                  <div className="flex items-center gap-2 flex-wrap">
                    {renderStatusBadge(movie.status as string)}
                    <Badge variant="secondary" className="font-medium">
                      {CATEGORY_LABEL_MAP[movie.category as string] ||
                        movie.category}
                    </Badge>
                  </div>
                </div>
              </div>
            </CardHeader>
            <CardContent className="space-y-6">
              <Separator />

              {/* Thông tin cơ bản */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div className="flex items-center gap-3">
                  <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-primary/10">
                    <Clock className="h-5 w-5 text-primary" />
                  </div>
                  <div>
                    <p className="text-sm text-muted-foreground">Thời lượng</p>
                    <p className="font-semibold">
                      {movie.durationMinutes} phút
                    </p>
                  </div>
                </div>

                <div className="flex items-center gap-3">
                  <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-primary/10">
                    <Calendar className="h-5 w-5 text-primary" />
                  </div>
                  <div>
                    <p className="text-sm text-muted-foreground">
                      Ngày khởi chiếu
                    </p>
                    <p className="font-semibold">
                      {formatDate(movie.releaseDate)}
                    </p>
                  </div>
                </div>
              </div>

              {/* Mô tả */}
              {movie.description && (
                <>
                  <Separator />
                  <div>
                    <h3 className="text-lg font-semibold mb-2">
                      Mô tả nội dung
                    </h3>
                    <p className="text-sm text-muted-foreground leading-relaxed whitespace-pre-line">
                      {movie.description}
                    </p>
                  </div>
                </>
              )}

              {/* Thông tin bổ sung */}
              <Separator />
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
                <div>
                  <p className="text-muted-foreground mb-1">ID</p>
                  <p className="font-mono text-xs">{movie.id}</p>
                </div>
                {movie.slug && (
                  <div>
                    <p className="text-muted-foreground mb-1">Slug</p>
                    <p className="font-mono text-xs">{movie.slug}</p>
                  </div>
                )}
                <div>
                  <p className="text-muted-foreground mb-1">Ngày tạo</p>
                  <p className="font-medium">{formatDate(movie.createdAt)}</p>
                </div>
              </div>
            </CardContent>
          </Card>
          {/* Trailer */}
          {movie.trailerUrl && (
            <Card>
              <CardHeader>
                <CardTitle className="text-lg">Trailer</CardTitle>
              </CardHeader>
              <CardContent className="space-y-3">
                {getYoutubeEmbedUrl(movie.trailerUrl) ? (
                  <div className="relative aspect-video w-full overflow-hidden rounded-lg">
                    <iframe
                      src={getYoutubeEmbedUrl(movie.trailerUrl)!}
                      title={`Trailer ${movie.title}`}
                      className="absolute inset-0 h-full w-full"
                      frameBorder="0"
                      allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
                      allowFullScreen
                    />
                  </div>
                ) : (
                  <Button
                    variant="outline"
                    className="w-full gap-2"
                    onClick={() => window.open(movie.trailerUrl, "_blank")}
                  >
                    <ExternalLink className="h-4 w-4" />
                    Mở trailer trên Youtube
                  </Button>
                )}
              </CardContent>
            </Card>
          )}
        </div>
      </div>
    </div>
  );
};

export default MovieDetailPage;
