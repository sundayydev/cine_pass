namespace BE_CinePass.Shared.Common
{
    public enum NotificationActionType
    {
        None = 0,

        // Đơn hàng
        OpenOrderDetail = 1,

        // Phim
        OpenMovieDetail = 2,
        OpenShowtime = 3,

        // Ưu đãi
        OpenVoucherList = 4,
        OpenVoucherDetail = 5,

        // Thành viên
        OpenPointsHistory = 6,

        // Thông báo
        OpenNotificationCenter = 7,

        // Ngoài app
        OpenUrl = 8
    }
}
