import { Link } from 'react-router'

function Checkout() {
    return (
        <section>
            <nav className="breadcrumb" style={{ marginBottom: '24px', color: 'var(--muted)', fontSize: '0.9rem' }}>
                <Link to="/">Trang chủ</Link> / <span>Thanh toán</span>
            </nav>
            <div className="empty-state">
                <h2>Thanh toán</h2>
                <p>Trang này đang được phát triển...</p>
            </div>
        </section>
    )
}

export default Checkout
