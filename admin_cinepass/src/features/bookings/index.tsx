import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import {
    Search,
    Eye,
    Clock,
    CheckCircle2,
    XCircle,
    Ban,
    Filter,
    Calendar,
    User,
    Receipt,
    RefreshCw
} from "lucide-react";
import { toast } from "sonner";
import { format } from "date-fns";
import { vi } from "date-fns/locale";

// API Services
import { orderApi, type OrderResponseDto } from "@/services/apiOrder";

// Shadcn UI
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "@/components/ui/table";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "@/components/ui/tooltip";
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/select";

// Hooks
import { useDebounce } from "@/hooks/useDebounce";
import { cn } from "@/lib/utils";

// Order Status (strings from backend)
export const OrderStatus = {
    Pending: "Pending",
    Paid: "Paid",
    Confirmed: "Confirmed",
    Cancelled: "Cancelled",
    Expired: "Expired"
} as const;

// Status Labels - keyed by string
const statusConfig: Record<string, { label: string; color: string; bg: string; icon: any }> = {
    [OrderStatus.Pending]: {
        label: "Chờ thanh toán",
        color: "text-amber-600",
        bg: "bg-amber-100 border-amber-200 dark:bg-amber-900/30 dark:border-amber-800",
        icon: Clock
    },
    [OrderStatus.Paid]: {
        label: "Đã thanh toán",
        color: "text-blue-600",
        bg: "bg-blue-100 border-blue-200 dark:bg-blue-900/30 dark:border-blue-800",
        icon: CheckCircle2
    },
    [OrderStatus.Confirmed]: {
        label: "Đã xác nhận",
        color: "text-emerald-600",
        bg: "bg-emerald-100 border-emerald-200 dark:bg-emerald-900/30 dark:border-emerald-800",
        icon: CheckCircle2
    },
    [OrderStatus.Cancelled]: {
        label: "Đã hủy",
        color: "text-red-600",
        bg: "bg-red-100 border-red-200 dark:bg-red-900/30 dark:border-red-800",
        icon: XCircle
    },
    [OrderStatus.Expired]: {
        label: "Hết hạn",
        color: "text-slate-500",
        bg: "bg-slate-100 border-slate-200 dark:bg-slate-800 dark:border-slate-700",
        icon: Ban
    }
};

const defaultStatusConfig = {
    label: "Không rõ",
    color: "text-slate-500",
    bg: "bg-slate-100 border-slate-200",
    icon: Clock
};

