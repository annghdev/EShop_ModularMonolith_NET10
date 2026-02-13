import type {
    OrderDto,
    OrderSummaryDto,
    PlaceOrderRequest,
    CancelOrderRequest,
    ApplyVoucherResponse,
    ActivePromotionDto,
    ShippingFeeDto,
    OrderStatus,
    ShipmentStatus,
    UserVoucherDto,
} from '../types/order'
import type { MoneyDto } from '../types/cart'

// ===== Helpers =====

const delay = (ms: number) => new Promise(resolve => setTimeout(resolve, ms))

const money = (amount: number, currency = 'VND'): MoneyDto => ({ Amount: amount, Currency: currency })

const uuid = () => crypto.randomUUID()

const genOrderNumber = () => {
    const ts = new Date().toISOString().replace(/[-:T]/g, '').slice(0, 14)
    const rand = Math.random().toString(36).slice(2, 6).toUpperCase()
    return `ORD-${ts}-${rand}`
}

// ===== Mock Data Store =====

const mockOrders: OrderDto[] = [
    {
        Id: '11111111-1111-1111-1111-111111111111',
        OrderNumber: 'ORD-20260210120000-A1B2',
        CustomerId: 'user-001',
        Status: 'Delivered',
        ShippingAddress: {
            FullName: 'Nguyễn Văn An',
            Phone: '0901234567',
            Street: '123 Nguyễn Huệ',
            Ward: 'Phường Bến Nghé',
            District: 'Quận 1',
            City: 'TP. Hồ Chí Minh',
        },
        PaymentMethod: 'Online',
        PaymentStatus: 'Paid',
        PaidAt: '2026-02-10T12:05:00Z',
        ShippingMethod: 'Fast',
        ShippingInfo: {
            ShipmentNumber: 'SHP-20260210120500-X1Y2',
            Provider: 'GHN',
            TrackingNumber: 'GHN123456789',
            Status: 'Delivered' as ShipmentStatus,
            EstimatedDeliveryDate: '2026-02-12T00:00:00Z',
            ActualDeliveryDate: '2026-02-11T15:30:00Z',
            TrackingEvents: [
                { Id: uuid(), EventType: 'Created', Description: 'Đơn vận chuyển đã được tạo', Timestamp: '2026-02-10T12:05:00Z' },
                { Id: uuid(), EventType: 'PickupScheduled', Description: 'Đã lên lịch lấy hàng', Timestamp: '2026-02-10T14:00:00Z' },
                { Id: uuid(), EventType: 'PickedUp', Description: 'Đã lấy hàng', Location: 'Kho Quận 7', Timestamp: '2026-02-10T16:30:00Z' },
                { Id: uuid(), EventType: 'InTransit', Description: 'Đang vận chuyển', Location: 'Trung tâm phân loại Quận 2', Timestamp: '2026-02-11T08:00:00Z' },
                { Id: uuid(), EventType: 'OutForDelivery', Description: 'Đang giao hàng', Location: 'Quận 1', Timestamp: '2026-02-11T13:00:00Z' },
                { Id: uuid(), EventType: 'Delivered', Description: 'Đã giao hàng thành công', Location: '123 Nguyễn Huệ, Q1', Timestamp: '2026-02-11T15:30:00Z' },
            ],
        },
        SubTotal: money(1580000),
        ShippingFee: money(25000),
        TotalDiscount: money(158000),
        GrandTotal: money(1447000),
        Items: [
            {
                Id: uuid(), ProductId: 'p1', VariantId: 'v1', Sku: 'TSH-BLK-M',
                ProductName: 'Áo thun Basic Cotton', VariantName: 'Đen - M',
                Thumbnail: 'https://picsum.photos/seed/tshirt1/120/120',
                UnitPrice: money(350000), Quantity: 2, DiscountAmount: money(70000),
                LineTotal: money(700000), LineTotalAfterDiscount: money(630000),
            },
            {
                Id: uuid(), ProductId: 'p2', VariantId: 'v2', Sku: 'JNS-BLU-32',
                ProductName: 'Quần Jean Slim Fit', VariantName: 'Xanh đậm - 32',
                Thumbnail: 'https://picsum.photos/seed/jeans1/120/120',
                UnitPrice: money(880000), Quantity: 1, DiscountAmount: money(88000),
                LineTotal: money(880000), LineTotalAfterDiscount: money(792000),
            },
        ],
        Discounts: [
            { Id: uuid(), Source: 'Promotion', SourceId: 'promo-1', Description: 'Giảm 10% toàn bộ đơn hàng', Amount: money(158000) },
        ],
        StatusHistory: [
            { Id: uuid(), FromStatus: 'Draft', ToStatus: 'Pending', CreatedAt: '2026-02-10T12:00:00Z' },
            { Id: uuid(), FromStatus: 'Pending', ToStatus: 'Reserved', CreatedAt: '2026-02-10T12:01:00Z' },
            { Id: uuid(), FromStatus: 'Reserved', ToStatus: 'Confirmed', CreatedAt: '2026-02-10T12:05:00Z' },
            { Id: uuid(), FromStatus: 'Confirmed', ToStatus: 'Shipped', CreatedAt: '2026-02-10T16:30:00Z' },
            { Id: uuid(), FromStatus: 'Shipped', ToStatus: 'Delivered', CreatedAt: '2026-02-11T15:30:00Z' },
        ],
        CreatedAt: '2026-02-10T12:00:00Z',
        UpdatedAt: '2026-02-11T15:30:00Z',
    },
    {
        Id: '22222222-2222-2222-2222-222222222222',
        OrderNumber: 'ORD-20260212090000-C3D4',
        CustomerId: 'user-001',
        Status: 'Shipped',
        ShippingAddress: {
            FullName: 'Nguyễn Văn An',
            Phone: '0901234567',
            Street: '456 Lê Lợi',
            Ward: 'Phường Bến Thành',
            District: 'Quận 1',
            City: 'TP. Hồ Chí Minh',
        },
        PaymentMethod: 'COD',
        PaymentStatus: 'Pending',
        ShippingMethod: 'Standard',
        ShippingInfo: {
            ShipmentNumber: 'SHP-20260212090500-Z1W2',
            Provider: 'GHTK',
            TrackingNumber: 'GHTK987654321',
            Status: 'InTransit' as ShipmentStatus,
            EstimatedDeliveryDate: '2026-02-16T00:00:00Z',
            TrackingEvents: [
                { Id: uuid(), EventType: 'Created', Description: 'Đơn vận chuyển đã được tạo', Timestamp: '2026-02-12T09:05:00Z' },
                { Id: uuid(), EventType: 'PickedUp', Description: 'Đã lấy hàng', Location: 'Kho Quận 3', Timestamp: '2026-02-12T14:00:00Z' },
                { Id: uuid(), EventType: 'InTransit', Description: 'Đang vận chuyển', Location: 'Trung tâm Bình Dương', Timestamp: '2026-02-13T08:00:00Z' },
            ],
        },
        SubTotal: money(2450000),
        ShippingFee: money(15000),
        TotalDiscount: money(200000),
        GrandTotal: money(2265000),
        Items: [
            {
                Id: uuid(), ProductId: 'p3', VariantId: 'v3', Sku: 'DRS-RED-S',
                ProductName: 'Váy Hoa Vintage', VariantName: 'Đỏ - S',
                Thumbnail: 'https://picsum.photos/seed/dress1/120/120',
                UnitPrice: money(650000), Quantity: 1, DiscountAmount: money(0),
                LineTotal: money(650000), LineTotalAfterDiscount: money(650000),
            },
            {
                Id: uuid(), ProductId: 'p4', VariantId: 'v4', Sku: 'BAG-BRN-01',
                ProductName: 'Túi xách da thật', VariantName: 'Nâu',
                Thumbnail: 'https://picsum.photos/seed/bag1/120/120',
                UnitPrice: money(1800000), Quantity: 1, DiscountAmount: money(200000),
                LineTotal: money(1800000), LineTotalAfterDiscount: money(1600000),
            },
        ],
        Discounts: [
            { Id: uuid(), Source: 'Coupon', SourceId: 'cp-1', SourceCode: 'SALE200K', Description: 'Giảm 200,000₫ cho đơn từ 2 triệu', Amount: money(200000) },
        ],
        StatusHistory: [
            { Id: uuid(), FromStatus: 'Draft', ToStatus: 'Pending', CreatedAt: '2026-02-12T09:00:00Z' },
            { Id: uuid(), FromStatus: 'Pending', ToStatus: 'Reserved', CreatedAt: '2026-02-12T09:01:00Z' },
            { Id: uuid(), FromStatus: 'Reserved', ToStatus: 'Confirmed', CreatedAt: '2026-02-12T09:05:00Z' },
            { Id: uuid(), FromStatus: 'Confirmed', ToStatus: 'Shipped', CreatedAt: '2026-02-12T14:00:00Z' },
        ],
        CreatedAt: '2026-02-12T09:00:00Z',
        UpdatedAt: '2026-02-12T14:00:00Z',
    },
    {
        Id: '33333333-3333-3333-3333-333333333333',
        OrderNumber: 'ORD-20260205140000-E5F6',
        CustomerId: 'user-001',
        Status: 'Cancelled',
        ShippingAddress: {
            FullName: 'Nguyễn Văn An',
            Phone: '0901234567',
            Street: '789 Trần Hưng Đạo',
            Ward: 'Phường Cầu Kho',
            District: 'Quận 1',
            City: 'TP. Hồ Chí Minh',
        },
        PaymentMethod: 'Online',
        PaymentStatus: 'Refunded',
        ShippingMethod: 'SuperFast',
        SubTotal: money(990000),
        ShippingFee: money(45000),
        TotalDiscount: money(0),
        GrandTotal: money(1035000),
        CancellationReason: 'Đổi ý, không muốn mua nữa',
        Items: [
            {
                Id: uuid(), ProductId: 'p5', VariantId: 'v5', Sku: 'SHO-WHT-42',
                ProductName: 'Giày Sneaker Classic', VariantName: 'Trắng - 42',
                Thumbnail: 'https://picsum.photos/seed/shoe1/120/120',
                UnitPrice: money(990000), Quantity: 1, DiscountAmount: money(0),
                LineTotal: money(990000), LineTotalAfterDiscount: money(990000),
            },
        ],
        Discounts: [],
        StatusHistory: [
            { Id: uuid(), FromStatus: 'Draft', ToStatus: 'Pending', CreatedAt: '2026-02-05T14:00:00Z' },
            { Id: uuid(), FromStatus: 'Pending', ToStatus: 'Reserved', CreatedAt: '2026-02-05T14:01:00Z' },
            { Id: uuid(), FromStatus: 'Reserved', ToStatus: 'Confirmed', CreatedAt: '2026-02-05T14:05:00Z' },
            { Id: uuid(), FromStatus: 'Confirmed', ToStatus: 'Cancelled', Reason: 'Đổi ý, không muốn mua nữa', CreatedAt: '2026-02-05T15:00:00Z' },
        ],
        CreatedAt: '2026-02-05T14:00:00Z',
        UpdatedAt: '2026-02-05T15:00:00Z',
    },
    {
        Id: '44444444-4444-4444-4444-444444444444',
        OrderNumber: 'ORD-20260213080000-G7H8',
        CustomerId: 'user-001',
        Status: 'Confirmed',
        ShippingAddress: {
            FullName: 'Nguyễn Văn An',
            Phone: '0901234567',
            Street: '321 Hai Bà Trưng',
            Ward: 'Phường Tân Định',
            District: 'Quận 1',
            City: 'TP. Hồ Chí Minh',
        },
        PaymentMethod: 'Online',
        PaymentStatus: 'Paid',
        PaidAt: '2026-02-13T08:05:00Z',
        ShippingMethod: 'Fast',
        SubTotal: money(1250000),
        ShippingFee: money(25000),
        TotalDiscount: money(125000),
        GrandTotal: money(1150000),
        Items: [
            {
                Id: uuid(), ProductId: 'p6', VariantId: 'v6', Sku: 'PLO-NVY-L',
                ProductName: 'Áo Polo Premium', VariantName: 'Navy - L',
                Thumbnail: 'https://picsum.photos/seed/polo1/120/120',
                UnitPrice: money(450000), Quantity: 1, DiscountAmount: money(45000),
                LineTotal: money(450000), LineTotalAfterDiscount: money(405000),
            },
            {
                Id: uuid(), ProductId: 'p7', VariantId: 'v7', Sku: 'SHT-WHT-M',
                ProductName: 'Áo Sơ Mi Oxford', VariantName: 'Trắng - M',
                Thumbnail: 'https://picsum.photos/seed/shirt1/120/120',
                UnitPrice: money(800000), Quantity: 1, DiscountAmount: money(80000),
                LineTotal: money(800000), LineTotalAfterDiscount: money(720000),
            },
        ],
        Discounts: [
            { Id: uuid(), Source: 'Promotion', SourceId: 'promo-2', Description: 'Giảm 10% cho đơn hàng thời trang', Amount: money(125000) },
        ],
        StatusHistory: [
            { Id: uuid(), FromStatus: 'Draft', ToStatus: 'Pending', CreatedAt: '2026-02-13T08:00:00Z' },
            { Id: uuid(), FromStatus: 'Pending', ToStatus: 'Reserved', CreatedAt: '2026-02-13T08:01:00Z' },
            { Id: uuid(), FromStatus: 'Reserved', ToStatus: 'Confirmed', CreatedAt: '2026-02-13T08:05:00Z' },
        ],
        CreatedAt: '2026-02-13T08:00:00Z',
        UpdatedAt: '2026-02-13T08:05:00Z',
    },
]

