import { useState, useEffect, useCallback } from 'react'
import { useParams, Link, useNavigate } from 'react-router'
import * as orderApi from '../services/orderMockApi'
import type { OrderDto, OrderStatus } from '../types/order'

const formatMoney = (amount: number) => amount.toLocaleString('vi-VN') + '₫'
const formatDate = (iso: string) => new Date(iso).toLocaleString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' })

const STATUS_CONFIG: Record<OrderStatus, { label: string; color: string; icon: string }> = {
    Draft: { label: 'Nháp', color: 'var(--muted)', icon: '📝' },
    Pending: { label: 'Chờ xử lý', color: '#f5a623', icon: '⏳' },
    Reserved: { label: 'Đã giữ hàng', color: '#4a90d9', icon: '📦' },
    Confirmed: { label: 'Đã xác nhận', color: '#4a90d9', icon: '✅' },
    Shipped: { label: 'Đang giao hàng', color: '#f5a623', icon: '🚚' },
    Delivered: { label: 'Đã giao hàng', color: 'var(--accent-2)', icon: '✓' },
    Cancelled: { label: 'Đã hủy', color: 'var(--accent)', icon: '✕' },
    Refunded: { label: 'Đã hoàn tiền', color: '#9b59b6', icon: '↩' },
}

const CANCEL_REASONS = [
    'Đổi ý, không muốn mua nữa',
    'Tìm thấy giá tốt hơn ở nơi khác',
    'Đặt nhầm sản phẩm',
    'Thời gian giao hàng quá lâu',
    'Muốn thay đổi sản phẩm',
    'Khác',
]

function canCancelOrder(status: OrderStatus): boolean {
    return status !== 'Delivered' && status !== 'Cancelled' && status !== 'Refunded'
}

// ===== Cancel Modal =====

function CancelOrderModal({ onClose, onConfirm, isSubmitting }: {
    onClose: () => void
    onConfirm: (reason: string) => void
    isSubmitting: boolean
}) {
    const [reason, setReason] = useState('')
    const [selectedChip, setSelectedChip] = useState<string | null>(null)

    const handleChipClick = (chip: string) => {
        if (chip === 'Khác') {
            setSelectedChip(chip)
            setReason('')
        } else {
            setSelectedChip(chip)
            setReason(chip)
        }
    }

    const finalReason = selectedChip === 'Khác' ? reason : (selectedChip ?? reason)

    return (
        <div className="modal-overlay cancel-modal-overlay" onClick={onClose}>
            <div className="cancel-modal" onClick={e => e.stopPropagation()}>
                <div className="cancel-modal-header">
                    <h2>Hủy đơn hàng</h2>
                    <button className="modal-close" onClick={onClose} type="button">✕</button>
                </div>
                <div className="cancel-modal-body">
                    <p className="cancel-prompt">Vui lòng cho chúng tôi biết lý do bạn muốn hủy đơn hàng:</p>

                    <div className="reason-chips">
                        {CANCEL_REASONS.map(chip => (
                            <button
                                key={chip}
                                className={`reason-chip${selectedChip === chip ? ' active' : ''}`}
                                onClick={() => handleChipClick(chip)}
                                type="button"
                            >
                                {chip}
                            </button>
                        ))}
                    </div>

                    {(selectedChip === 'Khác' || (!selectedChip)) && (
                        <textarea
                            className="reason-input"
                            placeholder="Nhập lý do hủy đơn hàng..."
                            value={reason}
                            onChange={e => setReason(e.target.value)}
                            rows={3}
                        />
                    )}
                </div>
                <div className="cancel-modal-footer">
                    <button className="btn-secondary" onClick={onClose} type="button">Quay lại</button>
                    <button
                        className="btn-danger"
                        onClick={() => onConfirm(finalReason)}
                        disabled={!finalReason.trim() || isSubmitting}
                        type="button"
                    >
                        {isSubmitting ? 'Đang xử lý...' : 'Xác nhận hủy'}
                    </button>
                </div>
            </div>
        </div>
    )
}

// ===== Tracking Timeline =====

function TrackingTimeline({ order }: { order: OrderDto }) {
    const events = order.ShippingInfo?.TrackingEvents ?? []
    if (events.length === 0 && order.StatusHistory.length === 0) return null

    // If has shipping tracking events, show them; otherwise fallback to status history
    const timelineItems = events.length > 0
        ? events.map(e => ({ label: e.Description, time: e.Timestamp, location: e.Location, type: e.EventType })).reverse()
        : order.StatusHistory.map(h => ({
            label: `${STATUS_CONFIG[h.FromStatus]?.label ?? h.FromStatus} → ${STATUS_CONFIG[h.ToStatus]?.label ?? h.ToStatus}`,
            time: h.CreatedAt,
            location: h.Reason,
            type: h.ToStatus,
        })).reverse()

    return (
        <div className="tracking-section">
            <h3>📍 Theo dõi đơn hàng</h3>
            <div className="tracking-timeline">
                {timelineItems.map((item, idx) => (
                    <div
                        className={`tracking-step${idx === 0 ? ' current' : ''}`}
                        key={`${item.time}-${idx}`}
                    >
                        <div className="tracking-dot" />
                        <div className="tracking-content">
                            <span className="tracking-desc">{item.label}</span>
                            {item.location && <span className="tracking-location">📍 {item.location}</span>}
                            <span className="tracking-time">{formatDate(item.time)}</span>
                        </div>
                    </div>
                ))}
            </div>
        </div>
    )
}

