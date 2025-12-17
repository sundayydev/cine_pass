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
  Star,
  PlayCircle,
  Quote,
  Info,
} from "lucide-react";

// API & Config
import { movieApi, type MovieDetailResponseDto, type MovieActorDto, type MovieReviewDto } from "@/services/apiMovie";
import { PATHS } from "@/config/paths";
import { CATEGORY_LABEL_MAP } from "@/constants/movieCategory";

// Shadcn UI Components
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Separator } from "@/components/ui/separator";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { ScrollArea, ScrollBar } from "@/components/ui/scroll-area";
import { toast } from "sonner";
import { getYoutubeEmbedUrl } from "@/utils";
import { cn } from "@/lib/utils";

const MovieDetailPage = () => {
  const navigate = useNavigate();
  const { slug } = useParams<{ slug: string }>();
  const [movie, setMovie] = useState<MovieDetailResponseDto | null>(null);
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
        const isUUID =
          /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(slug);

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

  const formatDate = (dateString?: string) => {
    if (!dateString) return "N/A";
    try {
      return format(new Date(dateString), "dd 'thg' MM, yyyy");
    } catch {
      return dateString;
    }
  };

  const renderStatusBadge = (status: string) => {
    const normalizedStatus = status?.toUpperCase();
    const styles = {
      SHOWING: "bg-emerald-500/10 text-emerald-600 border-emerald-200 hover:bg-emerald-500/20",
      NOW_SHOWING: "bg-emerald-500/10 text-emerald-600 border-emerald-200 hover:bg-emerald-500/20",
      COMING_SOON: "bg-amber-500/10 text-amber-600 border-amber-200 hover:bg-amber-500/20",
      ENDED: "bg-slate-500/10 text-slate-600 border-slate-200 hover:bg-slate-500/20",
      CANCELLED: "bg-red-500/10 text-red-600 border-red-200 hover:bg-red-500/20",
    };

    // @ts-ignore
    const currentStyle = styles[normalizedStatus] || "bg-secondary text-secondary-foreground";

    let label = status;
    switch (normalizedStatus) {
      case "SHOWING": case "NOW_SHOWING": label = "Đang chiếu"; break;
      case "COMING_SOON": label = "Sắp chiếu"; break;
      case "ENDED": label = "Đã kết thúc"; break;
      case "CANCELLED": label = "Đã hủy"; break;
    }

    return <Badge variant="outline" className={cn("px-3 py-1 font-medium border", currentStyle)}>{label}</Badge>;
  };

  const renderStars = (rating: number = 0) => {
    return (
      <div className="flex gap-0.5 items-center">
        {[1, 2, 3, 4, 5].map((star) => (
          <Star
            key={star}
            className={cn("w-4 h-4", star <= rating ? "fill-yellow-400 text-yellow-400" : "text-gray-300 fill-gray-100")}
          />
        ))}
      </div>
    );
  };

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-background">
        <div className="flex flex-col items-center gap-4 animate-pulse">
          <Loader2 className="h-10 w-10 animate-spin text-primary" />
          <p className="text-sm text-muted-foreground font-medium">Đang tải dữ liệu...</p>
        </div>
      </div>
    );
  }

  if (!movie) return null;

  const averageRating =
    movie.reviews && movie.reviews.length > 0
      ? movie.reviews.reduce((acc, review) => acc + (review.rating || 0), 0) / movie.reviews.length
      : 0;

  return (
    <div className="relative min-h-screen bg-background pb-20 overflow-x-hidden">
      {/* --- HERO SECTION BACKGROUND --- */}
      <div className="absolute inset-0 h-[60vh] w-full overflow-hidden z-0">
        <img
          src={movie.posterUrl || "/placeholder.svg"}
          alt="Backdrop"
          className="w-full h-full object-cover blur-3xl opacity-40 dark:opacity-20 scale-110"
        />
        <div className="absolute inset-0 bg-gradient-to-t from-background via-background/80 to-transparent" />
        <div className="absolute inset-0 bg-gradient-to-r from-background/90 via-background/40 to-transparent" />
      </div>

      <div className="container relative z-10 px-4 md:px-8 pt-6 mx-auto max-w-7xl">
        {/* Navigation Bar */}
        <div className="flex items-center justify-between mb-8">
          <Button
            variant="ghost"
            className="group hover:bg-background/50 hover:text-primary backdrop-blur-sm"
            onClick={() => navigate(PATHS.MOVIES)}
          >
            <ArrowLeft className="mr-2 h-4 w-4 group-hover:-translate-x-1 transition-transform" />
            Quay lại
          </Button>
          <Button
            variant="secondary"
            className="bg-background/50 backdrop-blur-sm border shadow-sm hover:bg-background"
            onClick={() => navigate(PATHS.MOVIE_EDIT.replace(":id", movie.id))}
          >
            <Pencil className="mr-2 h-4 w-4" />
            Chỉnh sửa
          </Button>
        </div>

        {/* --- MAIN CONTENT HEADER --- */}
        <div className="grid grid-cols-1 md:grid-cols-[300px_1fr] lg:grid-cols-[350px_1fr] gap-8 lg:gap-12">

          {/* POSTER COLUMN */}
          <div className="flex flex-col gap-6">
            <div className="relative group perspective-1000">
              <div className="relative aspect-[2/3] w-full overflow-hidden rounded-2xl shadow-2xl border-4 border-background/20 ring-1 ring-white/10 transition-transform duration-500 group-hover:scale-[1.02]">
                <img
                  src={movie.posterUrl || "/placeholder.svg"}
                  alt={movie.title}
                  className="h-full w-full object-cover"
                  onError={(e) => {
                    (e.target as HTMLImageElement).src = "https://placehold.co/400x600?text=No+Poster";
                  }}
                />
                {/* Glossy Overlay */}
                <div className="absolute inset-0 bg-gradient-to-tr from-white/5 to-transparent opacity-0 group-hover:opacity-100 transition-opacity" />
              </div>
            </div>

            {/* Quick Stats Card */}
            <Card className="bg-background/60 backdrop-blur-md border-muted">
              <CardContent className="p-4 grid grid-cols-2 gap-4 divide-x divide-border">
                <div className="text-center space-y-1">
                  <div className="text-2xl font-bold text-primary flex items-center justify-center gap-1">
                    {averageRating.toFixed(1)} <Star className="w-4 h-4 fill-primary text-primary" />
                  </div>
                  <p className="text-xs text-muted-foreground">{movie.reviews?.length || 0} đánh giá</p>
                </div>
                <div className="text-center space-y-1 pl-4">
                  <div className="text-2xl font-bold text-foreground">
                    {movie.durationMinutes}<span className="text-sm font-normal text-muted-foreground ml-1">phút</span>
                  </div>
                  <p className="text-xs text-muted-foreground">Thời lượng</p>
                </div>
              </CardContent>
            </Card>
          </div>

          {/* INFO COLUMN */}
          <div className="space-y-8 pt-2 md:pt-4">
            <div>
              <div className="flex flex-wrap gap-2 mb-4">
                {renderStatusBadge(movie.status as string)}
                <Badge variant="secondary" className="bg-primary/10 text-primary hover:bg-primary/20">
                  {CATEGORY_LABEL_MAP[movie.category as string] || movie.category}
                </Badge>
              </div>

              <h1 className="text-4xl md:text-5xl lg:text-6xl font-bold tracking-tight text-foreground mb-4 leading-tight">
                {movie.title}
              </h1>

              <div className="flex flex-wrap items-center gap-6 text-muted-foreground text-sm md:text-base mb-6">
                <div className="flex items-center gap-2">
                  <Calendar className="w-4 h-4" />
                  <span>Khởi chiếu: <span className="text-foreground font-medium">{formatDate(movie.releaseDate)}</span></span>
                </div>
                <div className="hidden md:block w-1 h-1 bg-muted-foreground rounded-full" />
                <div className="flex items-center gap-2">
                  <Clock className="w-4 h-4" />
                  <span>Cập nhật: {formatDate(movie.updatedAt || movie.createdAt)}</span>
                </div>
              </div>

              {/* Action Buttons */}
              <div className="flex flex-wrap gap-3">
                {movie.trailerUrl && (
                  <Button
                    size="lg"
                    className="rounded-full gap-2 shadow-lg shadow-primary/20"
                    onClick={() => window.open(movie.trailerUrl, '_blank')}
                  >
                    <PlayCircle className="w-5 h-5 fill-current" />
                    Xem Trailer
                  </Button>
                )}
                <Button size="lg" variant="outline" className="rounded-full border-2 gap-2 bg-transparent hover:bg-accent/50">
                  <ExternalLink className="w-4 h-4" />
                  Chia sẻ
                </Button>
              </div>
            </div>

            {/* Description */}
            <div className="space-y-3">
              <h3 className="text-lg font-semibold flex items-center gap-2">
                <Quote className="w-4 h-4 text-primary rotate-180" />
                Nội dung phim
              </h3>
              <p className="text-muted-foreground leading-relaxed text-lg max-w-3xl">
                {movie.description || "Chưa có mô tả cho phim này."}
              </p>
            </div>

            <Separator className="bg-border/60" />

            {/* Actors Section - Carousel Style */}
            <div className="space-y-4">
              <h3 className="text-xl font-bold">Diễn viên & Đoàn làm phim</h3>
              {movie.actors && movie.actors.length > 0 ? (
                <ScrollArea className="w-full whitespace-nowrap pb-4">
                  <div className="flex w-max space-x-4">
                    {movie.actors.map((actor) => (
                      <div key={actor.id} className="w-[140px] space-y-3 group cursor-pointer">
                        <div className="overflow-hidden rounded-xl aspect-[3/4]">
                          <img
                            src={actor.imageUrl}
                            alt={actor.name}
                            className="h-full w-full object-cover transition-transform duration-300 group-hover:scale-110"
                            onError={(e) => {
                              (e.target as HTMLImageElement).src = "https://placehold.co/300x400?text=Actor";
                            }}
                          />
                        </div>
                        <div className="space-y-1">
                          <h4 className="text-sm font-bold leading-tight whitespace-normal group-hover:text-primary transition-colors">
                            {actor.name}
                          </h4>
                          <p className="text-xs text-muted-foreground whitespace-normal line-clamp-2">
                            {actor.description || "Diễn viên"}
                          </p>
                        </div>
                      </div>
                    ))}
                  </div>
                  <ScrollBar orientation="horizontal" />
                </ScrollArea>
              ) : (
                <p className="text-muted-foreground italic">Chưa cập nhật danh sách diễn viên.</p>
              )}
            </div>
          </div>
        </div>

        {/* --- BOTTOM SECTION --- */}
        <div className="mt-12 grid grid-cols-1 lg:grid-cols-3 gap-8">

          {/* LEFT: Reviews (2/3) */}
          <div className="lg:col-span-2 space-y-6">
            <div className="flex items-center justify-between">
              <h3 className="text-2xl font-bold">Đánh giá từ khán giả</h3>
              {movie.reviews && movie.reviews.length > 0 && (
                <Badge variant="outline" className="text-sm">{movie.reviews.length} nhận xét</Badge>
              )}
            </div>

            {movie.reviews && movie.reviews.length > 0 ? (
              <div className="space-y-4">
                {movie.reviews.map((review) => (
                  <div key={review.id} className="bg-card/50 border rounded-xl p-5 hover:bg-accent/30 transition-colors">
                    <div className="flex gap-4">
                      <Avatar className="h-10 w-10 border">
                        <AvatarFallback className="bg-primary/10 text-primary font-bold">
                          {review.userName?.[0]?.toUpperCase() || "U"}
                        </AvatarFallback>
                      </Avatar>
                      <div className="flex-1">
                        <div className="flex items-center justify-between mb-1">
                          <span className="font-semibold text-foreground">{review.userName || "Ẩn danh"}</span>
                          <span className="text-xs text-muted-foreground">{formatDate(review.createdAt)}</span>
                        </div>
                        <div className="mb-2">{renderStars(review.rating)}</div>
                        <p className="text-sm text-muted-foreground leading-relaxed">{review.comment}</p>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <div className="text-center py-10 bg-muted/20 rounded-xl border border-dashed">
                <p className="text-muted-foreground">Chưa có đánh giá nào. Hãy là người đầu tiên!</p>
              </div>
            )}
          </div>

          {/* RIGHT: Trailer & Technical (1/3) */}
          <div className="space-y-6">
            {/* Trailer Box */}
            {movie.trailerUrl && (
              <Card className="overflow-hidden border-0 shadow-lg">
                <CardHeader className="bg-muted/50 pb-2">
                  <CardTitle className="text-lg flex items-center gap-2">
                    <Film className="w-5 h-5 text-primary" /> Trailer
                  </CardTitle>
                </CardHeader>
                <CardContent className="p-0">
                  {getYoutubeEmbedUrl(movie.trailerUrl) ? (
                    <div className="aspect-video">
                      <iframe
                        src={getYoutubeEmbedUrl(movie.trailerUrl)!}
                        title="Trailer"
                        className="w-full h-full"
                        allowFullScreen
                      />
                    </div>
                  ) : (
                    <div className="p-6 text-center">
                      <Button variant="outline" onClick={() => window.open(movie.trailerUrl, "_blank")}>
                        Mở trên Youtube
                      </Button>
                    </div>
                  )}
                </CardContent>
              </Card>
            )}

            {/* Technical Info */}
            <Card>
              <CardHeader className="pb-3">
                <CardTitle className="text-base flex items-center gap-2">
                  <Info className="w-4 h-4" /> Thông tin kỹ thuật
                </CardTitle>
              </CardHeader>
              <CardContent className="grid gap-3 text-sm">
                <div className="flex justify-between py-2 border-b">
                  <span className="text-muted-foreground">ID Phim</span>
                  <span className="font-mono text-xs">{movie.id.slice(0, 8)}...</span>
                </div>
                <div className="flex justify-between py-2 border-b">
                  <span className="text-muted-foreground">Định dạng</span>
                  <span className="font-medium">2D / Digital</span>
                </div>
                {movie.slug && (
                  <div className="flex flex-col gap-1 py-2">
                    <span className="text-muted-foreground">Slug</span>
                    <span className="font-mono text-xs text-muted-foreground/80">{movie.slug}</span>
                  </div>
                )}
              </CardContent>
            </Card>
          </div>

        </div>
      </div>
    </div>
  );
};

export default MovieDetailPage;