// ===== Mock Promotions =====

const mockPromotions: ActivePromotionDto[] = [
    {
        Id: 'promo-active-1',
        Name: 'Flash Sale Tháng 2',
        Description: 'Giảm 15% cho tất cả sản phẩm thời trang',
        Type: 'DiscountByCategory',
        ActionDescription: 'Giảm 15% tối đa 300,000₫',
        EndDate: '2026-02-28T23:59:59Z',
    },
    {
        Id: 'promo-active-2',
        Name: 'Miễn phí vận chuyển',
        Description: 'Miễn phí vận chuyển cho đơn hàng từ 500,000₫',
        Type: 'DiscountByBrand',
        ActionDescription: 'Free ship cho đơn ≥ 500K',
        EndDate: '2026-03-15T23:59:59Z',
    },
]

// ===== Shipping Fee Calculator =====

const shippingFees: ShippingFeeDto[] = [
    { Method: 'Standard', Label: 'Tiêu chuẩn', Description: '3-5 ngày làm việc', Fee: money(15000), EstimatedDays: '3-5 ngày' },
    { Method: 'Fast', Label: 'Nhanh', Description: '1-2 ngày làm việc', Fee: money(25000), EstimatedDays: '1-2 ngày' },
    { Method: 'SuperFast', Label: 'Hỏa tốc', Description: 'Giao trong ngày', Fee: money(45000), EstimatedDays: 'Trong ngày' },
]

