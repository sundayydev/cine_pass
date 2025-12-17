import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { ArrowLeft, Monitor, Armchair, Clock, Calendar, Ticket, Users } from "lucide-react";
import { format, parseISO } from "date-fns";
import { vi } from "date-fns/locale";
import { cn } from "@/lib/utils";

// API Services
import {
    showtimeApi,
    type ShowtimeSeatsResponse,
    type SeatWithStatusDto,
    SeatStatus
} from "@/services/apiShowtime";
import { seatTypeApi, type SeatTypeResponseDto } from "@/services/apiSeatType";
import { PATHS } from "@/config/paths";

// Shadcn UI
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Spinner } from "@/components/ui/spinner";

const ShowtimeSeatsPage = () => {
    const { showtimeId } = useParams<{ showtimeId: string }>();
    const navigate = useNavigate();

    const [seatsData, setSeatsData] = useState<ShowtimeSeatsResponse | null>(null);
    const [seatTypes, setSeatTypes] = useState<SeatTypeResponseDto[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    // --- HELPERS ---

    // Check if seat is Couple type
    const isCoupleSeat = (seatTypeCode?: string) => {
        return seatTypeCode?.toUpperCase().includes("COUPLE");
    };

    // Get seat styling based on status and type
    const getSeatStyle = (seat: SeatWithStatusDto) => {
        // Base style for status
        if (seat.status === SeatStatus.Sold) {
            return "bg-red-500 border-red-600 text-white shadow-red-200 cursor-default";
        }

        if (seat.status === SeatStatus.Holding) {
            return "bg-orange-400 border-orange-500 text-white shadow-orange-200 cursor-default";
        }

        // Available - color by type
        const code = seat.seatTypeCode?.toUpperCase() || "NORMAL";

        if (code.includes("VIP")) return "bg-amber-500 border-amber-600 text-white shadow-amber-200";
        if (code.includes("COUPLE")) return "bg-pink-500 border-pink-600 text-white shadow-pink-200";

        // Normal/Default - Available
        return "bg-sky-500 border-sky-600 text-white shadow-sky-200";
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

    // --- API LOAD ---
    useEffect(() => {
        const abortController = new AbortController();

        if (showtimeId) {
            loadData(abortController.signal);
        }

        // Cleanup - abort pending requests when component unmounts
        return () => {
            abortController.abort();
        };
    }, [showtimeId]);

    const loadData = async (signal?: AbortSignal) => {
        if (!showtimeId) return;

        try {
            setIsLoading(true);
            setError(null);
            const [seatsResponse, seatTypesData] = await Promise.all([
                showtimeApi.getSeatsWithStatus(showtimeId),
                seatTypeApi.getAll(),
            ]);

            // Check if the request was aborted before updating state
            if (signal?.aborted) return;

            setSeatsData(seatsResponse);
            setSeatTypes(seatTypesData);
        } catch (err) {
            // Ignore abort errors - they're expected when navigating away
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
                                Sơ đồ Ghế - Suất Chiếu
                            </h1>
                            <div className="flex items-center gap-4 text-sm text-muted-foreground mt-1">
                                <span className="flex items-center gap-1">
                                    <Monitor className="w-4 h-4" />
                                    {seatsData.screenName}
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

                    {/* Stats */}
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
                            <div className="text-center">
                                <div className="text-xl font-bold text-orange-500">{seatsData.holdingSeats}</div>
                                <div className="text-xs text-muted-foreground">Đang giữ</div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <div className="container mx-auto max-w-7xl p-6">
                <Card className="h-full border-none shadow-md bg-white">
                    <CardHeader className="pb-2 border-b">
                        <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
                            <CardTitle className="flex items-center gap-2">
                                <Ticket className="w-5 h-5 text-primary" />
                                Trạng thái ghế ngồi
                            </CardTitle>

                            {/* Legend */}
                            <div className="flex flex-wrap gap-4 text-xs">
                                <div className="flex items-center gap-1.5">
                                    <div className="w-3 h-3 rounded bg-sky-500"></div> Thường (Trống)
                                </div>
                                <div className="flex items-center gap-1.5">
                                    <div className="w-3 h-3 rounded bg-amber-500"></div> VIP (Trống)
                                </div>
                                <div className="flex items-center gap-1.5">
                                    <div className="w-8 h-3 rounded bg-pink-500"></div> Đôi (Trống)
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

                    <CardContent className="p-8 bg-slate-50 min-h-[600px] flex flex-col items-center">

                        {/* 1. SCREEN VISUAL */}
                        <div className="w-full max-w-2xl mb-12 relative group">
                            <div className="h-2 bg-slate-300 rounded-[50%] shadow-[0_15px_15px_-5px_rgba(0,0,0,0.1)] w-full mx-auto transform -scale-x-100"></div>
                            <div className="absolute -top-6 left-0 right-0 text-center">
                                <span className="text-xs font-bold text-slate-400 uppercase tracking-[0.3em] flex items-center justify-center gap-2">
                                    <Monitor className="w-4 h-4" /> Màn hình chiếu
                                </span>
                            </div>
                            {/* Light beam effect */}
                            <div className="absolute top-2 left-[10%] right-[10%] h-16 bg-gradient-to-b from-slate-200/50 to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-500"></div>
                        </div>

                        {/* 2. SEAT MAP CONTAINER */}
                        <div className="w-full overflow-x-auto pb-12 custom-scrollbar">
                            {seatsData.seats.length === 0 ? (
                                <div className="flex flex-col items-center justify-center py-20 text-muted-foreground">
                                    <Armchair className="w-16 h-16 mb-4 opacity-20" />
                                    <p>Chưa có ghế nào được thiết lập.</p>
                                </div>
                            ) : (
                                <div className="flex flex-col gap-3 min-w-max px-8">
                                    {sortedRows.map(row => (
                                        <div key={row} className="flex items-center justify-center gap-3 group/row hover:bg-slate-100/50 rounded-lg py-1 px-2 transition-colors">

                                            {/* Row Label (Left) */}
                                            <div className="w-8 text-right font-bold text-slate-400 text-sm">{row}</div>

                                            {/* Seats in Row */}
                                            <div className="flex items-center gap-2">
                                                {seatsByRow[row].map(seat => {
                                                    const isCouple = isCoupleSeat(seat.seatTypeCode);

                                                    return (
                                                        <div key={seat.id} className="relative group/seat">
                                                            <div
                                                                className={cn(
                                                                    "relative flex items-center justify-center text-[10px] font-bold transition-all duration-200",
                                                                    // Styling shape
                                                                    "h-9 rounded-t-lg rounded-b-md border-b-4",
                                                                    // Width logic based on type
                                                                    isCouple ? "w-20 tracking-widest" : "w-9",
                                                                    // Color logic
                                                                    getSeatStyle(seat)
                                                                )}
                                                            >
                                                                {seat.seatNumber}

                                                                {/* Sold/Holding Icon overlay */}
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

                                                                {/* Tooltip on hover */}
                                                                <div className="absolute -top-14 left-1/2 -translate-x-1/2 bg-black text-white text-[10px] py-2 px-3 rounded opacity-0 group-hover/seat:opacity-100 whitespace-nowrap z-10 pointer-events-none transition-opacity flex flex-col gap-0.5">
                                                                    <span className="font-bold">{seat.seatCode} - {getSeatTypeName(seat.seatTypeCode)}</span>
                                                                    <span>{getStatusLabel(seat.status)}</span>
                                                                    <span>{formatPrice(seat.price)}</span>
                                                                    {seat.heldByUserId && (
                                                                        <span className="text-orange-300 text-[9px]">User: {seat.heldByUserId.slice(0, 8)}...</span>
                                                                    )}
                                                                </div>
                                                            </div>
                                                        </div>
                                                    );
                                                })}
                                            </div>

                                            {/* Row Label (Right - Optional balance) */}
                                            <div className="w-8 text-left font-bold text-slate-400 text-sm">{row}</div>
                                        </div>
                                    ))}
                                </div>
                            )}
                        </div>

                        {/* Occupancy Progress */}
                        <div className="w-full max-w-2xl mt-8">
                            <div className="flex items-center justify-between text-sm mb-2">
                                <span className="text-muted-foreground">Tỷ lệ lấp đầy</span>
                                <span className="font-bold text-slate-900">
                                    {seatsData.totalSeats > 0
                                        ? Math.round(((seatsData.soldSeats + seatsData.holdingSeats) / seatsData.totalSeats) * 100)
                                        : 0}%
                                </span>
                            </div>
                            <div className="h-3 bg-slate-200 rounded-full overflow-hidden flex">
                                <div
                                    className="h-full bg-red-500 transition-all duration-500"
                                    style={{ width: `${seatsData.totalSeats > 0 ? (seatsData.soldSeats / seatsData.totalSeats) * 100 : 0}%` }}
                                />
                                <div
                                    className="h-full bg-orange-400 transition-all duration-500"
                                    style={{ width: `${seatsData.totalSeats > 0 ? (seatsData.holdingSeats / seatsData.totalSeats) * 100 : 0}%` }}
                                />
                            </div>
                            <div className="flex items-center justify-center gap-6 mt-3 text-xs text-muted-foreground">
                                <span className="flex items-center gap-1.5"><div className="w-2 h-2 rounded-full bg-red-500"></div> Đã bán ({seatsData.soldSeats})</span>
                                <span className="flex items-center gap-1.5"><div className="w-2 h-2 rounded-full bg-orange-400"></div> Đang giữ ({seatsData.holdingSeats})</span>
                                <span className="flex items-center gap-1.5"><div className="w-2 h-2 rounded-full bg-slate-300"></div> Còn trống ({seatsData.availableSeats})</span>
                            </div>
                        </div>
                    </CardContent>
                </Card>
            </div>
        </div>
    );
};

export default ShowtimeSeatsPage;
