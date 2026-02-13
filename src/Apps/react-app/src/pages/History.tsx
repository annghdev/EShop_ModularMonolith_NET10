import { useState, useEffect } from 'react'
import { Link, useNavigate } from 'react-router'
import * as orderApi from '../services/orderMockApi'
import type { OrderSummaryDto, OrderStatus } from '../types/order'

const formatMoney = (amount: number) => amount.toLocaleString('vi-VN') + '₫'
const formatDate = (iso: string) => new Date(iso).toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' })

const STATUS_LABELS: Record<OrderStatus, { label: string; className: string; icon: string }> = {
    Draft: { label: 'Nháp', className: 'status-draft', icon: '📝' },
    Pending: { label: 'Chờ xử lý', className: 'status-pending', icon: '⏳' },
    Reserved: { label: 'Đã giữ hàng', className: 'status-reserved', icon: '📦' },
    Confirmed: { label: 'Đã xác nhận', className: 'status-confirmed', icon: '✅' },
    Shipped: { label: 'Đang giao', className: 'status-shipped', icon: '🚚' },
    Delivered: { label: 'Đã giao', className: 'status-delivered', icon: '✓' },
    Cancelled: { label: 'Đã hủy', className: 'status-cancelled', icon: '✕' },
    Refunded: { label: 'Đã hoàn tiền', className: 'status-refunded', icon: '↩' },
}

type TabFilter = 'all' | 'processing' | 'shipping' | 'delivered' | 'cancelled'

const TABS: { key: TabFilter; label: string }[] = [
    { key: 'all', label: 'Tất cả' },
    { key: 'processing', label: 'Đang xử lý' },
    { key: 'shipping', label: 'Đang giao' },
    { key: 'delivered', label: 'Đã giao' },
    { key: 'cancelled', label: 'Đã hủy' },
]

function getStatusFilter(tab: TabFilter): OrderStatus | undefined {
    switch (tab) {
        case 'processing': return 'Confirmed'
        case 'shipping': return 'Shipped'
        case 'delivered': return 'Delivered'
        case 'cancelled': return 'Cancelled'
        default: return undefined
    }
}

function History() {
    const navigate = useNavigate()
    const [orders, setOrders] = useState<OrderSummaryDto[]>([])
    const [loading, setLoading] = useState(true)
    const [activeTab, setActiveTab] = useState<TabFilter>('all')

    useEffect(() => {
        setLoading(true)
        void (async () => {
            const statusFilter = getStatusFilter(activeTab)
            const data = await orderApi.getOrderHistory(statusFilter)
            setOrders(data)
            setLoading(false)
        })()
    }, [activeTab])

    return (
        <section className="history-page">
            <nav className="breadcrumb">
                <Link to="/">Trang chủ</Link> / <span>Lịch sử mua hàng</span>
            </nav>

            <h1 className="history-title">Lịch sử mua hàng</h1>

            {/* Tabs */}
            <div className="history-tabs">
                {TABS.map(tab => (
                    <button
                        key={tab.key}
                        className={`history-tab${activeTab === tab.key ? ' active' : ''}`}
                        onClick={() => setActiveTab(tab.key)}
                        type="button"
                    >
                        {tab.label}
                    </button>
                ))}
            </div>

            {/* Order List */}
            {loading ? (
                <div className="order-loading"><div className="spinner" /><p>Đang tải danh sách đơn hàng...</p></div>
            ) : orders.length === 0 ? (
                <div className="empty-state">
                    <h2>📦 Chưa có đơn hàng nào</h2>
                    <p>
                        {activeTab === 'all'
                            ? 'Bạn chưa có đơn hàng nào. Hãy bắt đầu mua sắm!'
                            : `Không có đơn hàng nào ở trạng thái "${TABS.find(t => t.key === activeTab)?.label}".`}
                    </p>
                    <button className="btn-primary" onClick={() => navigate('/products')} type="button">🛍️ Mua sắm ngay</button>
                </div>
            ) : (
                <div className="history-list">
                    {orders.map(order => {
                        const statusCfg = STATUS_LABELS[order.Status]
                        return (
                            <div className="history-card" key={order.Id} onClick={() => navigate(`/history/${order.Id}`)}>
                                <div className="history-card-header">
                                    <span className="history-order-number">{order.OrderNumber}</span>
                                    <span className={`history-status ${statusCfg.className}`}>
                                        {statusCfg.icon} {statusCfg.label}
                                    </span>
                                </div>
                                <div className="history-card-body">
                                    <div className="history-item-preview">
                                        <img
                                            src={order.FirstItemThumbnail || 'https://picsum.photos/seed/placeholder/60/60'}
                                            alt={order.FirstItemName}
                                            className="history-thumb"
                                        />
                                        <div className="history-item-info">
                                            <span className="history-item-name">{order.FirstItemName}</span>
                                            {order.ItemCount > 1 && (
                                                <span className="history-item-more">và {order.ItemCount - 1} sản phẩm khác</span>
                                            )}
                                        </div>
                                    </div>
                                    <div className="history-card-meta">
                                        <span className="history-date">{formatDate(order.CreatedAt)}</span>
                                        <span className="history-total">{formatMoney(order.GrandTotal.Amount)}</span>
                                    </div>
                                </div>
                                <div className="history-card-footer">
                                    <span className="history-payment-badge">
                                        {order.PaymentMethod === 'Online' ? '💳 Online' : '💵 COD'}
                                    </span>
                                    <button className="btn-view-detail" type="button">
                                        Xem chi tiết →
                                    </button>
                                </div>
                            </div>
                        )
                    })}
                </div>
            )}
        </section>
    )
}

export default History