// ===== Mock API Functions =====

export async function getActivePromotions(): Promise<ActivePromotionDto[]> {
    await delay(300)
    return mockPromotions
}

export async function getShippingFees(): Promise<ShippingFeeDto[]> {
    await delay(200)
    return shippingFees
}

export async function getUserVouchers(): Promise<UserVoucherDto[]> {
    await delay(200)
    return [
        { Code: 'SALE200K', Description: 'Giảm 200,000₫ cho đơn từ 2 triệu', DiscountType: 'FixedAmount', DiscountValue: 200000, MinOrderAmount: 2000000, ExpiresAt: '2026-03-31T23:59:59Z' },
        { Code: 'GIAM10', Description: 'Giảm 10% tối đa 150,000₫', DiscountType: 'Percentage', DiscountValue: 10, MinOrderAmount: 500000, MaxDiscount: 150000, ExpiresAt: '2026-03-31T23:59:59Z' },
        { Code: 'FREESHIP', Description: 'Miễn phí vận chuyển', DiscountType: 'FixedAmount', DiscountValue: 0, MinOrderAmount: 0, ExpiresAt: '2026-04-30T23:59:59Z' },
        { Code: 'WELCOME50', Description: 'Giảm 50,000₫ cho khách hàng mới', DiscountType: 'FixedAmount', DiscountValue: 50000, MinOrderAmount: 200000, ExpiresAt: '2026-12-31T23:59:59Z' },
    ]
}

