import { useState, useEffect } from "react";
import { useNavigate, useParams } from "react-router-dom";
import {
    ArrowLeft,
    Loader2,
    Ticket,
    Package,
    Clock,
    CheckCircle2,
    XCircle,
    Ban,
    Calendar,
    User,
    CreditCard,
    MapPin,
    Film,
    Copy,
    Check,
    Mail,
    Phone,
    Armchair,
    Monitor,
    Popcorn,
    Printer,
    Building2
} from "lucide-react";
import { toast } from "sonner";
import { format } from "date-fns";
import { vi } from "date-fns/locale";
import QRCode from "react-qr-code";

// API Services
import { orderApi, type OrderDetailDto, type OrderTicketDetail, type OrderProductDetail } from "@/services/apiOrder";

// Shadcn UI
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Separator } from "@/components/ui/separator";
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle } from "@/components/ui/alert-dialog";

import { cn } from "@/lib/utils";

// Order Status (strings from backend)
export const OrderStatus = {
    Pending: "Pending",
    Paid: "Paid",
    Confirmed: "Confirmed",
    Cancelled: "Cancelled",
    Expired: "Expired"
} as const;

// Status Config
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

// Ticket Card Component
const TicketCard = ({ ticket, index }: { ticket: OrderTicketDetail; index: number }) => {
    const movieTitle = ticket.showtime?.movie?.title || "Phim";
    const posterUrl = ticket.showtime?.movie?.posterUrl;
    const screenName = ticket.showtime?.screen?.name || "Phòng chiếu";
    const cinemaName = ticket.showtime?.screen?.cinemaName || "Rạp chiếu phim";
    const seatCode = ticket.seat?.seatCode || "N/A";
    const seatType = ticket.seat?.seatTypeCode || "NORMAL";
    const showtime = ticket.showtime?.startTime;
    const qrData = ticket.eTicket?.ticketCode || ticket.eTicket?.qrData || ticket.id;
    const ticketCode = ticket.eTicket?.ticketCode || "N/A";
    const isUsed = ticket.eTicket?.isUsed || false;
    const price = ticket.price || 0;

    const formatPrice = (price: number) => {
        return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: 'VND'
        }).format(price);
    };

    const formatDate = (dateString: string | null | undefined) => {
        if (!dateString) return "N/A";
        try {
            return format(new Date(dateString), "HH:mm - dd/MM/yyyy", { locale: vi });
        } catch {
            return "N/A";
        }
    };

    const getSeatTypeLabel = (type: string) => {
        const upper = type?.toUpperCase() || "";
        if (upper.includes("VIP")) return { label: "VIP", color: "bg-amber-500" };
        if (upper.includes("COUPLE")) return { label: "Đôi", color: "bg-pink-500" };
        return { label: "Thường", color: "bg-sky-500" };
    };

    const seatTypeInfo = getSeatTypeLabel(seatType);

    return (
        <div className={cn(
            "relative overflow-hidden rounded-2xl shadow-lg",
            isUsed ? "opacity-70" : ""
        )}>
            {/* Ticket Container with Cut Design */}
            <div className="flex">
                {/* Left Part - Movie Info */}
                <div className="flex-1 bg-gradient-to-br from-slate-900 via-slate-800 to-slate-900 text-white p-5 relative">
                    {/* Movie Poster Background */}
                    {posterUrl && (
                        <div
                            className="absolute inset-0 opacity-20 bg-cover bg-center"
                            style={{ backgroundImage: `url(${posterUrl})` }}
                        />
                    )}

                    <div className="relative z-10">
                        {/* Movie Title */}
                        <div className="flex items-start gap-3 mb-4">
                            <div className="h-10 w-10 rounded-lg bg-primary/20 flex items-center justify-center shrink-0">
                                <Film className="h-5 w-5 text-primary" />
                            </div>
                            <div>
                                <h3 className="font-bold text-lg leading-tight">{movieTitle}</h3>
                                <p className="text-xs text-slate-400 mt-1">Vé #{index + 1}</p>
                            </div>
                        </div>

                        {/* Details Grid */}
                        <div className="grid grid-cols-2 gap-3 text-sm">
                            <div className="space-y-1 col-span-2">
                                <div className="flex items-center gap-2 text-slate-400">
                                    <Building2 className="h-3 w-3" />
                                    <span className="text-xs">Rạp chiếu</span>
                                </div>
                                <p className="font-semibold">{cinemaName}</p>
                            </div>
                            <div className="space-y-1">
                                <div className="flex items-center gap-2 text-slate-400">
                                    <Monitor className="h-3 w-3" />
                                    <span className="text-xs">Phòng chiếu</span>
                                </div>
                                <p className="font-semibold">{screenName}</p>
                            </div>
                            <div className="space-y-1">
                                <div className="flex items-center gap-2 text-slate-400">
                                    <Armchair className="h-3 w-3" />
                                    <span className="text-xs">Ghế ngồi</span>
                                </div>
                                <div className="flex items-center gap-2">
                                    <span className="font-bold text-xl">{seatCode}</span>
                                    <span className={cn("text-[10px] px-1.5 py-0.5 rounded text-white", seatTypeInfo.color)}>
                                        {seatTypeInfo.label}
                                    </span>
                                </div>
                            </div>
                            <div className="space-y-1">
                                <div className="flex items-center gap-2 text-slate-400">
                                    <Calendar className="h-3 w-3" />
                                    <span className="text-xs">Suất chiếu</span>
                                </div>
                                <p className="font-semibold">{formatDate(showtime)}</p>
                            </div>
                            <div className="space-y-1">
                                <div className="flex items-center gap-2 text-slate-400">
                                    <CreditCard className="h-3 w-3" />
                                    <span className="text-xs">Giá vé</span>
                                </div>
                                <p className="font-bold text-emerald-400">{formatPrice(price)}</p>
                            </div>
                        </div>

                        {/* Ticket Code */}
                        <div className="mt-4 pt-3 border-t border-slate-700">
                            <div className="flex items-center justify-between">
                                <code className="text-xs text-slate-400 font-mono">{ticketCode}</code>
                                {isUsed && (
                                    <Badge className="bg-red-500/20 text-red-400 border-red-500/30">
                                        Đã sử dụng
                                    </Badge>
                                )}
                            </div>
                        </div>
                    </div>

                    {/* Decorative circles for cut effect */}
                    <div className="absolute -right-3 top-1/2 -translate-y-1/2 w-6 h-6 bg-white dark:bg-slate-950 rounded-full" />
                </div>

                {/* Right Part - QR Code */}
                <div className="w-36 bg-white flex flex-col items-center justify-center p-4 relative">
                    {/* Decorative circle */}
                    <div className="absolute -left-3 top-1/2 -translate-y-1/2 w-6 h-6 bg-white dark:bg-slate-950 rounded-full" />

                    {/* Dashed line */}
                    <div className="absolute left-0 top-4 bottom-4 border-l-2 border-dashed border-slate-200" />

                    <div className="relative z-10 flex flex-col items-center">
                        {/* QR Code */}
                        <div className={cn("p-2 bg-white rounded-lg shadow-inner", isUsed && "opacity-50")}>
                            <QRCode
                                value={qrData}
                                size={80}
                                level="M"
                                className={isUsed ? "grayscale" : ""}
                            />
                        </div>
                        <p className="text-[10px] text-slate-500 mt-2 text-center">Quét mã để check-in</p>

                        {isUsed && (
                            <div className="absolute inset-0 flex items-center justify-center">
                                <div className="bg-red-500/90 text-white text-xs font-bold px-3 py-1 rounded-full rotate-[-15deg] shadow-lg">
                                    ĐÃ DÙNG
                                </div>
                            </div>
                        )}
                    </div>
                </div>
            </div>
        </div>
    );
};

