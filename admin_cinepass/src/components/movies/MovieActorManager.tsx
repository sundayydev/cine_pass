import { useState, useEffect } from 'react';
import { Plus, X, Loader2, User2, Search } from 'lucide-react';
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
            <Card>
                <CardHeader>
                    <CardTitle>Diễn viên</CardTitle>
                </CardHeader>
                <CardContent>
                    <div className="flex items-center justify-center py-8">
                        <Loader2 className="h-6 w-6 animate-spin text-primary" />
                    </div>
                </CardContent>
            </Card>
        );
    }

    return (
        <Card>
            <CardHeader>
                <div className="flex items-center justify-between">
                    <div>
                        <CardTitle>Diễn viên</CardTitle>
                        <CardDescription>
                            Quản lý danh sách diễn viên tham gia phim
                        </CardDescription>
                    </div>
                    <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
                        <DialogTrigger asChild>
                            <Button size="sm">
                                <Plus className="h-4 w-4 mr-2" />
                                Thêm diễn viên
                            </Button>
                        </DialogTrigger>
                        <DialogContent className="max-w-2xl max-h-[80vh] overflow-y-auto">
                            <DialogHeader>
                                <DialogTitle>Thêm diễn viên vào phim</DialogTitle>
                                <DialogDescription>
                                    Chọn diễn viên và nhập thông tin vai diễn
                                </DialogDescription>
                            </DialogHeader>

                            <div className="space-y-4 py-4">
                                {/* Search actors */}
                                <div className="space-y-2">
                                    <Label>Tìm kiếm diễn viên</Label>
                                    <div className="relative">
                                        <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                                        <Input
                                            placeholder="Nhập tên diễn viên..."
                                            value={searchQuery}
                                            onChange={(e) => setSearchQuery(e.target.value)}
                                            className="pl-9"
                                        />
                                    </div>
                                </div>

                                {/* Actor selection */}
                                <div className="space-y-2">
                                    <Label>Chọn diễn viên *</Label>
                                    <div className="grid grid-cols-1 gap-2 max-h-60 overflow-y-auto border rounded-md p-2">
                                        {filteredActors.length === 0 ? (
                                            <p className="text-sm text-muted-foreground text-center py-4">
                                                {searchQuery
                                                    ? 'Không tìm thấy diễn viên phù hợp'
                                                    : 'Tất cả diễn viên đã được thêm vào phim'}
                                            </p>
                                        ) : (
                                            filteredActors.map((actor) => (
                                                <button
                                                    key={actor.id}
                                                    type="button"
                                                    onClick={() => setSelectedActorId(actor.id)}
                                                    className={`flex items-center gap-3 p-3 rounded-md border transition-colors text-left ${selectedActorId === actor.id
                                                        ? 'border-primary bg-primary/5'
                                                        : 'border-border hover:border-primary/50'
                                                        }`}
                                                >
                                                    <Avatar className="h-10 w-10">
                                                        <AvatarImage src={actor.imageUrl} alt={actor.name} />
                                                        <AvatarFallback>
                                                            <User2 className="h-5 w-5" />
                                                        </AvatarFallback>
                                                    </Avatar>
                                                    <div className="flex-1 min-w-0">
                                                        <p className="font-medium truncate">{actor.name}</p>
                                                        {actor.description && (
                                                            <p className="text-xs text-muted-foreground truncate">
                                                                {actor.description}
                                                            </p>
                                                        )}
                                                    </div>
                                                </button>
                                            ))
                                        )}
                                    </div>
                                </div>

                                {/* Character Name */}
                                <div className="space-y-2">
                                    <Label htmlFor="characterName">Tên nhân vật</Label>
                                    <Input
                                        id="characterName"
                                        placeholder="VD: Tony Stark"
                                        value={characterName}
                                        onChange={(e) => setCharacterName(e.target.value)}
                                    />
                                </div>

                                {/* Role */}
                                <div className="space-y-2">
                                    <Label htmlFor="role">Vai trò</Label>
                                    <Input
                                        id="role"
                                        placeholder="VD: Vai chính, Vai phụ, Khách mời..."
                                        value={role}
                                        onChange={(e) => setRole(e.target.value)}
                                    />
                                </div>
                            </div>

                            <DialogFooter>
                                <Button
                                    variant="outline"
                                    onClick={() => setIsDialogOpen(false)}
                                    disabled={isAdding}
                                >
                                    Hủy
                                </Button>
                                <Button onClick={handleAddActor} disabled={isAdding || !selectedActorId}>
                                    {isAdding ? (
                                        <>
                                            <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                                            Đang thêm...
                                        </>
                                    ) : (
                                        'Thêm diễn viên'
                                    )}
                                </Button>
                            </DialogFooter>
                        </DialogContent>
                    </Dialog>
                </div>
            </CardHeader>
            <CardContent>
                {movieActors.length === 0 ? (
                    <div className="text-center py-8 text-muted-foreground">
                        <User2 className="h-12 w-12 mx-auto mb-2 opacity-50" />
                        <p className="text-sm">Chưa có diễn viên nào</p>
                        <p className="text-xs">Nhấn "Thêm diễn viên" để bắt đầu</p>
                    </div>
                ) : (
                    <div className="space-y-3">
                        {movieActors.map((ma) => (
                            <div
                                key={ma.id}
                                className="flex items-center gap-3 p-3 rounded-lg border bg-card hover:bg-accent/50 transition-colors"
                            >
                                <Avatar className="h-12 w-12">
                                    <AvatarImage
                                        src={ma.actorInfo?.imageUrl}
                                        alt={ma.actorInfo?.name || 'Actor'}
                                    />
                                    <AvatarFallback>
                                        <User2 className="h-6 w-6" />
                                    </AvatarFallback>
                                </Avatar>

                                <div className="flex-1 min-w-0">
                                    <h4 className="font-medium truncate">
                                        {ma.actorInfo?.name || 'Không có tên'}
                                    </h4>
                                    <div className="flex flex-wrap gap-2 mt-1">
                                        {ma.characterName && (
                                            <Badge variant="secondary" className="text-xs">
                                                {ma.characterName}
                                            </Badge>
                                        )}
                                        {ma.role && (
                                            <Badge variant="outline" className="text-xs">
                                                {ma.role}
                                            </Badge>
                                        )}
                                    </div>
                                </div>

                                <Button
                                    variant="ghost"
                                    size="icon"
                                    className="text-destructive hover:text-destructive hover:bg-destructive/10"
                                    onClick={() => handleRemoveActor(ma.id, ma.actorInfo?.name || 'diễn viên này')}
                                >
                                    <X className="h-4 w-4" />
                                </Button>
                            </div>
                        ))}
                    </div>
                )}
            </CardContent>
        </Card>
    );
};
