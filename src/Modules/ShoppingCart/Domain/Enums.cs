namespace ShoppingCart.Domain;

public enum CartStatus
{
    Active,         // Giỏ hàng đang hoạt động
    CheckedOut,     // Đã checkout thành Order
    Abandoned,      // Bỏ quên (không hoạt động lâu)
    Merged          // Đã merge với cart khác (guest -> registered)
}