// Product Item Component
const ProductItem = ({ item }: { item: OrderProductDetail }) => {
    const formatPrice = (price: number) => {
        return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: 'VND'
        }).format(price);
    };

    return (
        <div className="flex items-center justify-between p-4 rounded-xl bg-gradient-to-r from-orange-50 to-amber-50 dark:from-orange-950/20 dark:to-amber-950/20 border border-orange-100 dark:border-orange-900/30">
            <div className="flex items-center gap-4">
                {item.product?.imageUrl ? (
                    <img
                        src={item.product.imageUrl}
                        alt={item.product.name}
                        className="w-14 h-14 rounded-lg object-cover shadow-sm"
                    />
                ) : (
                    <div className="w-14 h-14 rounded-lg bg-gradient-to-br from-orange-400 to-amber-500 flex items-center justify-center shadow-sm">
                        <Popcorn className="h-6 w-6 text-white" />
                    </div>
                )}
                <div>
                    <p className="font-semibold text-slate-900 dark:text-white">{item.product?.name || "Sản phẩm"}</p>
                    <div className="flex items-center gap-2 text-sm text-muted-foreground">
                        <span>{formatPrice(item.unitPrice)}</span>
                        <span>×</span>
                        <span className="font-medium">{item.quantity}</span>
                    </div>
                </div>
            </div>
            <div className="text-right">
                <p className="font-bold text-lg text-emerald-600 dark:text-emerald-400">
                    {formatPrice(item.unitPrice * item.quantity)}
                </p>
            </div>
        </div>
    );
};

