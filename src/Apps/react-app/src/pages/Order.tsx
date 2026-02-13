import { useState, useEffect, useCallback, useMemo, useRef } from 'react'
import { useNavigate } from 'react-router'
import { useCart } from '../hooks/useCart'
import * as orderApi from '../services/orderMockApi'
import type { PlaceOrderRequest, AddressDto, PaymentMethod, PaymentProvider, ShippingMethod, ShippingFeeDto, ActivePromotionDto, ApplyVoucherResponse, OrderDto, UserVoucherDto } from '../types/order'

// ===== Helper =====
const formatMoney = (amount: number) => amount.toLocaleString('vi-VN') + '₫'

// ===== Sub-Components =====

function OrderItems({ items }: { items: ReturnType<typeof useCart>['cart'] extends infer C ? C extends { Items: infer I } ? I : never : never }) {
    if (!items || items.length === 0) return null
    return (
        <div className="order-items-section">
            <h3>Danh sách sản phẩm</h3>
            <div className="order-items-list">
                {items.map((item) => (
                    <div className="order-item" key={item.ItemId}>
                        <img
                            src={item.Thumbnail || 'https://picsum.photos/seed/placeholder/80/80'}
                            alt={item.ProductName}
                            className="order-item-thumb"
                        />
                        <div className="order-item-info">
                            <span className="order-item-name">{item.ProductName}</span>
                            <span className="order-item-variant">{item.VariantName}</span>
                            <span className="order-item-qty">x{item.Quantity}</span>
                        </div>
                        <div className="order-item-price">
                            {item.DiscountAmount.Amount > 0 && (
                                <span className="order-item-original">{formatMoney(item.OriginalPrice.Amount * item.Quantity)}</span>
                            )}
                            <span className="order-item-final">{formatMoney(item.LineTotal.Amount)}</span>
                        </div>
                    </div>
                ))}
            </div>
        </div>
    )
}

function DeliveryForm({ address, onChange }: { address: AddressDto; onChange: (a: AddressDto) => void }) {
    const update = (field: keyof AddressDto, value: string) => {
        onChange({ ...address, [field]: value })
    }
    return (
        <div className="delivery-form-section">
            <h3>Thông tin giao hàng</h3>
            <div className="delivery-form">
                <div className="form-row">
                    <div className="form-group">
                        <label htmlFor="fullName">Họ và tên *</label>
                        <input id="fullName" type="text" placeholder="Nguyễn Văn A" value={address.FullName} onChange={e => update('FullName', e.target.value)} />
                    </div>
                    <div className="form-group">
                        <label htmlFor="phone">Số điện thoại *</label>
                        <input id="phone" type="tel" placeholder="0901234567" value={address.Phone} onChange={e => update('Phone', e.target.value)} />
                    </div>
                </div>
                <div className="form-group">
                    <label htmlFor="street">Địa chỉ *</label>
                    <input id="street" type="text" placeholder="Số nhà, tên đường" value={address.Street} onChange={e => update('Street', e.target.value)} />
                </div>
                <div className="form-row form-row-3">
                    <div className="form-group">
                        <label htmlFor="ward">Phường/Xã</label>
                        <input id="ward" type="text" placeholder="Phường Bến Nghé" value={address.Ward} onChange={e => update('Ward', e.target.value)} />
                    </div>
                    <div className="form-group">
                        <label htmlFor="district">Quận/Huyện *</label>
                        <input id="district" type="text" placeholder="Quận 1" value={address.District} onChange={e => update('District', e.target.value)} />
                    </div>
                    <div className="form-group">
                        <label htmlFor="city">Tỉnh/Thành phố *</label>
                        <input id="city" type="text" placeholder="TP. Hồ Chí Minh" value={address.City} onChange={e => update('City', e.target.value)} />
                    </div>
                </div>
                <div className="form-group">
                    <label htmlFor="note">Ghi chú</label>
                    <textarea id="note" placeholder="Ghi chú cho người giao hàng..." value={address.Note ?? ''} onChange={e => update('Note', e.target.value)} rows={2} />
                </div>
            </div>
        </div>
    )
}

