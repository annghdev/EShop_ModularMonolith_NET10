namespace Payment.Domain;

public enum PaymentProvider
{
    VNPay,
    Momo,
    ZaloPay,
    COD,
    BankTransfer
}

public enum PaymentGatewayStatus
{
    Active,
    Disabled,
    Maintenance
}

public enum PaymentTransactionStatus
{
    Pending,      // Đã tạo, chờ payment
    Processing,   // Đang xử lý tại gateway
    Success,      // Thành công
    Failed,       // Thất bại
    Cancelled,    // Đã hủy
    Expired,      // Hết hạn (timeout)
    Refunding,    // Đang hoàn tiền
    Refunded      // Đã hoàn tiền
}

public enum RefundStatus
{
    Pending,
    Processing,
    Completed,
    Failed
}
