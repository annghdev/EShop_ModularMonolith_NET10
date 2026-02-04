import { useParams, Link } from 'react-router'

function OrderDetails() {
    const { id } = useParams<{ id: string }>()

    return (
        <section>
            <nav className="breadcrumb" style={{ marginBottom: '24px', color: 'var(--muted)', fontSize: '0.9rem' }}>
                <Link to="/">Trang chủ</Link> / <Link to="/history">Lịch sử mua hàng</Link> / <span>Đơn hàng #{id}</span>
            </nav>
            <div className="empty-state">
                <h2>Chi tiết đơn hàng #{id}</h2>
                <p>Trang này đang được phát triển...</p>
            </div>
        </section>
    )
}

export default OrderDetails