const PAYMENT_GATEWAYS: { value: PaymentProvider; label: string; icon: string }[] = [
    { value: 'VNPay', label: 'VNPay', icon: '🏦' },
    { value: 'Momo', label: 'Momo', icon: '💜' },
    { value: 'ZaloPay', label: 'ZaloPay', icon: '💙' },
    { value: 'BankTransfer', label: 'Chuyển khoản', icon: '🏧' },
]

function PaymentMethodSelector({ selected, onSelect, selectedGateway, onSelectGateway }: {
    selected: PaymentMethod; onSelect: (m: PaymentMethod) => void
    selectedGateway: PaymentProvider; onSelectGateway: (g: PaymentProvider) => void
}) {
    const methods: { value: PaymentMethod; label: string; icon: string; desc: string }[] = [
        { value: 'Online', label: 'Thanh toán Online', icon: '💳', desc: 'VNPay, Momo, ZaloPay' },
        { value: 'COD', label: 'Thanh toán khi nhận hàng', icon: '💵', desc: 'Thanh toán bằng tiền mặt' },
    ]
    return (
        <div className="method-section">
            <h4>💳 Phương thức thanh toán</h4>
            <div className="method-options">
                {methods.map(m => (
                    <div
                        key={m.value}
                        className={`method-option${selected === m.value ? ' active' : ''}`}
                        onClick={() => onSelect(m.value)}
                        role="button"
                        tabIndex={0}
                    >
                        <span className="method-icon">{m.icon}</span>
                        <div className="method-text">
                            <span className="method-label">{m.label}</span>
                            <span className="method-desc">{m.desc}</span>
                        </div>
                        <span className={`method-radio${selected === m.value ? ' checked' : ''}`} />
                        {m.value === 'Online' && selected === 'Online' && (
                            <div className="gateway-tags" onClick={e => e.stopPropagation()}>
                                {PAYMENT_GATEWAYS.map(g => (
                                    <button
                                        key={g.value}
                                        className={`gateway-tag${selectedGateway === g.value ? ' active' : ''}`}
                                        onClick={() => onSelectGateway(g.value)}
                                        type="button"
                                    >
                                        <span>{g.icon}</span> {g.label}
                                    </button>
                                ))}
                            </div>
                        )}
                    </div>
                ))}
            </div>
        </div>
    )
}

function ShippingMethodSelector({
    selected, onSelect, fees,
}: { selected: ShippingMethod; onSelect: (m: ShippingMethod) => void; fees: ShippingFeeDto[] }) {
    return (
        <div className="method-section">
            <h4>🚚 Phương thức vận chuyển</h4>
            <div className="method-options">
                {fees.map(f => (
                    <button
                        key={f.Method}
                        className={`method-option${selected === f.Method ? ' active' : ''}`}
                        onClick={() => onSelect(f.Method)}
                        type="button"
                    >
                        <div className="method-text">
                            <span className="method-label">{f.Label}</span>
                            <span className="method-desc">{f.Description} • {f.EstimatedDays}</span>
                        </div>
                        <span className="method-fee">{formatMoney(f.Fee.Amount)}</span>
                        <span className={`method-radio${selected === f.Method ? ' checked' : ''}`} />
                    </button>
                ))}
            </div>
        </div>
    )
}