export async function applyVoucher(code: string, subTotal: number): Promise<ApplyVoucherResponse> {
    await delay(500)

    const vouchers: Record<string, { desc: string; percent?: number; fixed?: number; min: number; max?: number }> = {
        'SALE200K': { desc: 'Giảm 200,000₫ cho đơn từ 2 triệu', fixed: 200000, min: 2000000 },
        'GIAM10': { desc: 'Giảm 10% tối đa 150,000₫', percent: 10, min: 500000, max: 150000 },
        'FREESHIP': { desc: 'Miễn phí vận chuyển', fixed: 0, min: 0 },
        'WELCOME50': { desc: 'Giảm 50,000₫ cho khách hàng mới', fixed: 50000, min: 200000 },
    }

    const voucher = vouchers[code.toUpperCase()]
    if (!voucher) {
        return { Success: false, ErrorMessage: 'Mã voucher không hợp lệ hoặc đã hết hạn.' }
    }

    if (subTotal < voucher.min) {
        return { Success: false, ErrorMessage: `Đơn hàng tối thiểu ${voucher.min.toLocaleString('vi-VN')}₫ để áp dụng mã này.` }
    }

    let discountAmount = 0
    if (voucher.fixed !== undefined) {
        discountAmount = voucher.fixed
    } else if (voucher.percent !== undefined) {
        discountAmount = Math.round(subTotal * voucher.percent / 100)
        if (voucher.max && discountAmount > voucher.max) {
            discountAmount = voucher.max
        }
    }

    return {
        Success: true,
        CouponCode: code.toUpperCase(),
        Description: voucher.desc,
        DiscountAmount: money(discountAmount),
    }
}