// ===== Main OrderDetails Page =====

function OrderDetails() {
    const { id } = useParams<{ id: string }>()
    const navigate = useNavigate()
    const [order, setOrder] = useState<OrderDto | null>(null)
    const [loading, setLoading] = useState(true)
    const [showCancelModal, setShowCancelModal] = useState(false)
    const [isCancelling, setIsCancelling] = useState(false)
    const [cancelError, setCancelError] = useState<string | null>(null)

    useEffect(() => {
        if (!id) return
        setLoading(true)
        void (async () => {
            const data = await orderApi.getOrderById(id)
            setOrder(data)
            setLoading(false)
        })()
    }, [id])

    const handleCancel = useCallback(async (reason: string) => {
        if (!id) return
        setIsCancelling(true)
        setCancelError(null)
        const result = await orderApi.cancelOrder(id, { Reason: reason })
        if (result.success) {
            const updated = await orderApi.getOrderById(id)
            setOrder(updated)
            setShowCancelModal(false)
        } else {
            setCancelError(result.error ?? 'Không thể hủy đơn hàng.')
        }
        setIsCancelling(false)
    }, [id])

    if (loading) {
        return (
            <section className="order-detail-page">
                <div className="order-loading"><div className="spinner" /><p>Đang tải thông tin đơn hàng...</p></div>
            </section>
        )
    }

    if (!order) {
        return (
            <section className="order-detail-page">
                <nav className="breadcrumb">
                    <Link to="/">Trang chủ</Link> / <Link to="/history">Lịch sử mua hàng</Link> / <span>Không tìm thấy</span>
                </nav>
                <div className="empty-state">
                    <h2>Không tìm thấy đơn hàng</h2>
                    <p>Đơn hàng bạn tìm kiếm không tồn tại hoặc đã bị xóa.</p>
                    <button className="btn-primary" onClick={() => navigate('/history')} type="button">Quay lại lịch sử</button>
                </div>
            </section>
        )
    }

    const statusCfg = STATUS_CONFIG[order.Status]

    return (
        <section className="order-detail-page">
            <nav className="breadcrumb">
                <Link to="/">Trang chủ</Link> / <Link to="/history">Lịch sử mua hàng</Link> / <span>{order.OrderNumber}</span>
            </nav>

            {/* Header */}
            <div className="order-detail-header">
                <div className="order-detail-title">
                    <h1>Đơn hàng {order.OrderNumber}</h1>
                    <span className="order-date">Đặt ngày {formatDate(order.CreatedAt)}</span>
                </div>
                <span className="status-badge" style={{ background: statusCfg.color }}>
                    {statusCfg.icon} {statusCfg.label}
                </span>
            </div>

            <div className="order-detail-layout">
                {/* Left - Details */}
                <div className="order-detail-main">
                    {/* Items */}
                    <div className="order-info-card">
                        <h3>🛒 Sản phẩm</h3>
                        <div className="detail-items-list">
                            {order.Items.map(item => (
                                <div className="detail-item" key={item.Id}>
                                    <img
                                        src={item.Thumbnail || 'https://picsum.photos/seed/placeholder/80/80'}
                                        alt={item.ProductName}
                                        className="detail-item-thumb"
                                    />
                                    <div className="detail-item-info">
                                        <span className="detail-item-name">{item.ProductName}</span>
                                        <span className="detail-item-variant">{item.VariantName}</span>
                                        <span className="detail-item-meta">{formatMoney(item.UnitPrice.Amount)} × {item.Quantity}</span>
                                    </div>
                                    <div className="detail-item-total">
                                        {item.DiscountAmount.Amount > 0 && (
                                            <span className="detail-item-original">{formatMoney(item.LineTotal.Amount)}</span>
                                        )}
                                        <span className="detail-item-final">{formatMoney(item.LineTotalAfterDiscount.Amount)}</span>
                                    </div>
                                </div>
                            ))}
                        </div>
                    </div>

                    {/* Delivery Info */}
                    <div className="order-info-card">
                        <h3>📍 Thông tin giao hàng</h3>
                        <div className="info-grid">
                            <div className="info-item">
                                <span className="info-label">Người nhận</span>
                                <span className="info-value">{order.ShippingAddress.FullName}</span>
                            </div>
                            <div className="info-item">
                                <span className="info-label">SĐT</span>
                                <span className="info-value">{order.ShippingAddress.Phone}</span>
                            </div>
                            <div className="info-item full-width">
                                <span className="info-label">Địa chỉ</span>
                                <span className="info-value">
                                    {order.ShippingAddress.Street}, {order.ShippingAddress.Ward}, {order.ShippingAddress.District}, {order.ShippingAddress.City}
                                </span>
                            </div>
                        </div>
                    </div>

                    {/* Payment */}
                    <div className="order-info-card">
                        <h3>💳 Thanh toán</h3>
                        <div className="info-grid">
                            <div className="info-item">
                                <span className="info-label">Phương thức</span>
                                <span className="info-value">{order.PaymentMethod === 'Online' ? '💳 Thanh toán Online' : '💵 Thanh toán khi nhận hàng (COD)'}</span>
                            </div>
                            <div className="info-item">
                                <span className="info-label">Trạng thái</span>
                                <span className={`payment-status ${order.PaymentStatus.toLowerCase()}`}>
                                    {order.PaymentStatus === 'Paid' ? '✅ Đã thanh toán' : order.PaymentStatus === 'Pending' ? '⏳ Chờ thanh toán' : order.PaymentStatus === 'Refunded' ? '↩ Đã hoàn tiền' : '❌ Thất bại'}
                                </span>
                            </div>
                            {order.PaidAt && (
                                <div className="info-item">
                                    <span className="info-label">Thanh toán lúc</span>
                                    <span className="info-value">{formatDate(order.PaidAt)}</span>
                                </div>
                            )}
                        </div>
                    </div>

                    {/* Shipping Method */}
                    <div className="order-info-card">
                        <h3>🚚 Vận chuyển</h3>
                        <div className="info-grid">
                            <div className="info-item">
                                <span className="info-label">Phương thức</span>
                                <span className="info-value">
                                    {order.ShippingMethod === 'Standard' ? 'Tiêu chuẩn (3-5 ngày)' : order.ShippingMethod === 'Fast' ? 'Nhanh (1-2 ngày)' : 'Hỏa tốc (trong ngày)'}
                                </span>
                            </div>
                            {order.ShippingInfo?.TrackingNumber && (
                                <div className="info-item">
                                    <span className="info-label">Mã vận đơn</span>
                                    <span className="info-value tracking-number">{order.ShippingInfo.TrackingNumber}</span>
                                </div>
                            )}
                            {order.ShippingInfo?.Provider && (
                                <div className="info-item">
                                    <span className="info-label">Đơn vị vận chuyển</span>
                                    <span className="info-value">{order.ShippingInfo.Provider}</span>
                                </div>
                            )}
                        </div>
                    </div>

                    {/* Tracking */}
                    <div className="order-info-card">
                        <TrackingTimeline order={order} />
                    </div>
                </div>

                {/* Right - Summary */}
                <div className="order-detail-sidebar">
                    <div className="order-summary-card">
                        <h3>💰 Tổng thanh toán</h3>

                        {/* Discounts */}
                        {order.Discounts.length > 0 && (
                            <div className="applied-discounts">
                                <h4>🎟️ Giảm giá đã áp dụng</h4>
                                {order.Discounts.map(d => (
                                    <div className="discount-tag" key={d.Id}>
                                        <span className="discount-source">
                                            {d.Source === 'Coupon' ? `🎟️ ${d.SourceCode}` : d.Source === 'Promotion' ? '🔥 Khuyến mãi' : '✏️ Điều chỉnh'}
                                        </span>
                                        <span className="discount-desc">{d.Description}</span>
                                        <span className="discount-amount">-{formatMoney(d.Amount.Amount)}</span>
                                    </div>
                                ))}
                            </div>
                        )}

                        <div className="price-summary">
                            <div className="price-row">
                                <span>Tạm tính</span>
                                <span>{formatMoney(order.SubTotal.Amount)}</span>
                            </div>
                            <div className="price-row">
                                <span>Phí vận chuyển</span>
                                <span>{formatMoney(order.ShippingFee.Amount)}</span>
                            </div>
                            {order.TotalDiscount.Amount > 0 && (
                                <div className="price-row discount-row">
                                    <span>Tổng giảm giá</span>
                                    <span>-{formatMoney(order.TotalDiscount.Amount)}</span>
                                </div>
                            )}
                            <div className="price-row total-row">
                                <span>Tổng cộng</span>
                                <span className="grand-total">{formatMoney(order.GrandTotal.Amount)}</span>
                            </div>
                        </div>

                        {/* Cancel Reason */}
                        {order.CancellationReason && (
                            <div className="cancellation-info">
                                <h4>Lý do hủy</h4>
                                <p>{order.CancellationReason}</p>
                            </div>
                        )}

                        {/* Customer Note */}
                        {order.CustomerNote && (
                            <div className="customer-note-info">
                                <h4>Ghi chú</h4>
                                <p>{order.CustomerNote}</p>
                            </div>
                        )}

                        {/* Cancel Button */}
                        {canCancelOrder(order.Status) && (
                            <button className="btn-cancel-order" onClick={() => setShowCancelModal(true)} type="button">
                                Hủy đơn hàng
                            </button>
                        )}

                        {cancelError && <p className="cancel-error">{cancelError}</p>}
                    </div>
                </div>
            </div>

            {/* Cancel Modal */}
            {showCancelModal && (
                <CancelOrderModal
                    onClose={() => setShowCancelModal(false)}
                    onConfirm={handleCancel}
                    isSubmitting={isCancelling}
                />
            )}
        </section>
    )
}

export default OrderDetails
