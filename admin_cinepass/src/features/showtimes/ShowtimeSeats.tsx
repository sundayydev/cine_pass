import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { ArrowLeft, Monitor, Armchair, Clock, Calendar, Ticket, Users, ShoppingCart, X, CreditCard, Loader2, CheckCircle2, Package } from "lucide-react";
import { format, parseISO } from "date-fns";
import { vi } from "date-fns/locale";
import { cn } from "@/lib/utils";
import { toast } from "sonner";

// API Services
import {
    showtimeApi,
    type ShowtimeSeatsResponse,
    type SeatWithStatusDto,
    SeatStatus
} from "@/services/apiShowtime";
import { seatTypeApi, type SeatTypeResponseDto } from "@/services/apiSeatType";
import { productApi, type ProductResponseDto } from "@/services/apiProduct";
import { PATHS } from "@/config/paths";

// Shadcn UI
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Spinner } from "@/components/ui/spinner";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Separator } from "@/components/ui/separator";
import { Badge } from "@/components/ui/badge";
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogHeader,
    DialogTitle,
    DialogFooter,
} from "@/components/ui/dialog";
import {
    Sheet,
    SheetContent,
    SheetDescription,
    SheetHeader,
    SheetTitle,
    SheetFooter,
} from "@/components/ui/sheet";

// Types
interface SelectedSeat extends SeatWithStatusDto { }

interface ProductCartItem {
    product: ProductResponseDto;
    quantity: number;
}

interface PosOrderCreateDto {
    customerPhone: string;
    customerName: string;
    customerEmail?: string;
    tickets: { showtimeId: string; seatId: string }[];
    products: { productId: string; quantity: number }[];
    cashReceived: number;
    staffNote?: string;
}

// API for POS order
import axiosClient from "@/lib/axiosClient";

const createPosOrder = async (dto: PosOrderCreateDto): Promise<any> => {
    const response = await axiosClient.post('/api/staff/orders/pos-create', dto) as any;
    if (!response.success) {
        throw new Error(response.message || 'Lỗi khi tạo đơn hàng');
    }
    return response.data;
};