export async function placeOrder(request: PlaceOrderRequest, cartItems: { ProductName: string; VariantName: string; Thumbnail?: string | null; UnitPrice: MoneyDto; Quantity: number; ProductId: string; VariantId: string; Sku: string }[]): Promise<{ success: boolean; order?: OrderDto; error?: string }> {
    await delay(1200)

    // Simulate random failure (10% chance)
    if (Math.random() < 0.1) {
        return {
            success: false,
            error: 'Không thể xử lý đơn hàng. Hệ thống thanh toán đang bảo trì. Vui lòng thử lại sau ít phút.',
        }
    }

    const subTotal = cartItems.reduce((sum, item) => sum + item.UnitPrice.Amount * item.Quantity, 0)
    const shippingFee = shippingFees.find(f => f.Method === request.ShippingMethod)?.Fee.Amount ?? 15000
    const grandTotal = subTotal + shippingFee

    const newOrder: OrderDto = {
        Id: uuid(),
        OrderNumber: genOrderNumber(),
        CustomerId: 'user-001',
        Status: 'Confirmed' as OrderStatus,
        ShippingAddress: request.ShippingAddress,
        PaymentMethod: request.PaymentMethod,
        PaymentStatus: request.PaymentMethod === 'Online' ? 'Paid' : 'Pending',
        PaidAt: request.PaymentMethod === 'Online' ? new Date().toISOString() : undefined,
        ShippingMethod: request.ShippingMethod,
        SubTotal: money(subTotal),
        ShippingFee: money(shippingFee),
        TotalDiscount: money(0),
        GrandTotal: money(grandTotal),
        CustomerNote: request.CustomerNote,
        Items: cartItems.map(item => ({
            Id: uuid(),
            ProductId: item.ProductId,
            VariantId: item.VariantId,
            Sku: item.Sku,
            ProductName: item.ProductName,
            VariantName: item.VariantName,
            Thumbnail: item.Thumbnail,
            UnitPrice: item.UnitPrice,
            Quantity: item.Quantity,
            DiscountAmount: money(0),
            LineTotal: money(item.UnitPrice.Amount * item.Quantity),
            LineTotalAfterDiscount: money(item.UnitPrice.Amount * item.Quantity),
        })),
        Discounts: [],
        StatusHistory: [
            { Id: uuid(), FromStatus: 'Draft', ToStatus: 'Pending', CreatedAt: new Date().toISOString() },
            { Id: uuid(), FromStatus: 'Pending', ToStatus: 'Reserved', CreatedAt: new Date().toISOString() },
            { Id: uuid(), FromStatus: 'Reserved', ToStatus: 'Confirmed', CreatedAt: new Date().toISOString() },
        ],
        CreatedAt: new Date().toISOString(),
        UpdatedAt: new Date().toISOString(),
    }

    mockOrders.unshift(newOrder)
    return { success: true, order: newOrder }
}

