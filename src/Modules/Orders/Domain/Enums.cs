namespace Orders.Domain;

public enum OrderStatus
{
    Draft,          // Đang tạo
    Pending,        // Chờ reserve inventory
    Reserved,       // Inventory đã reserve
    Confirmed,      // [Online] Đã thanh toán / [COD] Xác nhận đơn
    Shipped,        // Đang giao hàng
    Delivered,      // Đã giao hàng
    Cancelled,      // Đã hủy
    Refunded        // Đã hoàn tiền
}

public enum PaymentMethod
{
    Online,         // Thanh toán trước (VNPAY, Momo, etc.)
    COD             // Thanh toán khi nhận hàng
}

public enum PaymentStatus
{
    Pending,        // Chờ thanh toán
    Paid,           // Đã thanh toán
    Failed,         // Thanh toán thất bại
    Refunded        // Đã hoàn tiền
}

public enum ShippingMethod
{
    Standard,       // Giao hàng tiêu chuẩn (3-5 ngày)
    Fast,           // Giao hàng nhanh (1-2 ngày)
    SuperFast       // Giao hàng hỏa tốc (trong ngày)
}

public enum DiscountSourceType
{
    Coupon,
    Promotion,
    Manual          // Admin manual adjustment
}
