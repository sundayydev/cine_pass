
namespace BE_CinePass.Domain.Common
{
    public enum NotificationActionType
    {
        None,

        // Đơn hàng
        OpenOrderDetail,

        // Phim
        OpenMovieDetail,
        OpenShowtime,

        // Ưu đãi
        OpenVoucherList,
        OpenVoucherDetail,

        // Thành viên
        OpenPointsHistory,

        // Thông báo
        OpenNotificationCenter,

        // Ngoài app
        OpenUrl
    }
}