const BookingsListPage = () => {
    const navigate = useNavigate();

    // State
    const [orders, setOrders] = useState<OrderResponseDto[]>([]);
    const [filteredOrders, setFilteredOrders] = useState<OrderResponseDto[]>([]);
    const [isLoading, setIsLoading] = useState(false);
    const [searchTerm, setSearchTerm] = useState("");
    const debouncedSearch = useDebounce(searchTerm, 500);
    const [statusFilter, setStatusFilter] = useState<string>("all");

    // Stats
    const [stats, setStats] = useState({
        total: 0,
        pending: 0,
        paid: 0,
        confirmed: 0,
        cancelled: 0,
        expired: 0,
        totalRevenue: 0
    });

    // Load orders
    useEffect(() => {
        loadOrders();
    }, []);

    // Filter orders when search or status changes
    useEffect(() => {
        filterOrders();
    }, [debouncedSearch, statusFilter, orders]);

    const loadOrders = async () => {
        try {
            setIsLoading(true);
            const data = await orderApi.getAll();
            setOrders(data);

            // Calculate stats (status is now string)
            const newStats = {
                total: data.length,
                pending: data.filter(o => o.status === OrderStatus.Pending).length,
                paid: data.filter(o => o.status === OrderStatus.Paid).length,
                confirmed: data.filter(o => o.status === OrderStatus.Confirmed).length,
                cancelled: data.filter(o => o.status === OrderStatus.Cancelled).length,
                expired: data.filter(o => o.status === OrderStatus.Expired).length,
                totalRevenue: data
                    .filter(o => o.status === OrderStatus.Confirmed || o.status === OrderStatus.Paid)
                    .reduce((sum, o) => sum + o.totalAmount, 0)
            };
            setStats(newStats);
        } catch (error) {
            console.error("Error loading orders:", error);
            toast.error("Lỗi khi tải danh sách đơn hàng");
        } finally {
            setIsLoading(false);
        }
    };

    const filterOrders = () => {
        let filtered = [...orders];

        // Filter by status (string-based)
        if (statusFilter !== "all") {
            filtered = filtered.filter(order => order.status === statusFilter);
        }

        // Filter by search term (order ID or userId)
        if (debouncedSearch) {
            filtered = filtered.filter(order =>
                order.id.toLowerCase().includes(debouncedSearch.toLowerCase()) ||
                (order.userId && order.userId.toLowerCase().includes(debouncedSearch.toLowerCase()))
            );
        }

        // Sort by date (newest first)
        filtered.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());

        setFilteredOrders(filtered);
    };

    const handleConfirmOrder = async (orderId: string) => {
        try {
            await orderApi.confirmOrder(orderId);
            toast.success("Xác nhận đơn hàng thành công");
            loadOrders();
        } catch (error: any) {
            toast.error(error.message || "Lỗi khi xác nhận đơn hàng");
        }
    };

    const handleCancelOrder = async (orderId: string) => {
        try {
            await orderApi.cancelOrder(orderId);
            toast.success("Hủy đơn hàng thành công");
            loadOrders();
        } catch (error: any) {
            toast.error(error.message || "Lỗi khi hủy đơn hàng");
        }
    };

    // Format price
    const formatPrice = (price: number) => {
        return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: 'VND'
        }).format(price);
    };

    // Format date - handle null/undefined safely
    const formatDate = (dateString: string | null | undefined) => {
        if (!dateString) return "N/A";
        try {
            return format(new Date(dateString), "dd/MM/yyyy HH:mm", { locale: vi });
        } catch {
            return "N/A";
        }
    };

    return (
        <div className="space-y-6 p-1">
            {/* Header Section */}
            <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
                <div>
                    <h1 className="text-3xl font-bold tracking-tight bg-linear-to-r from-primary to-violet-600 bg-clip-text text-transparent">
                        Quản lý Đơn hàng
                    </h1>
                    <p className="text-muted-foreground mt-1 text-sm">
                        Quản lý các đơn đặt vé và sản phẩm
                    </p>
                </div>
                <Button
                    onClick={loadOrders}
                    variant="outline"
                    className="gap-2"
                    disabled={isLoading}
                >
                    <RefreshCw className={cn("h-4 w-4", isLoading && "animate-spin")} />
                    Làm mới
                </Button>
            </div>

            {/* Stats Cards */}
            <div className="grid grid-cols-2 md:grid-cols-4 lg:grid-cols-6 gap-4">
                <Card className="border-none shadow-sm bg-card/50">
                    <CardContent className="p-4">
                        <div className="flex items-center gap-3">
                            <div className="h-10 w-10 rounded-lg bg-primary/10 flex items-center justify-center">
                                <Receipt className="h-5 w-5 text-primary" />
                            </div>
                            <div>
                                <p className="text-2xl font-bold">{stats.total}</p>
                                <p className="text-xs text-muted-foreground">Tổng đơn</p>
                            </div>
                        </div>
                    </CardContent>
                </Card>

                <Card className="border-none shadow-sm bg-card/50">
                    <CardContent className="p-4">
                        <div className="flex items-center gap-3">
                            <div className="h-10 w-10 rounded-lg bg-amber-100 dark:bg-amber-900/30 flex items-center justify-center">
                                <Clock className="h-5 w-5 text-amber-600" />
                            </div>
                            <div>
                                <p className="text-2xl font-bold">{stats.pending}</p>
                                <p className="text-xs text-muted-foreground">Chờ TT</p>
                            </div>
                        </div>
                    </CardContent>
                </Card>

                <Card className="border-none shadow-sm bg-card/50">
                    <CardContent className="p-4">
                        <div className="flex items-center gap-3">
                            <div className="h-10 w-10 rounded-lg bg-blue-100 dark:bg-blue-900/30 flex items-center justify-center">
                                <CheckCircle2 className="h-5 w-5 text-blue-600" />
                            </div>
                            <div>
                                <p className="text-2xl font-bold">{stats.paid}</p>
                                <p className="text-xs text-muted-foreground">Đã TT</p>
                            </div>
                        </div>
                    </CardContent>
                </Card>

                <Card className="border-none shadow-sm bg-card/50">
                    <CardContent className="p-4">
                        <div className="flex items-center gap-3">
                            <div className="h-10 w-10 rounded-lg bg-emerald-100 dark:bg-emerald-900/30 flex items-center justify-center">
                                <CheckCircle2 className="h-5 w-5 text-emerald-600" />
                            </div>
                            <div>
                                <p className="text-2xl font-bold">{stats.confirmed}</p>
                                <p className="text-xs text-muted-foreground">Đã XN</p>
                            </div>
                        </div>
                    </CardContent>
                </Card>

                <Card className="border-none shadow-sm bg-card/50">
                    <CardContent className="p-4">
                        <div className="flex items-center gap-3">
                            <div className="h-10 w-10 rounded-lg bg-red-100 dark:bg-red-900/30 flex items-center justify-center">
                                <XCircle className="h-5 w-5 text-red-600" />
                            </div>
                            <div>
                                <p className="text-2xl font-bold">{stats.cancelled + stats.expired}</p>
                                <p className="text-xs text-muted-foreground">Hủy/Hết hạn</p>
                            </div>
                        </div>
                    </CardContent>
                </Card>

                <Card className="border-none shadow-sm bg-linear-to-br from-emerald-500 to-emerald-600 text-white">
                    <CardContent className="p-4">
                        <div>
                            <p className="text-xs opacity-80">Doanh thu</p>
                            <p className="text-lg font-bold">{formatPrice(stats.totalRevenue)}</p>
                        </div>
                    </CardContent>
                </Card>
            </div>

            {/* Main Content */}
            <Card className="border-none shadow-md bg-card/50 backdrop-blur-sm">
                <CardHeader className="pb-3">
                    <div className="flex flex-col md:flex-row items-start md:items-center justify-between gap-4">
                        <CardTitle className="text-xl">Danh sách đơn hàng</CardTitle>
                        <div className="flex flex-col sm:flex-row gap-3 w-full md:w-auto">
                            {/* Status Filter */}
                            <div className="flex items-center gap-2">
                                <Filter className="h-4 w-4 text-muted-foreground" />
                                <Select value={statusFilter} onValueChange={setStatusFilter}>
                                    <SelectTrigger className="w-[160px] bg-background">
                                        <SelectValue placeholder="Trạng thái" />
                                    </SelectTrigger>
                                    <SelectContent>
                                        <SelectItem value="all">Tất cả</SelectItem>
                                        <SelectItem value="Pending">Chờ thanh toán</SelectItem>
                                        <SelectItem value="Paid">Đã thanh toán</SelectItem>
                                        <SelectItem value="Confirmed">Đã xác nhận</SelectItem>
                                        <SelectItem value="Cancelled">Đã hủy</SelectItem>
                                        <SelectItem value="Expired">Hết hạn</SelectItem>
                                    </SelectContent>
                                </Select>
                            </div>
                            {/* Search */}
                            <div className="relative w-full sm:w-80">
                                <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                                <Input
                                    placeholder="Tìm theo mã đơn hoặc user ID..."
                                    value={searchTerm}
                                    onChange={(e) => setSearchTerm(e.target.value)}
                                    className="pl-10 bg-background"
                                />
                            </div>
                        </div>
                    </div>
                </CardHeader>

                <CardContent className="p-0">
                    <div className="rounded-md border bg-background">
                        <Table>
                            <TableHeader>
                                <TableRow className="bg-muted/50 hover:bg-muted/50">
                                    <TableHead className="w-[200px]">Mã đơn hàng</TableHead>
                                    <TableHead>Khách hàng</TableHead>
                                    <TableHead className="text-right">Tổng tiền</TableHead>
                                    <TableHead className="text-center">Trạng thái</TableHead>
                                    <TableHead>Ngày tạo</TableHead>
                                    <TableHead>Hết hạn</TableHead>
                                    <TableHead className="text-right pr-6">Hành động</TableHead>
                                </TableRow>
                            </TableHeader>
                            <TableBody>
                                {isLoading ? (
                                    <TableRow>
                                        <TableCell colSpan={7} className="h-32 text-center text-muted-foreground">
                                            <div className="flex items-center justify-center gap-2">
                                                <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-primary"></div>
                                                Đang tải dữ liệu...
                                            </div>
                                        </TableCell>
                                    </TableRow>
                                ) : filteredOrders.length === 0 ? (
                                    <TableRow>
                                        <TableCell colSpan={7} className="h-48 text-center">
                                            <div className="flex flex-col items-center justify-center gap-2">
                                                <div className="h-12 w-12 rounded-full bg-muted flex items-center justify-center">
                                                    <Receipt className="h-6 w-6 text-muted-foreground/50" />
                                                </div>
                                                <p className="text-muted-foreground font-medium">Chưa có đơn hàng nào</p>
                                                <p className="text-xs text-muted-foreground">Đơn hàng sẽ xuất hiện khi khách hàng đặt vé.</p>
                                            </div>
                                        </TableCell>
                                    </TableRow>
                                ) : (
                                    filteredOrders.map((order) => {
                                        const status = statusConfig[order.status] || defaultStatusConfig;
                                        const StatusIcon = status.icon;

                                        return (
                                            <TableRow key={order.id} className="group hover:bg-muted/30 transition-colors">
                                                {/* Order ID */}
                                                <TableCell>
                                                    <div className="flex items-center gap-2">
                                                        <div className="h-8 w-8 rounded-lg bg-primary/10 flex items-center justify-center">
                                                            <Receipt className="h-4 w-4 text-primary" />
                                                        </div>
                                                        <code className="text-xs font-mono bg-muted px-2 py-1 rounded">
                                                            {order.id.slice(0, 8)}...
                                                        </code>
                                                    </div>
                                                </TableCell>

                                                {/* User */}
                                                <TableCell>
                                                    <div className="flex items-center gap-2">
                                                        <User className="h-4 w-4 text-muted-foreground" />
                                                        <span className="text-sm font-mono">
                                                            {order.userId ? `${order.userId.slice(0, 8)}...` : "Khách vãng lai"}
                                                        </span>
                                                    </div>
                                                </TableCell>

                                                {/* Total Amount */}
                                                <TableCell className="text-right">
                                                    <span className="font-semibold text-emerald-600 dark:text-emerald-400">
                                                        {formatPrice(order.totalAmount)}
                                                    </span>
                                                </TableCell>

                                                {/* Status */}
                                                <TableCell className="text-center">
                                                    <Badge className={cn("gap-1", status.bg, status.color)}>
                                                        <StatusIcon className="h-3 w-3" />
                                                        {status.label}
                                                    </Badge>
                                                </TableCell>

                                                {/* Created At */}
                                                <TableCell>
                                                    <div className="flex items-center gap-1 text-sm text-muted-foreground">
                                                        <Calendar className="h-3 w-3" />
                                                        {formatDate(order.createdAt)}
                                                    </div>
                                                </TableCell>

                                                {/* Expires At */}
                                                <TableCell>
                                                    <div className="flex items-center gap-1 text-sm text-muted-foreground">
                                                        <Clock className="h-3 w-3" />
                                                        {formatDate(order.expireAt)}
                                                    </div>
                                                </TableCell>

                                                {/* Actions */}
                                                <TableCell className="text-right pr-6">
                                                    <div className="flex items-center justify-end gap-1">
                                                        <TooltipProvider>
                                                            <Tooltip>
                                                                <TooltipTrigger asChild>
                                                                    <Button
                                                                        variant="ghost"
                                                                        size="icon"
                                                                        className="h-8 w-8 text-muted-foreground hover:text-primary hover:bg-primary/10"
                                                                        onClick={() => navigate(`/bookings/${order.id}`)}
                                                                    >
                                                                        <Eye className="h-4 w-4" />
                                                                    </Button>
                                                                </TooltipTrigger>
                                                                <TooltipContent>
                                                                    <p>Xem chi tiết</p>
                                                                </TooltipContent>
                                                            </Tooltip>
                                                        </TooltipProvider>

                                                        {order.status === OrderStatus.Paid && (
                                                            <TooltipProvider>
                                                                <Tooltip>
                                                                    <TooltipTrigger asChild>
                                                                        <Button
                                                                            variant="ghost"
                                                                            size="icon"
                                                                            className="h-8 w-8 text-muted-foreground hover:text-emerald-600 hover:bg-emerald-50"
                                                                            onClick={() => handleConfirmOrder(order.id)}
                                                                        >
                                                                            <CheckCircle2 className="h-4 w-4" />
                                                                        </Button>
                                                                    </TooltipTrigger>
                                                                    <TooltipContent>
                                                                        <p>Xác nhận đơn hàng</p>
                                                                    </TooltipContent>
                                                                </Tooltip>
                                                            </TooltipProvider>
                                                        )}

                                                        {(order.status === OrderStatus.Pending || order.status === OrderStatus.Paid) && (
                                                            <TooltipProvider>
                                                                <Tooltip>
                                                                    <TooltipTrigger asChild>
                                                                        <Button
                                                                            variant="ghost"
                                                                            size="icon"
                                                                            className="h-8 w-8 text-muted-foreground hover:text-red-600 hover:bg-red-50"
                                                                            onClick={() => handleCancelOrder(order.id)}
                                                                        >
                                                                            <XCircle className="h-4 w-4" />
                                                                        </Button>
                                                                    </TooltipTrigger>
                                                                    <TooltipContent>
                                                                        <p>Hủy đơn hàng</p>
                                                                    </TooltipContent>
                                                                </Tooltip>
                                                            </TooltipProvider>
                                                        )}
                                                    </div>
                                                </TableCell>
                                            </TableRow>
                                        );
                                    })
                                )}
                            </TableBody>
                        </Table>
                    </div>
                </CardContent>
            </Card>
        </div>
    );
};

export default BookingsListPage;