function VoucherInput({ onApply, appliedVoucher, onRemove }: {
    onApply: (code: string) => void
    appliedVoucher: ApplyVoucherResponse | null
    onRemove: () => void
}) {
    const [code, setCode] = useState('')
    const [loading, setLoading] = useState(false)
    const [showDropdown, setShowDropdown] = useState(false)
    const [vouchers, setVouchers] = useState<UserVoucherDto[]>([])
    const wrapperRef = useRef<HTMLDivElement>(null)

    useEffect(() => {
        void (async () => {
            const list = await orderApi.getUserVouchers()
            setVouchers(list)
        })()
    }, [])

    // Close dropdown on click outside
    useEffect(() => {
        const handler = (e: MouseEvent) => {
            if (wrapperRef.current && !wrapperRef.current.contains(e.target as Node)) {
                setShowDropdown(false)
            }
        }
        document.addEventListener('mousedown', handler)
        return () => document.removeEventListener('mousedown', handler)
    }, [])

    const filtered = useMemo(() => {
        if (!code.trim()) return vouchers
        const q = code.toUpperCase()
        return vouchers.filter(v => v.Code.includes(q) || v.Description.toUpperCase().includes(q))
    }, [code, vouchers])

    const handleApply = async () => {
        if (!code.trim()) return
        setLoading(true)
        setShowDropdown(false)
        onApply(code.trim())
        setLoading(false)
    }

    const handleSelect = (v: UserVoucherDto) => {
        setCode(v.Code)
        setShowDropdown(false)
    }

    return (
        <div className="voucher-section" ref={wrapperRef}>
            <h4>🎟️ Mã giảm giá</h4>
            {appliedVoucher?.Success ? (
                <div className="voucher-applied">
                    <div className="voucher-tag">
                        <span className="voucher-code">{appliedVoucher.CouponCode}</span>
                        <span className="voucher-desc">{appliedVoucher.Description}</span>
                    </div>
                    <button className="voucher-remove" onClick={onRemove} type="button">✕</button>
                </div>
            ) : (
                <div className="voucher-autocomplete">
                    <div className="voucher-input-group">
                        <input
                            type="text"
                            className="voucher-input"
                            placeholder="Nhập mã voucher..."
                            value={code}
                            onChange={e => { setCode(e.target.value); setShowDropdown(true) }}
                            onFocus={() => setShowDropdown(true)}
                            onKeyDown={e => e.key === 'Enter' && handleApply()}
                        />
                        <button className="voucher-btn" onClick={handleApply} disabled={loading || !code.trim()} type="button">
                            {loading ? '...' : 'Áp dụng'}
                        </button>
                    </div>
                    {showDropdown && filtered.length > 0 && (
                        <div className="voucher-dropdown">
                            {filtered.map(v => (
                                <button
                                    key={v.Code}
                                    className="voucher-dropdown-item"
                                    onClick={() => handleSelect(v)}
                                    type="button"
                                >
                                    <span className="voucher-dropdown-code">{v.Code}</span>
                                    <span className="voucher-dropdown-desc">{v.Description}</span>
                                    {v.MinOrderAmount > 0 && (
                                        <span className="voucher-dropdown-min">Đơn tối thiểu {v.MinOrderAmount.toLocaleString('vi-VN')}₫</span>
                                    )}
                                </button>
                            ))}
                        </div>
                    )}
                </div>
            )}
            {appliedVoucher && !appliedVoucher.Success && (
                <p className="voucher-error">{appliedVoucher.ErrorMessage}</p>
            )}
        </div>
    )
}

function PromotionBanner({ promotions }: { promotions: ActivePromotionDto[] }) {
    if (promotions.length === 0) return null
    return (
        <div className="promo-banner-section">
            <h4>🔥 Khuyến mãi đang diễn ra</h4>
            {promotions.map(p => (
                <div className="promo-banner" key={p.Id}>
                    <span className="promo-name">{p.Name}</span>
                    <span className="promo-desc">{p.ActionDescription}</span>
                    <span className="promo-end">Đến {new Date(p.EndDate).toLocaleDateString('vi-VN')}</span>
                </div>
            ))}
        </div>
    )
}

function OrderResultModal({ result, onClose }: {
    result: { success: boolean; order?: OrderDto; error?: string }
    onClose: () => void
}) {
    const navigate = useNavigate()

    if (result.success && result.order) {
        return (
            <div className="modal-overlay order-result-overlay" onClick={onClose}>
                <div className="order-result-modal" onClick={e => e.stopPropagation()}>
                    <div className="result-success">
                        <div className="success-icon">✓</div>
                        <h2>Đặt hàng thành công!</h2>
                        <p className="result-order-number">Mã đơn hàng: <strong>{result.order.OrderNumber}</strong></p>
                        <p className="result-msg">Cảm ơn bạn đã mua hàng. Đơn hàng đang được xử lý.</p>
                        <div className="result-actions">
                            <button className="btn-primary" onClick={() => navigate('/products')} type="button">🛍️ Tiếp tục mua sắm</button>
                            <button className="btn-secondary" onClick={() => navigate('/')} type="button">🏠 Trang chủ</button>
                            <button className="btn-secondary" onClick={() => navigate(`/history/${result.order!.Id}`)} type="button">📋 Chi tiết đơn hàng</button>
                        </div>
                    </div>
                </div>
            </div>
        )
    }

    return (
        <div className="modal-overlay order-result-overlay" onClick={onClose}>
            <div className="order-result-modal" onClick={e => e.stopPropagation()}>
                <div className="result-fail">
                    <div className="fail-icon">✕</div>
                    <h2>Đặt hàng thất bại</h2>
                    <p className="result-error">{result.error}</p>
                    <div className="result-guide">
                        <h4>Hướng dẫn khắc phục:</h4>
                        <ul>
                            <li>Kiểm tra kết nối mạng và thử lại</li>
                            <li>Đảm bảo thông tin thanh toán chính xác</li>
                            <li>Thử phương thức thanh toán khác</li>
                        </ul>
                    </div>
                    <div className="result-support">
                        <p>Cần hỗ trợ? Liên hệ: <strong>1900-xxxx</strong> hoặc <strong>support@eshop.vn</strong></p>
                    </div>
                    <div className="result-actions">
                        <button className="btn-primary" onClick={onClose} type="button">Thử lại</button>
                        <button className="btn-secondary" onClick={() => navigate('/')} type="button">Trang chủ</button>
                    </div>
                </div>
            </div>
        </div>
    )
}

