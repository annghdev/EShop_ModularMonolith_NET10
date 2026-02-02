namespace Shipping.Domain;

public enum ShippingProvider
{
    GHN,            // Giao Hàng Nhanh
    GHTK,           // Giao Hàng Tiết Kiệm
    ViettelPost,    // Viettel Post
    JT,             // J&T Express
    NinjaVan,       // Ninja Van
    Manual          // Tự vận chuyển
}

public enum ShippingCarrierStatus
{
    Active,
    Disabled,
    Maintenance
}

public enum ShipmentStatus
{
    Created,        // Đã tạo đơn vận chuyển
    AwaitingPickup, // Chờ lấy hàng
    PickedUp,       // Đã lấy hàng
    InTransit,      // Đang vận chuyển
    OutForDelivery, // Đang giao hàng
    Delivered,      // Đã giao thành công
    DeliveryFailed, // Giao hàng thất bại
    Returned,       // Hoàn trả
    Cancelled       // Đã hủy
}

public enum TrackingEventType
{
    Created,
    PickupScheduled,
    PickedUp,
    ArrivedAtHub,
    InTransit,
    OutForDelivery,
    DeliveryAttempted,
    Delivered,
    DeliveryFailed,
    Returning,
    Returned,
    Cancelled,
    Other
}