const ShowtimeSeatsPage = () => {
    const { showtimeId } = useParams<{ showtimeId: string }>();
    const navigate = useNavigate();

    // Data states
    const [seatsData, setSeatsData] = useState<ShowtimeSeatsResponse | null>(null);
    const [seatTypes, setSeatTypes] = useState<SeatTypeResponseDto[]>([]);
    const [products, setProducts] = useState<ProductResponseDto[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    // Selection states
    const [selectedSeats, setSelectedSeats] = useState<SelectedSeat[]>([]);
    const [productCart, setProductCart] = useState<ProductCartItem[]>([]);

    // UI states
    const [showCart, setShowCart] = useState(false);
    const [showCheckoutDialog, setShowCheckoutDialog] = useState(false);
    const [isCreatingOrder, setIsCreatingOrder] = useState(false);

    // Customer & Payment states
    const [customerName, setCustomerName] = useState("");
    const [customerPhone, setCustomerPhone] = useState("");
    const [customerEmail, setCustomerEmail] = useState("");
    const [cashReceived, setCashReceived] = useState<number>(0);
    const [staffNote, setStaffNote] = useState("");

    // --- HELPERS ---

    // Check if seat is Couple type
    const isCoupleSeat = (seatTypeCode?: string) => {
        return seatTypeCode?.toUpperCase().includes("COUPLE");
    };

    // Check if seat is selected
    const isSeatSelected = (seatId: string) => {
        return selectedSeats.some(s => s.id === seatId);
    };

    // Get seat styling based on status, type and selection
    const getSeatStyle = (seat: SeatWithStatusDto) => {
        // Selected state
        if (isSeatSelected(seat.id)) {
            return "bg-emerald-500 border-emerald-600 text-white shadow-emerald-200 ring-2 ring-emerald-400 ring-offset-2 cursor-pointer scale-110";
        }

        // Sold
        if (seat.status === SeatStatus.Sold) {
            return "bg-red-500 border-red-600 text-white shadow-red-200 cursor-not-allowed opacity-70";
        }

        // Holding
        if (seat.status === SeatStatus.Holding) {
            return "bg-orange-400 border-orange-500 text-white shadow-orange-200 cursor-not-allowed opacity-70";
        }

        // Available - color by type
        const code = seat.seatTypeCode?.toUpperCase() || "NORMAL";

        if (code.includes("VIP")) return "bg-amber-500 border-amber-600 text-white shadow-amber-200 cursor-pointer hover:scale-110 transition-transform";
        if (code.includes("COUPLE")) return "bg-pink-500 border-pink-600 text-white shadow-pink-200 cursor-pointer hover:scale-110 transition-transform";

        // Normal/Default - Available
        return "bg-sky-500 border-sky-600 text-white shadow-sky-200 cursor-pointer hover:scale-110 transition-transform";
    };

    const getSeatTypeName = (seatTypeCode?: string) => {
        if (!seatTypeCode) return "Thường";
        const seatType = seatTypes.find((t) => t.code === seatTypeCode);
        return seatType?.name || seatTypeCode;
    };

    const getStatusLabel = (status: SeatStatus) => {
        switch (status) {
            case SeatStatus.Available:
                return "Còn trống";
            case SeatStatus.Sold:
                return "Đã bán";
            case SeatStatus.Holding:
                return "Đang giữ";
            default:
                return "Không rõ";
        }
    };

    const formatPrice = (price: number) =>
        new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND" }).format(price);

    // --- SEAT SELECTION ---
    const handleSeatClick = (seat: SeatWithStatusDto) => {
        // Only allow clicking available seats
        if (seat.status !== SeatStatus.Available) return;

        setSelectedSeats(prev => {
            const isSelected = prev.some(s => s.id === seat.id);
            if (isSelected) {
                return prev.filter(s => s.id !== seat.id);
            } else {
                return [...prev, seat];
            }
        });
    };

    // --- PRODUCT CART ---
    const addProductToCart = (product: ProductResponseDto) => {
        setProductCart(prev => {
            const existing = prev.find(p => p.product.id === product.id);
            if (existing) {
                return prev.map(p =>
                    p.product.id === product.id
                        ? { ...p, quantity: p.quantity + 1 }
                        : p
                );
            }
            return [...prev, { product, quantity: 1 }];
        });
    };

    const updateProductQuantity = (productId: string, delta: number) => {
        setProductCart(prev => {
            return prev.map(p => {
                if (p.product.id === productId) {
                    const newQty = p.quantity + delta;
                    if (newQty <= 0) return null;
                    return { ...p, quantity: newQty };
                }
                return p;
            }).filter(Boolean) as ProductCartItem[];
        });
    };

    const removeProductFromCart = (productId: string) => {
        setProductCart(prev => prev.filter(p => p.product.id !== productId));
    };

    // --- CALCULATIONS ---
    const ticketTotal = selectedSeats.reduce((sum, s) => sum + s.price, 0);
    const productTotal = productCart.reduce((sum, p) => sum + (p.product.price * p.quantity), 0);
    const grandTotal = ticketTotal + productTotal;
    const changeAmount = cashReceived - grandTotal;

    // --- ORDER CREATION ---
    const handleCreateOrder = async () => {
        if (selectedSeats.length === 0) {
            toast.error("Vui lòng chọn ít nhất một ghế");
            return;
        }

        if (!customerName.trim() || !customerPhone.trim()) {
            toast.error("Vui lòng nhập tên và số điện thoại khách hàng");
            return;
        }

        if (cashReceived < grandTotal) {
            toast.error("Số tiền khách đưa không đủ");
            return;
        }

        try {
            setIsCreatingOrder(true);

            const orderData: PosOrderCreateDto = {
                customerName: customerName.trim(),
                customerPhone: customerPhone.trim(),
                customerEmail: customerEmail.trim() || undefined,
                tickets: selectedSeats.map(seat => ({
                    showtimeId: showtimeId!,
                    seatId: seat.id
                })),
                products: productCart.map(p => ({
                    productId: p.product.id,
                    quantity: p.quantity
                })),
                cashReceived,
                staffNote: staffNote.trim() || undefined
            };

            const result = await createPosOrder(orderData);

            toast.success(
                <div className="flex flex-col gap-1">
                    <span className="font-bold">Đặt vé thành công!</span>
                    <span>Tiền thừa: {formatPrice(changeAmount)}</span>
                </div>
            );

            // Reset states
            setSelectedSeats([]);
            setProductCart([]);
            setShowCheckoutDialog(false);
            setShowCart(false);
            setCustomerName("");
            setCustomerPhone("");
            setCustomerEmail("");
            setCashReceived(0);
            setStaffNote("");

            // Reload seat data
            loadData();

        } catch (err: any) {
            toast.error(err.message || "Lỗi khi tạo đơn hàng");
        } finally {
            setIsCreatingOrder(false);
        }
    };

    // --- API LOAD ---
    useEffect(() => {
        const abortController = new AbortController();

        if (showtimeId) {
            loadData(abortController.signal);
        }

        return () => {
            abortController.abort();
        };
    }, [showtimeId]);

    const loadData = async (signal?: AbortSignal) => {
        if (!showtimeId) return;

        try {
            setIsLoading(true);
            setError(null);
            const [seatsResponse, seatTypesData, productsData] = await Promise.all([
                showtimeApi.getSeatsWithStatus(showtimeId),
                seatTypeApi.getAll(),
                productApi.getActive(),
            ]);

            if (signal?.aborted) return;

            setSeatsData(seatsResponse);
            setSeatTypes(seatTypesData);
            setProducts(productsData);
        } catch (err) {
            if (err instanceof Error && err.name === 'AbortError') {
                return;
            }
            console.error("Error loading data:", err);
            if (!signal?.aborted) {
                setError(err instanceof Error ? err.message : "Lỗi khi tải thông tin");
            }
        } finally {
            if (!signal?.aborted) {
                setIsLoading(false);
            }
        }
    };

    // --- DATA PROCESSING ---
    const seatsByRow = seatsData?.seats.reduce((acc, seat) => {
        if (!acc[seat.seatRow]) acc[seat.seatRow] = [];
        acc[seat.seatRow].push(seat);
        return acc;
    }, {} as Record<string, SeatWithStatusDto[]>) || {};

    const sortedRows = Object.keys(seatsByRow).sort();
    sortedRows.forEach((row) => {
        seatsByRow[row].sort((a, b) => a.seatNumber - b.seatNumber);
    });

    if (isLoading) {
        return (
            <div className="h-screen flex items-center justify-center">
                <Spinner className="w-10 h-10" />
            </div>
        );
    }

    if (error || !seatsData) {
        return (
            <div className="h-screen flex flex-col items-center justify-center gap-4">
                <p className="text-red-500">{error || "Không tìm thấy thông tin"}</p>
                <Button variant="outline" onClick={() => navigate(PATHS.SHOWTIMES)}>
                    <ArrowLeft className="w-4 h-4 mr-2" />
                    Quay lại
                </Button>
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-slate-50/50 pb-20">
            {/* --- HEADER --- */}
            <div className="bg-white border-b sticky top-0 z-30 px-6 py-4 shadow-sm">
                <div className="container mx-auto max-w-7xl flex flex-col md:flex-row md:items-center justify-between gap-4">
                    <div className="flex items-center gap-4">
                        <Button variant="ghost" size="icon" onClick={() => navigate(PATHS.SHOWTIMES)}>
                            <ArrowLeft className="h-5 w-5" />
                        </Button>
                        <div>
                            <h1 className="text-2xl font-bold flex items-center gap-2">
                                <Armchair className="w-6 h-6 text-primary" />
                                Đặt Vé - {seatsData?.movieTitle}
                            </h1>
                            <div className="flex items-center gap-4 text-sm text-muted-foreground mt-1">
                                <span className="flex items-center gap-1">
                                    <Monitor className="w-4 h-4" />
                                    {seatsData?.screenName}
                                </span>
                                <span className="flex items-center gap-1">
                                    <Calendar className="w-4 h-4" />
                                    {format(parseISO(seatsData.showDateTime), "EEEE, dd/MM/yyyy", { locale: vi })}
                                </span>
                                <span className="flex items-center gap-1">
                                    <Clock className="w-4 h-4" />
                                    {format(parseISO(seatsData.showDateTime), "HH:mm")}
                                </span>
                            </div>
                        </div>
                    </div>

                    {/* Cart Button & Stats */}
                    <div className="flex items-center gap-4">
                        <div className="flex items-center gap-6 bg-slate-100 rounded-xl px-4 py-2">
                            <div className="text-center">
                                <div className="text-xl font-bold text-slate-900">{seatsData.totalSeats}</div>
                                <div className="text-xs text-muted-foreground">Tổng ghế</div>
                            </div>
                            <div className="text-center">
                                <div className="text-xl font-bold text-sky-500">{seatsData.availableSeats}</div>
                                <div className="text-xs text-muted-foreground">Còn trống</div>
                            </div>
                            <div className="text-center">
                                <div className="text-xl font-bold text-red-500">{seatsData.soldSeats}</div>
                                <div className="text-xs text-muted-foreground">Đã bán</div>
                            </div>
                        </div>

                        {/* Cart Button */}
                        <Button
                            onClick={() => setShowCart(true)}
                            className="relative"
                            variant={selectedSeats.length > 0 ? "default" : "outline"}
                        >
                            <ShoppingCart className="w-5 h-5 mr-2" />
                            Giỏ hàng
                            {(selectedSeats.length > 0 || productCart.length > 0) && (
                                <Badge className="absolute -top-2 -right-2 bg-red-500 border-none">
                                    {selectedSeats.length + productCart.length}
                                </Badge>
                            )}
                        </Button>
                    </div>
                </div>
            </div>

            <div className="container mx-auto max-w-7xl p-6">
                <div className="grid grid-cols-1 lg:grid-cols-4 gap-6">
                    {/* Seat Map - Large */}
                    <div className="lg:col-span-3">
                        <Card className="h-full border-none shadow-md bg-white">
                            <CardHeader className="pb-2 border-b">
                                <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
                                    <CardTitle className="flex items-center gap-2">
                                        <Ticket className="w-5 h-5 text-primary" />
                                        Chọn ghế ngồi
                                    </CardTitle>

                                    {/* Legend */}
                                    <div className="flex flex-wrap gap-4 text-xs">
                                        <div className="flex items-center gap-1.5">
                                            <div className="w-3 h-3 rounded bg-sky-500"></div> Thường
                                        </div>
                                        <div className="flex items-center gap-1.5">
                                            <div className="w-3 h-3 rounded bg-amber-500"></div> VIP
                                        </div>
                                        <div className="flex items-center gap-1.5">
                                            <div className="w-8 h-3 rounded bg-pink-500"></div> Đôi
                                        </div>
                                        <div className="flex items-center gap-1.5">
                                            <div className="w-3 h-3 rounded bg-emerald-500"></div> Đang chọn
                                        </div>
                                        <div className="flex items-center gap-1.5">
                                            <div className="w-3 h-3 rounded bg-red-500"></div> Đã bán
                                        </div>
                                        <div className="flex items-center gap-1.5">
                                            <div className="w-3 h-3 rounded bg-orange-400"></div> Đang giữ
                                        </div>
                                    </div>
                                </div>
                            </CardHeader>

                            <CardContent className="p-8 bg-slate-50 min-h-[500px] flex flex-col items-center">
                                {/* Screen Visual */}
                                <div className="w-full max-w-2xl mb-12 relative group">
                                    <div className="h-2 bg-slate-300 rounded-[50%] shadow-[0_15px_15px_-5px_rgba(0,0,0,0.1)] w-full mx-auto transform -scale-x-100"></div>
                                    <div className="absolute -top-6 left-0 right-0 text-center">
                                        <span className="text-xs font-bold text-slate-400 uppercase tracking-[0.3em] flex items-center justify-center gap-2">
                                            <Monitor className="w-4 h-4" /> Màn hình chiếu
                                        </span>
                                    </div>
                                </div>

                                {/* Seat Map */}
                                <div className="w-full overflow-x-auto pb-8 custom-scrollbar">
                                    {seatsData.seats.length === 0 ? (
                                        <div className="flex flex-col items-center justify-center py-20 text-muted-foreground">
                                            <Armchair className="w-16 h-16 mb-4 opacity-20" />
                                            <p>Chưa có ghế nào được thiết lập.</p>
                                        </div>
                                    ) : (
                                        <div className="flex flex-col gap-3 min-w-max px-8">
                                            {sortedRows.map(row => (
                                                <div key={row} className="flex items-center justify-center gap-3 group/row hover:bg-slate-100/50 rounded-lg py-1 px-2 transition-colors">
                                                    <div className="w-8 text-right font-bold text-slate-400 text-sm">{row}</div>
                                                    <div className="flex items-center gap-2">
                                                        {seatsByRow[row].map(seat => {
                                                            const isCouple = isCoupleSeat(seat.seatTypeCode);
                                                            const isAvailable = seat.status === SeatStatus.Available;

                                                            return (
                                                                <div key={seat.id} className="relative group/seat">
                                                                    <div
                                                                        onClick={() => handleSeatClick(seat)}
                                                                        className={cn(
                                                                            "relative flex items-center justify-center text-[10px] font-bold transition-all duration-200",
                                                                            "h-9 rounded-t-lg rounded-b-md border-b-4",
                                                                            isCouple ? "w-20 tracking-widest" : "w-9",
                                                                            getSeatStyle(seat)
                                                                        )}
                                                                    >
                                                                        {seat.seatNumber}

                                                                        {/* Status Icon */}
                                                                        {seat.status === SeatStatus.Sold && (
                                                                            <div className="absolute inset-0 flex items-center justify-center">
                                                                                <Users className="w-4 h-4 opacity-60" />
                                                                            </div>
                                                                        )}
                                                                        {seat.status === SeatStatus.Holding && (
                                                                            <div className="absolute inset-0 flex items-center justify-center">
                                                                                <Clock className="w-4 h-4 opacity-60" />
                                                                            </div>
                                                                        )}
                                                                        {isSeatSelected(seat.id) && (
                                                                            <div className="absolute -top-1 -right-1 bg-white rounded-full p-0.5">
                                                                                <CheckCircle2 className="w-3 h-3 text-emerald-500" />
                                                                            </div>
                                                                        )}

                                                                        {/* Tooltip */}
                                                                        <div className="absolute -top-14 left-1/2 -translate-x-1/2 bg-black text-white text-[10px] py-2 px-3 rounded opacity-0 group-hover/seat:opacity-100 whitespace-nowrap z-10 pointer-events-none transition-opacity flex flex-col gap-0.5">
                                                                            <span className="font-bold">{seat.seatCode} - {getSeatTypeName(seat.seatTypeCode)}</span>
                                                                            <span>{getStatusLabel(seat.status)}</span>
                                                                            <span>{formatPrice(seat.price)}</span>
                                                                        </div>
                                                                    </div>
                                                                </div>
                                                            );
                                                        })}
                                                    </div>
                                                    <div className="w-8 text-left font-bold text-slate-400 text-sm">{row}</div>
                                                </div>
                                            ))}
                                        </div>
                                    )}
                                </div>
                            </CardContent>
                        </Card>
                    </div>

                    {/* Quick Products Panel */}
                    <div className="lg:col-span-1">
                        <Card className="border-none shadow-md bg-white sticky top-24">
                            <CardHeader className="pb-2 border-b">
                                <CardTitle className="flex items-center gap-2 text-lg">
                                    <Package className="w-5 h-5 text-orange-500" />
                                    Thêm đồ ăn/uống
                                </CardTitle>
                            </CardHeader>
                            <CardContent className="p-4 max-h-[400px] overflow-y-auto">
                                <div className="grid grid-cols-1 gap-3">
                                    {products.map(product => (
                                        <div
                                            key={product.id}
                                            onClick={() => addProductToCart(product)}
                                            className="flex items-center gap-3 p-3 rounded-lg border hover:border-primary hover:bg-primary/5 cursor-pointer transition-all"
                                        >
                                            {product.imageUrl ? (
                                                <img src={product.imageUrl} alt={product.name} className="w-12 h-12 rounded-lg object-cover" />
                                            ) : (
                                                <div className="w-12 h-12 rounded-lg bg-orange-100 flex items-center justify-center">
                                                    <Package className="w-6 h-6 text-orange-500" />
                                                </div>
                                            )}
                                            <div className="flex-1 min-w-0">
                                                <p className="font-medium text-sm truncate">{product.name}</p>
                                                <p className="text-xs text-emerald-600 font-semibold">{formatPrice(product.price)}</p>
                                            </div>
                                            <Button size="sm" variant="ghost" className="shrink-0">
                                                +
                                            </Button>
                                        </div>
                                    ))}
                                    {products.length === 0 && (
                                        <p className="text-sm text-muted-foreground text-center py-8">Không có sản phẩm</p>
                                    )}
                                </div>
                            </CardContent>
                        </Card>
                    </div>
                </div>
            </div>

            {/* Floating Cart Summary */}
            {selectedSeats.length > 0 && (
                <div className="fixed bottom-6 left-1/2 -translate-x-1/2 z-40">
                    <div className="bg-white rounded-2xl shadow-2xl border px-6 py-4 flex items-center gap-6">
                        <div className="flex items-center gap-2">
                            <Ticket className="w-5 h-5 text-primary" />
                            <span className="font-semibold">{selectedSeats.length} ghế</span>
                        </div>
                        <Separator orientation="vertical" className="h-8" />
                        <div className="text-right">
                            <p className="text-xs text-muted-foreground">Tổng tiền</p>
                            <p className="font-bold text-lg text-emerald-600">{formatPrice(grandTotal)}</p>
                        </div>
                        <Button onClick={() => setShowCart(true)} size="lg" className="ml-2">
                            Thanh toán
                        </Button>
                    </div>
                </div>
            )}

            {/* Cart Sheet */}
            <Sheet open={showCart} onOpenChange={setShowCart}>
                <SheetContent className="w-full sm:max-w-lg flex flex-col">
                    <SheetHeader>
                        <SheetTitle className="flex items-center gap-2">
                            <ShoppingCart className="w-5 h-5" />
                            Giỏ hàng
                        </SheetTitle>
                        <SheetDescription>
                            Chi tiết đơn hàng của bạn
                        </SheetDescription>
                    </SheetHeader>

                    <div className="flex-1 overflow-y-auto py-4 space-y-6">
                        {/* Selected Seats */}
                        <div>
                            <h3 className="font-semibold flex items-center gap-2 mb-3">
                                <Ticket className="w-4 h-4 text-primary" />
                                Ghế đã chọn ({selectedSeats.length})
                            </h3>
                            {selectedSeats.length === 0 ? (
                                <p className="text-sm text-muted-foreground">Chưa chọn ghế nào</p>
                            ) : (
                                <div className="space-y-2">
                                    {selectedSeats.map(seat => (
                                        <div key={seat.id} className="flex items-center justify-between p-3 rounded-lg bg-muted/50">
                                            <div className="flex items-center gap-3">
                                                <Badge variant="secondary">{seat.seatCode}</Badge>
                                                <span className="text-sm text-muted-foreground">{getSeatTypeName(seat.seatTypeCode)}</span>
                                            </div>
                                            <div className="flex items-center gap-2">
                                                <span className="font-semibold">{formatPrice(seat.price)}</span>
                                                <Button
                                                    variant="ghost"
                                                    size="icon"
                                                    className="h-6 w-6 text-red-500 hover:text-red-600"
                                                    onClick={() => setSelectedSeats(prev => prev.filter(s => s.id !== seat.id))}
                                                >
                                                    <X className="w-4 h-4" />
                                                </Button>
                                            </div>
                                        </div>
                                    ))}
                                </div>
                            )}
                        </div>

                        <Separator />

                        {/* Products in Cart */}
                        <div>
                            <h3 className="font-semibold flex items-center gap-2 mb-3">
                                <Package className="w-4 h-4 text-orange-500" />
                                Đồ ăn / Uống ({productCart.length})
                            </h3>
                            {productCart.length === 0 ? (
                                <p className="text-sm text-muted-foreground">Chưa thêm sản phẩm nào</p>
                            ) : (
                                <div className="space-y-2">
                                    {productCart.map(item => (
                                        <div key={item.product.id} className="flex items-center justify-between p-3 rounded-lg bg-muted/50">
                                            <div className="flex items-center gap-3">
                                                <span className="font-medium text-sm">{item.product.name}</span>
                                            </div>
                                            <div className="flex items-center gap-3">
                                                <div className="flex items-center gap-1">
                                                    <Button
                                                        variant="outline"
                                                        size="icon"
                                                        className="h-6 w-6"
                                                        onClick={() => updateProductQuantity(item.product.id, -1)}
                                                    >
                                                        -
                                                    </Button>
                                                    <span className="w-8 text-center font-medium">{item.quantity}</span>
                                                    <Button
                                                        variant="outline"
                                                        size="icon"
                                                        className="h-6 w-6"
                                                        onClick={() => updateProductQuantity(item.product.id, 1)}
                                                    >
                                                        +
                                                    </Button>
                                                </div>
                                                <span className="font-semibold w-24 text-right">{formatPrice(item.product.price * item.quantity)}</span>
                                                <Button
                                                    variant="ghost"
                                                    size="icon"
                                                    className="h-6 w-6 text-red-500 hover:text-red-600"
                                                    onClick={() => removeProductFromCart(item.product.id)}
                                                >
                                                    <X className="w-4 h-4" />
                                                </Button>
                                            </div>
                                        </div>
                                    ))}
                                </div>
                            )}
                        </div>
                    </div>

                    {/* Summary */}
                    <div className="border-t pt-4 space-y-3">
                        <div className="flex justify-between text-sm">
                            <span className="text-muted-foreground">Vé ({selectedSeats.length})</span>
                            <span>{formatPrice(ticketTotal)}</span>
                        </div>
                        <div className="flex justify-between text-sm">
                            <span className="text-muted-foreground">Đồ ăn/uống</span>
                            <span>{formatPrice(productTotal)}</span>
                        </div>
                        <Separator />
                        <div className="flex justify-between text-lg font-bold">
                            <span>Tổng cộng</span>
                            <span className="text-emerald-600">{formatPrice(grandTotal)}</span>
                        </div>
                    </div>

                    <SheetFooter className="pt-4">
                        <Button
                            className="w-full"
                            size="lg"
                            disabled={selectedSeats.length === 0}
                            onClick={() => {
                                setShowCart(false);
                                setShowCheckoutDialog(true);
                                setCashReceived(grandTotal);
                            }}
                        >
                            <CreditCard className="w-5 h-5 mr-2" />
                            Tiến hành thanh toán
                        </Button>
                    </SheetFooter>
                </SheetContent>
            </Sheet>

            {/* Checkout Dialog */}
            <Dialog open={showCheckoutDialog} onOpenChange={setShowCheckoutDialog}>
                <DialogContent className="sm:max-w-md">
                    <DialogHeader>
                        <DialogTitle className="flex items-center gap-2">
                            <CreditCard className="w-5 h-5 text-primary" />
                            Thanh toán tiền mặt
                        </DialogTitle>
                        <DialogDescription>
                            Nhập thông tin khách hàng và số tiền nhận
                        </DialogDescription>
                    </DialogHeader>

                    <div className="space-y-4 py-4">
                        {/* Customer Info */}
                        <div className="space-y-2">
                            <Label htmlFor="customerName">Tên khách hàng *</Label>
                            <Input
                                id="customerName"
                                placeholder="Nguyễn Văn A"
                                value={customerName}
                                onChange={e => setCustomerName(e.target.value)}
                            />
                        </div>
                        <div className="space-y-2">
                            <Label htmlFor="customerPhone">Số điện thoại *</Label>
                            <Input
                                id="customerPhone"
                                placeholder="0901234567"
                                value={customerPhone}
                                onChange={e => setCustomerPhone(e.target.value)}
                            />
                        </div>
                        <div className="space-y-2">
                            <Label htmlFor="customerEmail">Email (tùy chọn)</Label>
                            <Input
                                id="customerEmail"
                                type="email"
                                placeholder="email@example.com"
                                value={customerEmail}
                                onChange={e => setCustomerEmail(e.target.value)}
                            />
                        </div>

                        <Separator />

                        {/* Payment */}
                        <div className="bg-muted/50 rounded-lg p-4 space-y-3">
                            <div className="flex justify-between text-lg font-bold">
                                <span>Tổng tiền</span>
                                <span className="text-emerald-600">{formatPrice(grandTotal)}</span>
                            </div>
                            <div className="space-y-2">
                                <Label htmlFor="cashReceived">Tiền khách đưa *</Label>
                                <Input
                                    id="cashReceived"
                                    type="number"
                                    placeholder="0"
                                    value={cashReceived || ""}
                                    onChange={e => setCashReceived(Number(e.target.value))}
                                    className="text-lg font-semibold"
                                />
                            </div>
                            {cashReceived >= grandTotal && (
                                <div className="flex justify-between text-lg font-bold text-primary">
                                    <span>Tiền thừa</span>
                                    <span>{formatPrice(changeAmount)}</span>
                                </div>
                            )}
                            {cashReceived < grandTotal && cashReceived > 0 && (
                                <p className="text-sm text-red-500">Số tiền khách đưa chưa đủ!</p>
                            )}
                        </div>

                        <div className="space-y-2">
                            <Label htmlFor="staffNote">Ghi chú</Label>
                            <Input
                                id="staffNote"
                                placeholder="Ghi chú của nhân viên..."
                                value={staffNote}
                                onChange={e => setStaffNote(e.target.value)}
                            />
                        </div>
                    </div>

                    <DialogFooter>
                        <Button variant="outline" onClick={() => setShowCheckoutDialog(false)}>
                            Hủy
                        </Button>
                        <Button
                            onClick={handleCreateOrder}
                            disabled={isCreatingOrder || !customerName || !customerPhone || cashReceived < grandTotal}
                        >
                            {isCreatingOrder ? (
                                <>
                                    <Loader2 className="w-4 h-4 mr-2 animate-spin" />
                                    Đang xử lý...
                                </>
                            ) : (
                                <>
                                    <CheckCircle2 className="w-4 h-4 mr-2" />
                                    Xác nhận thanh toán
                                </>
                            )}
                        </Button>
                    </DialogFooter>
                </DialogContent>
            </Dialog>
        </div>
    );
};

export default ShowtimeSeatsPage;
