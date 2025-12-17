import { useState, useEffect } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { ArrowLeft, Pencil, Loader2, User } from "lucide-react";
import { toast } from "sonner";

// API
import { actorApi } from "@/services/apiActor";
import type { ActorResponseDto } from "@/services/apiActor";
import { PATHS } from "@/config/paths";

// UI Components
import { Button } from "@/components/ui/button";

const ActorDetailPage = () => {
    const navigate = useNavigate();
    const { slug } = useParams<{ slug: string }>();
    const [actor, setActor] = useState<ActorResponseDto | null>(null);
    const [isLoading, setIsLoading] = useState(true);

    useEffect(() => {
        const loadActor = async () => {
            if (!slug) {
                navigate(PATHS.ACTORS);
                return;
            }

            try {
                setIsLoading(true);
                // Check if slug is UUID or slug text
                const isUUID =
                    /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(
                        slug
                    );

                let actorData: ActorResponseDto;
                if (isUUID) {
                    actorData = await actorApi.getById(slug);
                } else {
                    actorData = await actorApi.getBySlug(slug);
                }

                setActor(actorData);
            } catch (error: any) {
                console.error("Load Actor Error:", error);
                toast.error("Không thể tải thông tin diễn viên.");
                navigate(PATHS.ACTORS);
            } finally {
                setIsLoading(false);
            }
        };

        loadActor();
    }, [slug, navigate]);

    const formatDate = (dateString?: string) => {
        if (!dateString) return "N/A";
        try {
            return new Date(dateString).toLocaleDateString("vi-VN");
        } catch {
            return dateString;
        }
    };

    if (isLoading) {
        return (
            <div className="h-full flex items-center justify-center">
                <Loader2 className="h-8 w-8 animate-spin text-indigo-600" />
            </div>
        );
    }

    if (!actor) return null;

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
                            onClick={() => navigate(PATHS.ACTORS)}
                        >
                            <ArrowLeft className="h-5 w-5" />
                        </Button>
                        <div>
                            <h1 className="text-2xl font-bold text-slate-900 dark:text-white">
                                Chi tiết Diễn viên
                            </h1>
                            <p className="text-sm text-slate-500 dark:text-slate-400">
                                Thông tin chi tiết về diễn viên
                            </p>
                        </div>
                    </div>
                    <Button
                        className="bg-[#1e293b] hover:bg-slate-700 text-white px-4 py-2.5 rounded-lg flex items-center gap-2 shadow-md transition-colors text-sm font-medium"
                        onClick={() => navigate(PATHS.ACTOR_EDIT.replace(":id", actor.id))}
                    >
                        <Pencil className="h-4 w-4" />
                        Chỉnh sửa
                    </Button>
                </div>

                <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
                    {/* Left Column: Image */}
                    <div className="lg:col-span-4 xl:col-span-3">
                        <div className="bg-white dark:bg-[#1e293b] p-3 rounded-xl border border-slate-200 dark:border-slate-700 shadow-sm h-[430px]">
                            <div className="relative w-full h-[400px] rounded-lg overflow-hidden group bg-slate-100">
                                {actor.imageUrl ? (
                                    <img
                                        src={actor.imageUrl}
                                        alt={actor.name}
                                        className="absolute inset-0 w-full h-full object-cover transition-transform duration-500 group-hover:scale-105"
                                        onError={(e) => {
                                            (e.target as HTMLImageElement).src =
                                                "https://placehold.co/400x600/e2e8f0/64748b?text=No+Image";
                                        }}
                                    />
                                ) : (
                                    <div className="absolute inset-0 flex items-center justify-center bg-slate-100 dark:bg-slate-800">
                                        <User className="h-24 w-24 text-slate-300 dark:text-slate-600" />
                                    </div>
                                )}
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
                                    {actor.name}
                                </h2>
                                {actor.slug && (
                                    <p className="text-sm text-slate-500 dark:text-slate-400 font-mono">
                                        Slug: {actor.slug}
                                    </p>
                                )}
                            </div>

                            {/* Description */}
                            <div className="border-t border-slate-100 dark:border-slate-800 pt-6 mb-8">
                                <h3 className="text-base font-semibold text-slate-900 dark:text-white mb-3">
                                    Mô tả
                                </h3>
                                <p className="text-slate-600 dark:text-slate-300 leading-relaxed text-sm whitespace-pre-line">
                                    {actor.description || "Chưa có mô tả cho diễn viên này."}
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
                                        title={actor.id}
                                    >
                                        {actor.id}
                                    </p>
                                </div>
                                <div>
                                    <p className="text-xs font-medium text-slate-400 uppercase tracking-wider mb-1">
                                        Slug
                                    </p>
                                    <p
                                        className="font-mono text-sm text-slate-700 dark:text-slate-300 bg-slate-50 dark:bg-slate-800/50 p-2 rounded border border-slate-100 dark:border-slate-700 truncate"
                                        title={actor.slug}
                                    >
                                        {actor.slug || "N/A"}
                                    </p>
                                </div>
                                <div>
                                    <p className="text-xs font-medium text-slate-400 uppercase tracking-wider mb-1">
                                        Ngày tạo
                                    </p>
                                    <p className="text-sm text-slate-700 dark:text-slate-300">
                                        {formatDate(actor.createdAt)}
                                    </p>
                                </div>
                                <div>
                                    <p className="text-xs font-medium text-slate-400 uppercase tracking-wider mb-1">
                                        Ngày cập nhật
                                    </p>
                                    <p className="text-sm text-slate-700 dark:text-slate-300">
                                        {formatDate(actor.updatedAt)}
                                    </p>
                                </div>
                            </div>
                        </div>

                        {/* Image URL Card */}
                        {actor.imageUrl && (
                            <div className="bg-white dark:bg-[#1e293b] rounded-xl border border-slate-200 dark:border-slate-700 shadow-sm p-6">
                                <h3 className="text-lg font-bold text-slate-900 dark:text-white mb-3">
                                    URL Ảnh
                                </h3>
                                <div className="bg-slate-50 dark:bg-slate-800/50 p-3 rounded-lg border border-slate-100 dark:border-slate-700">
                                    <a
                                        href={actor.imageUrl}
                                        target="_blank"
                                        rel="noopener noreferrer"
                                        className="text-sm text-blue-600 dark:text-blue-400 hover:underline break-all font-mono"
                                    >
                                        {actor.imageUrl}
                                    </a>
                                </div>
                            </div>
                        )}
                    </div>
                </div>
            </div>
        </div>
    );
};

export default ActorDetailPage;
