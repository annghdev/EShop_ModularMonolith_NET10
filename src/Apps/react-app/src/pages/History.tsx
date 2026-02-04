import { Link } from 'react-router'

function History() {
    return (
        <section>
            <nav className="breadcrumb" style={{ marginBottom: '24px', color: 'var(--muted)', fontSize: '0.9rem' }}>
                <Link to="/">Trang chủ</Link> / <span>Lịch sử mua hàng</span>
            </nav>
            <div className="empty-state">
                <h2>Lịch sử mua hàng</h2>
                <p>Trang này đang được phát triển...</p>
                <p>Đăng nhập để xem lịch sử đơn hàng của bạn.</p>
            </div>
        </section>
    )
}

export default History
