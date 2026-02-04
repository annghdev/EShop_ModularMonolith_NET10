import { Link } from 'react-router'

function PersonalProfile() {
    return (
        <section>
            <nav className="breadcrumb" style={{ marginBottom: '24px', color: 'var(--muted)', fontSize: '0.9rem' }}>
                <Link to="/">Trang chủ</Link> / <span>Hồ sơ cá nhân</span>
            </nav>
            <div className="empty-state">
                <h2>Hồ sơ cá nhân</h2>
                <p>Trang này đang được phát triển...</p>
                <p>Đăng nhập để xem và chỉnh sửa thông tin cá nhân.</p>
            </div>
        </section>
    )
}

export default PersonalProfile
