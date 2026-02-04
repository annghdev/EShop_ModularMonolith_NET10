import { useParams, Link } from 'react-router'

function ProductDetails() {
    const { slug } = useParams<{ slug: string }>()

    return (
        <section>
            <nav className="breadcrumb" style={{ marginBottom: '24px', color: 'var(--muted)', fontSize: '0.9rem' }}>
                <Link to="/">Trang chủ</Link> / <Link to="/products">Sản phẩm</Link> / <span>{slug}</span>
            </nav>
            <div className="empty-state">
                <h2>Chi tiết sản phẩm</h2>
                <p>Slug: {slug}</p>
                <p>Trang này đang được phát triển...</p>
            </div>
        </section>
    )
}

export default ProductDetails