// ===== Main Order Page =====

function Order() {
    const navigate = useNavigate()
    const { cart, isLoading: cartLoading } = useCart()

    const [address, setAddress] = useState<AddressDto>({
        FullName: '', Phone: '', Street: '', Ward: '', District: '', City: '', Note: '',
    })
    const [paymentMethod, setPaymentMethod] = useState<PaymentMethod>('Online')
    const [paymentGateway, setPaymentGateway] = useState<PaymentProvider>('VNPay')
    const [shippingMethod, setShippingMethod] = useState<ShippingMethod>('Standard')
    const [shippingFees, setShippingFees] = useState<ShippingFeeDto[]>([])
    const [promotions, setPromotions] = useState<ActivePromotionDto[]>([])
    const [appliedVoucher, setAppliedVoucher] = useState<ApplyVoucherResponse | null>(null)
    const [customerNote, setCustomerNote] = useState('')
    const [isPlacing, setIsPlacing] = useState(false)
    const [orderResult, setOrderResult] = useState<{ success: boolean; order?: OrderDto; error?: string } | null>(null)

    useEffect(() => {
        void (async () => {
            const [fees, promos] = await Promise.all([
                orderApi.getShippingFees(),
                orderApi.getActivePromotions(),
            ])
            setShippingFees(fees)
            setPromotions(promos)
        })()
    }, [])

    const cartItems = cart?.Items ?? []
    const subTotal = useMemo(() => cartItems.reduce((s, item) => s + item.LineTotal.Amount, 0), [cartItems])

    const promotionDiscount = useMemo(() => {
        const flashSale = promotions.find(p => p.Id === 'promo-active-1')
        if (flashSale && subTotal > 0) {
            return Math.min(Math.round(subTotal * 0.15), 300000)
        }
        return 0
    }, [subTotal, promotions])

    const currentShippingFee = shippingFees.find(f => f.Method === shippingMethod)?.Fee.Amount ?? 0
    const voucherDiscount = appliedVoucher?.Success ? (appliedVoucher.DiscountAmount?.Amount ?? 0) : 0
    const grandTotal = Math.max(0, subTotal + currentShippingFee - voucherDiscount - promotionDiscount)

    const handleApplyVoucher = useCallback(async (code: string) => {
        const result = await orderApi.applyVoucher(code, subTotal)
        setAppliedVoucher(result)
    }, [subTotal])

    const handleRemoveVoucher = () => setAppliedVoucher(null)

    const isFormValid = address.FullName.trim() && address.Phone.trim() && address.Street.trim() && address.District.trim() && address.City.trim() && cartItems.length > 0

    const handlePlaceOrder = async () => {
        if (!isFormValid || isPlacing) return

        setIsPlacing(true)
        const request: PlaceOrderRequest = {
            ShippingAddress: address,
            PaymentMethod: paymentMethod,
            ShippingMethod: shippingMethod,
            VoucherCode: appliedVoucher?.CouponCode,
            CustomerNote: customerNote || undefined,
        }

        const result = await orderApi.placeOrder(request, cartItems.map(i => ({
            ProductId: i.ProductId,
            VariantId: i.VariantId,
            Sku: i.Sku,
            ProductName: i.ProductName,
            VariantName: i.VariantName,
            Thumbnail: i.Thumbnail,
            UnitPrice: i.UnitPrice,
            Quantity: i.Quantity,
        })))

        setOrderResult(result)
        setIsPlacing(false)
    }

    if (cartLoading) {
        return (
            <section className="order-page">
                <div className="order-loading">
                    <div className="spinner" />
                    <p>Đang tải giỏ hàng...</p>
                </div>
            </section>
        )
    }

    if (cartItems.length === 0 && !orderResult) {
        return (
            <section className="order-page">
                {/* <nav className="breadcrumb">
                    <Link to="/">Trang chủ</Link> / <span>Đặt hàng</span>
                </nav> */}
                <div className="empty-state">
                    <h2>🛒 Giỏ hàng trống</h2>
                    <p>Bạn chưa có sản phẩm nào trong giỏ hàng.</p>
                    <button className="btn-primary" onClick={() => navigate('/products')} type="button">Tiếp tục mua sắm</button>
                </div>
            </section>
        )
    }

    return (
        <section className="order-page">
            {/* <nav className="breadcrumb">
                <Link to="/">Trang chủ</Link> / <span>Đặt hàng</span>
            </nav> */}

            <h1 className="order-title">Đặt hàng</h1>

            <div className="order-layout">
                {/* Left column */}
                <div className="order-main">
                    <div className="order-info-card price-summary-card">
                        <DeliveryForm address={address} onChange={setAddress} />
                        <div className="customer-note-section">
                            <h3>📝 Ghi chú đơn hàng</h3>
                            <textarea
                                className="customer-note-input"
                                placeholder="Ghi chú cho đơn hàng (không bắt buộc)..."
                                value={customerNote}
                                onChange={e => setCustomerNote(e.target.value)}
                                rows={2}
                            />
                        </div>

                        <OrderItems items={cartItems} />
                        <div className="price-summary">
                            <div className="price-row">
                                <span>Tạm tính</span>
                                <span>{formatMoney(subTotal)}</span>
                            </div>
                            <div className="price-row">
                                <span>Phí vận chuyển</span>
                                <span>{formatMoney(currentShippingFee)}</span>
                            </div>
                            {promotionDiscount > 0 && (
                                <div className="price-row discount-row">
                                    <span>Giảm giá khuyến mãi</span>
                                    <span>-{formatMoney(promotionDiscount)}</span>
                                </div>
                            )}
                            {voucherDiscount > 0 && (
                                <div className="price-row discount-row">
                                    <span>Giảm giá voucher</span>
                                    <span>-{formatMoney(voucherDiscount)}</span>
                                </div>
                            )}
                            <div className="price-row total-row">
                                <span>Tổng cộng</span>
                                <span className="grand-total">{formatMoney(grandTotal)}</span>
                            </div>
                        </div>

                        <button
                            className="btn-place-order"
                            onClick={handlePlaceOrder}
                            disabled={!isFormValid || isPlacing}
                            type="button"
                        >
                            {isPlacing ? (
                                <><span className="spinner-small" /> Đang xử lý...</>
                            ) : (
                                `Đặt hàng (${formatMoney(grandTotal)})`
                            )}
                        </button>
                    </div>
                </div>

                {/* Right column - Methods */}
                <div className="order-sidebar">
                    <div className="order-summary-card">
                        {/* <h3>Chọn phương thức</h3> */}

                        <PaymentMethodSelector selected={paymentMethod} onSelect={setPaymentMethod} selectedGateway={paymentGateway} onSelectGateway={setPaymentGateway} />
                        <ShippingMethodSelector selected={shippingMethod} onSelect={setShippingMethod} fees={shippingFees} />
                        <VoucherInput onApply={handleApplyVoucher} appliedVoucher={appliedVoucher} onRemove={handleRemoveVoucher} />
                        <PromotionBanner promotions={promotions} />
                    </div>
                </div>
            </div>

            {/* Order Result Modal */}
            {orderResult && (
                <OrderResultModal
                    result={orderResult}
                    onClose={() => setOrderResult(null)}
                />
            )}
        </section>
    )
}

export default Order