const BookingDetailPage = () => {
    const navigate = useNavigate();
    const { id } = useParams<{ id: string }>();
    const [order, setOrder] = useState<OrderDetailDto | null>(null);
    const [isLoading, setIsLoading] = useState(true);
    const [isConfirming, setIsConfirming] = useState(false);
    const [isCancelling, setIsCancelling] = useState(false);
    const [showCancelDialog, setShowCancelDialog] = useState(false);
    const [copiedId, setCopiedId] = useState<string | null>(null);

    // Load order detail
    useEffect(() => {
        if (id) {
            loadOrderDetail(id);
        }
    }, [id]);

    const loadOrderDetail = async (orderId: string) => {
        try {
            setIsLoading(true);
            const data = await orderApi.getDetailById(orderId);
            setOrder(data);
        } catch (error) {
            console.error("Error loading order:", error);
            toast.error("Lỗi khi tải thông tin đơn hàng");
            navigate("/bookings");
        } finally {
            setIsLoading(false);
        }
    };

    const handleConfirmOrder = async () => {
        if (!id) return;
        try {
            setIsConfirming(true);
            await orderApi.confirmOrder(id);
            toast.success("Xác nhận đơn hàng thành công");
            loadOrderDetail(id);
        } catch (error: any) {
            toast.error(error.message || "Lỗi khi xác nhận đơn hàng");
        } finally {
            setIsConfirming(false);
        }
    };

    const handleCancelOrder = async () => {
        if (!id) return;
        try {
            setIsCancelling(true);
            await orderApi.cancelOrder(id);
            toast.success("Hủy đơn hàng thành công");
            setShowCancelDialog(false);
            loadOrderDetail(id);
        } catch (error: any) {
            toast.error(error.message || "Lỗi khi hủy đơn hàng");
        } finally {
            setIsCancelling(false);
        }
    };

    const copyToClipboard = (text: string, id: string) => {
        navigator.clipboard.writeText(text);
        setCopiedId(id);
        toast.success("Đã sao chép!");
        setTimeout(() => setCopiedId(null), 2000);
    };

    const handlePrint = () => {
        window.print();
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

    if (isLoading) {
        return (
            <div className="flex items-center justify-center min-h-[60vh]">
                <div className="flex flex-col items-center gap-4">
                    <Loader2 className="h-8 w-8 animate-spin text-primary" />
                    <p className="text-muted-foreground">Đang tải thông tin đơn hàng...</p>
                </div>
            </div>
        );
    }

    if (!order) {
        return (
            <div className="flex items-center justify-center min-h-[60vh]">
                <p className="text-muted-foreground">Không tìm thấy đơn hàng</p>
            </div>
        );
    }

    const status = statusConfig[order.status] || defaultStatusConfig;
    const StatusIcon = status.icon;

    // Calculate totals
    const ticketTotal = order.tickets?.reduce((sum, t) => sum + (t.price || 0), 0) || 0;
    const productTotal = order.products?.reduce((sum, p) => sum + ((p.unitPrice || 0) * p.quantity), 0) || 0;

    return (
        <div className="space-y-6 pb-10">
            {/* Print Styles */}
            <style>
                {`
                    @media print {
                        /* Hide everything except the main content */
                        body * {
                            visibility: hidden;
                        }
                        
                        /* Show only the print area and its children */
                        .print-area,
                        .print-area * {
                            visibility: visible;
                        }
                        
                        /* Position print area at top of page */
                        .print-area {
                            position: absolute;
                            left: 0;
                            top: 0;
                            width: 100%;
                            background: white;
                            padding: 1rem;
                        }
                        
                        /* Show print header */
                        .print\\:block {
                            display: block !important;
                        }
                        
                        /* Hide navigation, buttons, and sidebar */
                        .no-print,
                        button,
                        header,
                        nav,
                        .sidebar,
                        .alert-dialog {
                            display: none !important;
                        }
                        
                        /* Optimize ticket cards for print */
                        .ticket-print {
                            page-break-inside: avoid;
                            margin-bottom: 1.5rem;
                            break-inside: avoid;
                        }
                        
                        /* Ensure QR codes are visible */
                        canvas, svg {
                            visibility: visible !important;
                        }
                        
                        /* Preserve colors and backgrounds when printing */
                        * {
                            -webkit-print-color-adjust: exact;
                            print-color-adjust: exact;
                            color-adjust: exact;
                        }
                        
                        /* Remove shadows for cleaner print */
                        .shadow-md, .shadow-lg, .shadow-sm {
                            box-shadow: none !important;
                        }
                        
                        /* Add borders to cards for definition */
                        .border-none {
                            border: 1px solid #e5e7eb !important;
                        }
                        
                        /* Optimize page breaks */
                        @page {
                            margin: 1cm;
                        }
                    }
                `}
            </style>

            {/* Header */}
            <div className="flex items-center justify-between no-print">
                <div className="flex items-center gap-4">
                    <Button variant="ghost" size="icon" onClick={() => navigate("/bookings")}>
                        <ArrowLeft className="h-4 w-4" />
                    </Button>
                    <div>
                        <div className="flex items-center gap-3">
                            <h1 className="text-2xl font-bold tracking-tight">Chi tiết đơn hàng</h1>
                            <Badge className={cn("gap-1", status.bg, status.color)}>
                                <StatusIcon className="h-3 w-3" />
                                {status.label}
                            </Badge>
                        </div>
                        <p className="text-muted-foreground text-sm mt-1 flex items-center gap-2">
                            <code className="bg-muted px-2 py-0.5 rounded text-xs">{order.id}</code>
                            <Button
                                variant="ghost"
                                size="icon"
                                className="h-6 w-6"
                                onClick={() => copyToClipboard(order.id, "order-id")}
                            >
                                {copiedId === "order-id" ? (
                                    <Check className="h-3 w-3 text-emerald-500" />
                                ) : (
                                    <Copy className="h-3 w-3" />
                                )}
                            </Button>
                        </p>
                    </div>
                </div>

                {/* Action Buttons */}
                <div className="flex items-center gap-2">
                    {(order.status === OrderStatus.Paid || order.status === OrderStatus.Confirmed) && (
                        <Button
                            onClick={handlePrint}
                            variant="outline"
                            className="border-primary text-primary hover:bg-primary hover:text-white"
                        >
                            <Printer className="mr-2 h-4 w-4" />
                            In vé
                        </Button>
                    )}

                    {order.status === OrderStatus.Paid && (
                        <Button
                            onClick={handleConfirmOrder}
                            disabled={isConfirming}
                            className="bg-emerald-600 hover:bg-emerald-700"
                        >
                            {isConfirming && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                            <CheckCircle2 className="mr-2 h-4 w-4" />
                            Xác nhận đơn hàng
                        </Button>
                    )}

                    {(order.status === OrderStatus.Pending || order.status === OrderStatus.Paid) && (
                        <Button
                            variant="destructive"
                            onClick={() => setShowCancelDialog(true)}
                        >
                            <XCircle className="mr-2 h-4 w-4" />
                            Hủy đơn hàng
                        </Button>
                    )}
                </div>
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                {/* Main Content */}
                <div className="lg:col-span-2 space-y-6 print-area">
                    {/* Print Header - Only visible when printing */}
                    <div className="hidden print:block mb-6">
                        <div className="text-center border-b-2 border-slate-900 pb-4 mb-6">
                            <h1 className="text-3xl font-bold text-slate-900 mb-2">
                                {order.tickets?.[0]?.showtime?.screen?.cinemaName || "CinePass"}
                            </h1>
                            <p className="text-lg font-semibold text-slate-700">VÉ XEM PHIM</p>
                            <div className="flex justify-between text-sm text-slate-600 mt-2">
                                <span>Mã đơn hàng: <strong>{order.id}</strong></span>
                                <span>In lúc: <strong>{formatDate(new Date().toISOString())}</strong></span>
                            </div>
                        </div>
                    </div>

                    {/* Tickets Section */}
                    <Card className="border-none shadow-md">
                        <CardHeader className="pb-4">
                            <CardTitle className="flex items-center gap-2">
                                <div className="h-8 w-8 rounded-lg bg-primary/10 flex items-center justify-center">
                                    <Ticket className="h-4 w-4 text-primary" />
                                </div>
                                Vé đã đặt ({order.tickets?.length || 0} vé)
                            </CardTitle>
                        </CardHeader>
                        <CardContent className="space-y-4">
                            {order.tickets && order.tickets.length > 0 ? (
                                order.tickets.map((ticket, index) => (
                                    <div key={ticket.id || index} className="ticket-print">
                                        <TicketCard ticket={ticket} index={index} />
                                    </div>
                                ))
                            ) : (
                                <div className="text-center py-10 text-muted-foreground">
                                    <Ticket className="h-12 w-12 mx-auto mb-3 opacity-20" />
                                    <p>Chưa có vé nào</p>
                                </div>
                            )}
                        </CardContent>
                    </Card>

                    {/* Products Section */}
                    <Card className="border-none shadow-md">
                        <CardHeader className="pb-4">
                            <CardTitle className="flex items-center gap-2">
                                <div className="h-8 w-8 rounded-lg bg-orange-100 dark:bg-orange-900/30 flex items-center justify-center">
                                    <Package className="h-4 w-4 text-orange-500" />
                                </div>
                                Đồ ăn & Nước uống ({order.products?.length || 0} sản phẩm)
                            </CardTitle>
                        </CardHeader>
                        <CardContent className="space-y-3">
                            {order.products && order.products.length > 0 ? (
                                order.products.map((item, index) => (
                                    <ProductItem key={item.id || index} item={item} />
                                ))
                            ) : (
                                <div className="text-center py-10 text-muted-foreground">
                                    <Package className="h-12 w-12 mx-auto mb-3 opacity-20" />
                                    <p>Không có sản phẩm</p>
                                </div>
                            )}
                        </CardContent>
                    </Card>

                    {/* Print Footer - Only visible when printing */}
                    <div className="hidden print:block mt-8 pt-6 border-t-2 border-slate-900">
                        <div className="text-center space-y-2">
                            <p className="text-sm text-slate-700">
                                Vui lòng xuất trình mã QR này tại quầy vé hoặc cổng check-in
                            </p>
                            <p className="text-sm text-slate-700">
                                Mọi thắc mắc vui lòng liên hệ hotline: <strong>1900-xxxx</strong>
                            </p>
                            <p className="text-lg font-bold text-slate-900 mt-4">
                                CẢM ƠN QUÝ KHÁCH - CHÚC QUÝ KHÁCH XEM PHIM VUI VẺ!
                            </p>
                            <p className="text-xs text-slate-500 mt-2">
                                Powered by CinePass - Cinema Management System
                            </p>
                        </div>
                    </div>
                </div>

                {/* Sidebar */}
                <div className="space-y-6 no-print">
                    {/* Customer Info Card */}
                    {order.user && (
                        <Card className="border-none shadow-md bg-gradient-to-br from-blue-50 to-indigo-50 dark:from-blue-950/30 dark:to-indigo-950/30">
                            <CardHeader className="pb-3">
                                <CardTitle className="flex items-center gap-2 text-lg">
                                    <User className="h-4 w-4 text-blue-600" />
                                    Thông tin khách hàng
                                </CardTitle>
                            </CardHeader>
                            <CardContent className="space-y-3">
                                <div className="flex items-center gap-3">
                                    <div className="h-12 w-12 rounded-full bg-gradient-to-br from-blue-500 to-indigo-600 flex items-center justify-center text-white font-bold text-lg">
                                        {order.user.fullName?.charAt(0)?.toUpperCase() || order.user.email.charAt(0).toUpperCase()}
                                    </div>
                                    <div>
                                        <p className="font-semibold">{order.user.fullName || "Khách hàng"}</p>
                                        <p className="text-sm text-muted-foreground">ID: {order.user.id.slice(0, 8)}...</p>
                                    </div>
                                </div>
                                <Separator />
                                <div className="space-y-2">
                                    <div className="flex items-center gap-2 text-sm">
                                        <Mail className="h-4 w-4 text-muted-foreground" />
                                        <span>{order.user.email}</span>
                                    </div>
                                    {order.user.phone && (
                                        <div className="flex items-center gap-2 text-sm">
                                            <Phone className="h-4 w-4 text-muted-foreground" />
                                            <span>{order.user.phone}</span>
                                        </div>
                                    )}
                                </div>
                            </CardContent>
                        </Card>
                    )}

                    {/* Order Info Card */}
                    <Card className="border-none shadow-md">
                        <CardHeader className="pb-3">
                            <CardTitle className="flex items-center gap-2 text-lg">
                                <CreditCard className="h-4 w-4" />
                                Thông tin đơn hàng
                            </CardTitle>
                        </CardHeader>
                        <CardContent className="space-y-4">
                            {!order.user && (
                                <>
                                    <div className="flex items-center justify-between">
                                        <span className="text-muted-foreground flex items-center gap-2">
                                            <User className="h-4 w-4" />
                                            Khách hàng
                                        </span>
                                        <span className="text-sm">Khách vãng lai</span>
                                    </div>
                                    <Separator />
                                </>
                            )}

                            <div className="flex items-center justify-between">
                                <span className="text-muted-foreground flex items-center gap-2">
                                    <Calendar className="h-4 w-4" />
                                    Ngày tạo
                                </span>
                                <span className="text-sm font-medium">{formatDate(order.createdAt)}</span>
                            </div>

                            <div className="flex items-center justify-between">
                                <span className="text-muted-foreground flex items-center gap-2">
                                    <Clock className="h-4 w-4" />
                                    Hết hạn
                                </span>
                                <span className="text-sm">{formatDate(order.expireAt)}</span>
                            </div>

                            {order.paymentMethod && (
                                <>
                                    <Separator />
                                    <div className="flex items-center justify-between">
                                        <span className="text-muted-foreground">Phương thức TT</span>
                                        <Badge variant="outline">{order.paymentMethod}</Badge>
                                    </div>
                                </>
                            )}
                        </CardContent>
                    </Card>

                    {/* Payment Summary */}
                    <Card className="border-none shadow-lg bg-gradient-to-br from-emerald-500 to-emerald-600 text-white overflow-hidden relative">
                        {/* Decorative circles */}
                        <div className="absolute -top-10 -right-10 w-32 h-32 bg-white/10 rounded-full" />
                        <div className="absolute -bottom-5 -left-5 w-20 h-20 bg-white/10 rounded-full" />

                        <CardHeader className="relative z-10">
                            <CardTitle className="text-white text-lg">Tổng thanh toán</CardTitle>
                        </CardHeader>
                        <CardContent className="space-y-3 relative z-10">
                            <div className="flex justify-between text-emerald-100">
                                <span className="flex items-center gap-2">
                                    <Ticket className="h-4 w-4" />
                                    Vé ({order.tickets?.length || 0})
                                </span>
                                <span>{formatPrice(ticketTotal)}</span>
                            </div>
                            <div className="flex justify-between text-emerald-100">
                                <span className="flex items-center gap-2">
                                    <Package className="h-4 w-4" />
                                    Sản phẩm ({order.products?.length || 0})
                                </span>
                                <span>{formatPrice(productTotal)}</span>
                            </div>
                            <Separator className="bg-white/30" />
                            <div className="flex justify-between text-2xl font-bold">
                                <span>Tổng cộng</span>
                                <span>{formatPrice(order.totalAmount)}</span>
                            </div>
                        </CardContent>
                    </Card>
                </div>
            </div>

            {/* Cancel Dialog */}
            <AlertDialog open={showCancelDialog} onOpenChange={setShowCancelDialog}>
                <AlertDialogContent>
                    <AlertDialogHeader>
                        <AlertDialogTitle>Xác nhận hủy đơn hàng</AlertDialogTitle>
                        <AlertDialogDescription>
                            Bạn có chắc chắn muốn hủy đơn hàng này? Hành động này không thể hoàn tác.
                        </AlertDialogDescription>
                    </AlertDialogHeader>
                    <AlertDialogFooter>
                        <AlertDialogCancel>Không</AlertDialogCancel>
                        <AlertDialogAction
                            onClick={handleCancelOrder}
                            disabled={isCancelling}
                            className="bg-red-600 hover:bg-red-700"
                        >
                            {isCancelling && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                            Xác nhận hủy
                        </AlertDialogAction>
                    </AlertDialogFooter>
                </AlertDialogContent>
            </AlertDialog>
        </div>
    );
};

export default BookingDetailPage;
