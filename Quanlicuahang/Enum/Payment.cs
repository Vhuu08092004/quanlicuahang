namespace Quanlicuahang.Enum
{
    public enum PaymentMethod
    {
        Cash, // tiền mặt
        BankTransfer, // chuyển khoản
        Card, // thẻ tín dụng/ghi nợ
        Momo, // ví điện tử Momo
        VnPay, // ví điện tử VnPay
        Zalo, // ví điện tử ZaloPay
    }

    public enum PaymentStatus
    {
        Pending, // Đang chờ xử lý
        Completed, // Hoàn thành
        Failed, // Thất bại
        Cancelled, // Đã hủy
    }

    public enum OrderStatus
    {
        Pending, // Đang chờ xử lý
        Confirmed, // Đã xác nhận (nhân viên đã gọi điện xác nhận)
        Delivered, // Đã giao hàng
        Paid, // Đã thanh toán
        Cancelled, // Đã hủy
    }
}
