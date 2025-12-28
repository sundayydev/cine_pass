namespace BE_CinePass.Domain.Common;

public enum PointHistoryType
{
    Purchase,      // Tích điểm từ mua vé/sản phẩm
    Refund,        // Hoàn điểm khi hoàn tiền
    Reward,        // Điểm thưởng
    RedeemVoucher, // Đổi điểm lấy voucher
    Expired        // Điểm hết hạn
}
