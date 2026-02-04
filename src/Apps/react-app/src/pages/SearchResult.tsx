import { Link, useSearchParams } from 'react-router'

function SearchResult() {
    const [searchParams] = useSearchParams()
    const query = searchParams.get('q') || ''

    return (
        <section>
            <nav className="breadcrumb" style={{ marginBottom: '24px', color: 'var(--muted)', fontSize: '0.9rem' }}>
                <Link to="/">Trang chủ</Link> / <span>Tìm kiếm</span>
            </nav>
            <div className="empty-state">
                <h2>Kết quả tìm kiếm</h2>
                {query ? (
                    <p>Từ khóa: "{query}"</p>
                ) : (
                    <p>Nhập từ khóa để tìm kiếm sản phẩm.</p>
                )}
                <p>Trang này đang được phát triển...</p>
            </div>
        </section>
    )
}

export default SearchResult