export async function getOrderById(id: string): Promise<OrderDto | null> {
    await delay(400)
    return mockOrders.find(o => o.Id === id) ?? null
}

export async function getOrderHistory(statusFilter?: OrderStatus): Promise<OrderSummaryDto[]> {
    await delay(400)

    let filtered = mockOrders
    if (statusFilter) {
        filtered = mockOrders.filter(o => o.Status === statusFilter)
    }

    return filtered.map(o => ({
        Id: o.Id,
        OrderNumber: o.OrderNumber,
        Status: o.Status,
        PaymentMethod: o.PaymentMethod,
        PaymentStatus: o.PaymentStatus,
        ShippingMethod: o.ShippingMethod,
        ItemCount: o.Items.length,
        FirstItemThumbnail: o.Items[0]?.Thumbnail,
        FirstItemName: o.Items[0]?.ProductName ?? '',
        GrandTotal: o.GrandTotal,
        CreatedAt: o.CreatedAt,
    }))
}

export async function cancelOrder(id: string, request: CancelOrderRequest): Promise<{ success: boolean; error?: string }> {
    await delay(600)

    const order = mockOrders.find(o => o.Id === id)
    if (!order) {
        return { success: false, error: 'Không tìm thấy đơn hàng.' }
    }

    if (order.Status === 'Delivered' || order.Status === 'Cancelled') {
        return { success: false, error: 'Không thể hủy đơn hàng đã giao hoặc đã hủy.' }
    }

    order.Status = 'Cancelled'
    order.CancellationReason = request.Reason
    order.StatusHistory.push({
        Id: uuid(),
        FromStatus: order.StatusHistory[order.StatusHistory.length - 1]?.ToStatus ?? 'Confirmed',
        ToStatus: 'Cancelled',
        Reason: request.Reason,
        CreatedAt: new Date().toISOString(),
    })
    order.UpdatedAt = new Date().toISOString()

    return { success: true }
}
