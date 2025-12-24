import { useState, useEffect } from 'react';
import { Plus, X, Loader2, User2, Search, Check, Film, Star, SearchIcon } from 'lucide-react';
import { toast } from 'sonner';

// API Services
import { movieActorApi, type MovieActorResponseDto } from '@/services/apiMovieActor';
import { actorApi, type ActorResponseDto } from '@/services/apiActor';

// Shadcn UI Components
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogHeader,
    DialogTitle,
    DialogTrigger,
    DialogFooter,
} from '@/components/ui/dialog';
import { Label } from '@/components/ui/label';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { cn } from '@/lib/utils';

interface MovieActorManagerProps {
    movieId: string;
}

interface ActorWithRelation extends MovieActorResponseDto {
    actorInfo?: ActorResponseDto;
}

export const MovieActorManager = ({ movieId }: MovieActorManagerProps) => {
    const [movieActors, setMovieActors] = useState<ActorWithRelation[]>([]);
    const [allActors, setAllActors] = useState<ActorResponseDto[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [isDialogOpen, setIsDialogOpen] = useState(false);
    const [isAdding, setIsAdding] = useState(false);

    // Form state for adding actor
    const [selectedActorId, setSelectedActorId] = useState<string>('');
    const [characterName, setCharacterName] = useState('');
    const [role, setRole] = useState('');
    const [searchQuery, setSearchQuery] = useState('');

    // Load movie's actors
    const loadMovieActors = async () => {
        try {
            setIsLoading(true);
            const actors = await movieActorApi.getByMovieId(movieId);

            // Load actor details for each relation
            const actorsWithInfo = await Promise.all(
                actors.map(async (ma) => {
                    try {
                        const actorInfo = await actorApi.getById(ma.actorId);
                        return { ...ma, actorInfo };
                    } catch {
                        return ma;
                    }
                })
            );

            setMovieActors(actorsWithInfo);
        } catch (error: any) {
            console.error('Load movie actors error:', error);
            toast.error(error.message || 'Không thể tải danh sách diễn viên');
        } finally {
            setIsLoading(false);
        }
    };

    // Load all actors for selection
    const loadAllActors = async () => {
        try {
            const actors = await actorApi.getAll();
            setAllActors(actors);
        } catch (error: any) {
            console.error('Load all actors error:', error);
            toast.error('Không thể tải danh sách diễn viên');
        }
    };

    useEffect(() => {
        if (movieId) {
            loadMovieActors();
            loadAllActors();
        }
    }, [movieId]);

    // Filter actors that aren't already in the movie
    const availableActors = allActors.filter(
        (actor) => !movieActors.some((ma) => ma.actorId === actor.id)
    );

    // Filter actors by search query
    const filteredActors = availableActors.filter((actor) =>
        actor.name.toLowerCase().includes(searchQuery.toLowerCase())
    );

    // Handle adding actor to movie
    const handleAddActor = async () => {
        if (!selectedActorId) {
            toast.error('Vui lòng chọn diễn viên');
            return;
        }

        try {
            setIsAdding(true);
            await movieActorApi.create({
                movieId,
                actorId: selectedActorId,
                characterName: characterName || undefined,
                role: role || undefined,
            });

            toast.success('Đã thêm diễn viên vào phim!');

            // Reset form
            setSelectedActorId('');
            setCharacterName('');
            setRole('');
            setSearchQuery('');
            setIsDialogOpen(false);

            // Reload list
            await loadMovieActors();
        } catch (error: any) {
            console.error('Add actor error:', error);
            toast.error(error.message || 'Không thể thêm diễn viên');
        } finally {
            setIsAdding(false);
        }
    };

    // Handle removing actor from movie
    const handleRemoveActor = async (relationId: string, actorName: string) => {
        if (!confirm(`Bạn có chắc muốn xóa "${actorName}" khỏi phim này?`)) {
            return;
        }

        try {
            await movieActorApi.delete(relationId);
            toast.success('Đã xóa diễn viên khỏi phim');
            await loadMovieActors();
        } catch (error: any) {
            console.error('Remove actor error:', error);
            toast.error(error.message || 'Không thể xóa diễn viên');
        }
    };

    if (isLoading) {
        return (
            <Card className="border-2">
                <CardHeader>
                    <CardTitle className="flex items-center gap-2">
                        <Film className="h-5 w-5 text-primary" />
                        Diễn viên
                    </CardTitle>
                </CardHeader>
                <CardContent>
                    <div className="flex items-center justify-center py-12">
                        <div className="text-center space-y-3">
                            <Loader2 className="h-8 w-8 animate-spin text-primary mx-auto" />
                            <p className="text-sm text-muted-foreground">Đang tải danh sách diễn viên...</p>
                        </div>
                    </div>
                </CardContent>
            </Card>
        );
    }

    return (
        <Card className="border-2 overflow-hidden">
            <CardHeader className="bg-gradient-to-r from-primary/5 to-primary/10 border-b">
                <div className="flex items-center justify-between">
                    <div className="space-y-1">
                        <CardTitle className="flex items-center gap-2 text-xl">
                            <Film className="h-5 w-5 text-primary" />
                            Diễn viên
                            {movieActors.length > 0 && (
                                <Badge variant="secondary" className="ml-2">
                                    {movieActors.length}
                                </Badge>
                            )}
                        </CardTitle>
                        <CardDescription>
                            Quản lý danh sách diễn viên tham gia phim
                        </CardDescription>
                    </div>
                    <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
                        <DialogTrigger asChild>
                            <Button size="default" className="shadow-md hover:shadow-lg transition-all">
                                <Plus className="h-4 w-4 mr-2" />
                                Thêm diễn viên
                            </Button>
                        </DialogTrigger>
                        <DialogContent className="max-w-3xl max-h-[85vh] overflow-hidden flex flex-col">
                            <DialogHeader className="pb-4 border-b">
                                <DialogTitle className="text-2xl flex items-center gap-2">
                                    <Star className="h-6 w-6 text-primary" />
                                    Thêm diễn viên vào phim
                                </DialogTitle>
                                <DialogDescription className="text-base">
                                    Chọn diễn viên từ danh sách và nhập thông tin vai diễn
                                </DialogDescription>
                            </DialogHeader>

                            <div className="flex-1 overflow-y-auto py-6 space-y-6">
                                {/* Search actors */}
                                <div className="space-y-2">
                                    <Label className="text-base font-semibold"><SearchIcon className="h-5 w-5 mr-2" /> Tìm kiếm diễn viên</Label>
                                    <div className="relative">
                                        <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-5 w-5 text-muted-foreground" />
                                        <Input
                                            placeholder="Nhập tên diễn viên để tìm kiếm..."
                                            value={searchQuery}
                                            onChange={(e) => setSearchQuery(e.target.value)}
                                            className="pl-10 h-11 text-base"
                                        />
                                    </div>
                                </div>

                                {/* Actor selection */}
                                <div className="space-y-3">
                                    <Label className="text-base font-semibold">
                                        <User2 className="h-5 w-5 mr-2" /> Chọn diễn viên <span className="text-red-500">*</span>
                                    </Label>
                                    <div className="grid grid-cols-1 md:grid-cols-2 gap-3 max-h-[280px] overflow-y-auto p-1 pr-2">
                                        {filteredActors.length === 0 ? (
                                            <div className="col-span-2 text-center py-12">
                                                <User2 className="h-16 w-16 mx-auto mb-4 text-muted-foreground/40" />
                                                <p className="text-base font-medium text-muted-foreground">
                                                    {searchQuery
                                                        ? 'Không tìm thấy diễn viên phù hợp'
                                                        : 'Tất cả diễn viên đã được thêm vào phim'}
                                                </p>
                                                {searchQuery && (
                                                    <p className="text-sm text-muted-foreground mt-2">
                                                        Thử tìm kiếm với từ khóa khác
                                                    </p>
                                                )}
                                            </div>
                                        ) : (
                                            filteredActors.map((actor) => (
                                                <button
                                                    key={actor.id}
                                                    type="button"
                                                    onClick={() => setSelectedActorId(actor.id)}
                                                    className={cn(
                                                        "group relative flex items-center gap-4 p-4 rounded-xl border-2 transition-all text-left overflow-hidden",
                                                        selectedActorId === actor.id
                                                            ? 'border-primary bg-primary/10 shadow-md scale-[1.02]'
                                                            : 'border-border hover:border-primary/40 hover:bg-accent/50 hover:shadow-sm'
                                                    )}
                                                >
                                                    {/* Selection indicator */}
                                                    {selectedActorId === actor.id && (
                                                        <div className="absolute top-2 right-2 h-6 w-6 rounded-full bg-primary flex items-center justify-center shadow-lg">
                                                            <Check className="h-4 w-4 text-primary-foreground" />
                                                        </div>
                                                    )}

                                                    <Avatar className={cn(
                                                        "h-16 w-16 ring-2 ring-offset-2 transition-all",
                                                        selectedActorId === actor.id
                                                            ? "ring-primary ring-offset-background"
                                                            : "ring-transparent group-hover:ring-primary/30"
                                                    )}>
                                                        <AvatarImage src={actor.imageUrl} alt={actor.name} className="object-cover" />
                                                        <AvatarFallback className="bg-gradient-to-br from-primary/20 to-primary/40">
                                                            <User2 className="h-7 w-7 text-primary" />
                                                        </AvatarFallback>
                                                    </Avatar>
                                                    <div className="flex-1 min-w-0">
                                                        <p className={cn(
                                                            "font-semibold truncate text-base",
                                                            selectedActorId === actor.id && "text-primary"
                                                        )}>
                                                            {actor.name}
                                                        </p>
                                                        {actor.description && (
                                                            <p className="text-sm text-muted-foreground line-clamp-2 mt-1">
                                                                {actor.description}
                                                            </p>
                                                        )}
                                                    </div>
                                                </button>
                                            ))
                                        )}
                                    </div>
                                </div>

                                {/* Character details - Only show if an actor is selected */}
                                {selectedActorId && (
                                    <div className="space-y-4 p-4 bg-muted/30 rounded-xl border-2 border-dashed">
                                        <h4 className="font-semibold text-base flex items-center gap-2">
                                            <Star className="h-4 w-4 text-primary" />
                                            Thông tin vai diễn
                                        </h4>

                                        {/* Character Name */}
                                        <div className="space-y-2">
                                            <Label htmlFor="characterName" className="text-sm font-medium">
                                                Tên nhân vật
                                            </Label>
                                            <Input
                                                id="characterName"
                                                placeholder="VD: Tony Stark, Bruce Wayne..."
                                                value={characterName}
                                                onChange={(e) => setCharacterName(e.target.value)}
                                                className="h-11"
                                            />
                                        </div>

                                        {/* Role */}
                                        <div className="space-y-2">
                                            <Label htmlFor="role" className="text-sm font-medium">
                                                Vai trò
                                            </Label>
                                            <Input
                                                id="role"
                                                placeholder="VD: Vai chính, Vai phụ, Khách mời..."
                                                value={role}
                                                onChange={(e) => setRole(e.target.value)}
                                                className="h-11"
                                            />
                                        </div>
                                    </div>
                                )}
                            </div>

                            <DialogFooter className="pt-4 border-t gap-2">
                                <Button
                                    variant="outline"
                                    onClick={() => {
                                        setIsDialogOpen(false);
                                        setSelectedActorId('');
                                        setCharacterName('');
                                        setRole('');
                                        setSearchQuery('');
                                    }}
                                    disabled={isAdding}
                                    className="min-w-[100px]"
                                >
                                    Hủy
                                </Button>
                                <Button
                                    onClick={handleAddActor}
                                    disabled={isAdding || !selectedActorId}
                                    className="min-w-[140px] shadow-md hover:shadow-lg transition-all"
                                >
                                    {isAdding ? (
                                        <>
                                            <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                                            Đang thêm...
                                        </>
                                    ) : (
                                        <>
                                            <Plus className="h-4 w-4 mr-2" />
                                            Thêm diễn viên
                                        </>
                                    )}
                                </Button>
                            </DialogFooter>
                        </DialogContent>
                    </Dialog>
                </div>
            </CardHeader>
            <CardContent className="p-6">
                {movieActors.length === 0 ? (
                    <div className="text-center py-16 px-4">
                        <div className="inline-flex items-center justify-center w-20 h-20 rounded-full bg-primary/10 mb-6">
                            <User2 className="h-10 w-10 text-primary" />
                        </div>
                        <h3 className="text-lg font-semibold mb-2">Chưa có diễn viên nào</h3>
                        <p className="text-sm text-muted-foreground mb-6 max-w-sm mx-auto">
                            Thêm diễn viên để hiển thị thông tin về dàn diễn viên tham gia phim này
                        </p>
                        <Button
                            onClick={() => setIsDialogOpen(true)}
                            variant="outline"
                            className="border-2 border-dashed"
                        >
                            <Plus className="h-4 w-4 mr-2" />
                            Thêm diễn viên đầu tiên
                        </Button>
                    </div>
                ) : (
                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                        {movieActors.map((ma) => (
                            <div
                                key={ma.id}
                                className="group relative flex flex-col gap-4 p-5 rounded-xl border-2 bg-gradient-to-br from-card to-card/50 hover:border-primary/40 hover:shadow-lg transition-all duration-300"
                            >
                                {/* Remove button */}
                                <Button
                                    variant="ghost"
                                    size="icon"
                                    className="absolute top-2 right-2 h-8 w-8 rounded-full opacity-0 group-hover:opacity-100 transition-opacity bg-background/80 backdrop-blur-sm text-destructive hover:text-destructive hover:bg-destructive/20 border border-destructive/20"
                                    onClick={() => handleRemoveActor(ma.id, ma.actorInfo?.name || 'diễn viên này')}
                                >
                                    <X className="h-4 w-4" />
                                </Button>

                                <div className="flex items-start gap-4">
                                    <Avatar className="h-16 w-16 ring-2 ring-primary/10 flex-shrink-0">
                                        <AvatarImage
                                            src={ma.actorInfo?.imageUrl}
                                            alt={ma.actorInfo?.name || 'Actor'}
                                            className="object-cover"
                                        />
                                        <AvatarFallback className="bg-gradient-to-br from-primary/20 to-primary/40">
                                            <User2 className="h-8 w-8 text-primary" />
                                        </AvatarFallback>
                                    </Avatar>

                                    <div className="flex-1 min-w-0 pt-1">
                                        <h4 className="font-bold text-base truncate mb-1">
                                            {ma.actorInfo?.name || 'Không có tên'}
                                        </h4>
                                        <div className="flex flex-wrap gap-1.5">
                                            {ma.characterName && (
                                                <Badge
                                                    variant="secondary"
                                                    className="text-xs px-2 py-0.5 font-medium bg-primary/10 text-primary border-primary/20"
                                                >
                                                    {ma.characterName}
                                                </Badge>
                                            )}
                                            {ma.role && (
                                                <Badge
                                                    variant="outline"
                                                    className="text-xs px-2 py-0.5 font-medium"
                                                >
                                                    {ma.role}
                                                </Badge>
                                            )}
                                        </div>
                                    </div>
                                </div>

                                {ma.actorInfo?.description && (
                                    <p className="text-xs text-muted-foreground line-clamp-2 pl-20 -mt-2">
                                        {ma.actorInfo.description}
                                    </p>
                                )}
                            </div>
                        ))}
                    </div>
                )}
            </CardContent>
        </Card>
    );
};
